using System;
using System.Net.Http;
using System.Collections.Generic;

using Momo.Views;
using Momo.Models;
using Momo.Services;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

using Plugin.GoogleClient;
using Plugin.GoogleClient.Shared;


namespace Momo.ViewModels
{
    public class LoginCheckViewModel : BaseViewModel
    {
        public Command LoginCommand { get; }
        public Command JoinCommand { get; }

        private IGoogleClientManager _googleService;

        public LoginCheckViewModel()
        {
            LoginCommand = new Command(OnLoginClicked);
            JoinCommand = new Command(OnJoinClicked);

            _googleService = CrossGoogleClient.Current;
        }

        private void OnLoginClicked(object obj)
        {
#if DEBUG
            string get_phone_num = DependencyService.Get<IDeviceInfo>().GetPhoneNumber();
            //bool pass_google_login = get_phone_num == "01077932852";
            bool pass_google_login = true;

            if (Common.simul_mode || pass_google_login)
            {
                AlreadyCheckLogin(new Person());
                //Shell.Current.GoToAsync($"{nameof(LoginPage)}");
            }                
            else
#endif
            {
                switch (Device.RuntimePlatform)
                {
                    case "Android": GoogleLogin(); break;
                    case "iOS": break;
                }
            }
        }

        private async void GoogleLogin()
        {
            if (_googleService.IsLoggedIn)
                _googleService.Logout();

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
            _googleService.OnLogin += OnLoginCompleted;

            try
            {
                await _googleService.LoginAsync();
            }
            catch (GoogleClientSignInNetworkErrorException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(e.Message, okText:"확인");
            }
            catch (GoogleClientSignInCanceledErrorException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync("구글 로그인을 취소하였습니다", okText: "확인");
            }
            catch (GoogleClientSignInInvalidAccountErrorException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(e.Message, okText: "확인");
            }
            catch (GoogleClientSignInInternalErrorException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(e.Message, okText: "확인");
            }
            catch (GoogleClientNotInitializedErrorException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(e.Message, okText: "확인");
            }
            catch (GoogleClientBaseException e)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(e.Message, okText: "확인");
            }
        }

        private async void OnLoginCompleted(object sender, GoogleClientResultEventArgs<GoogleUser> loginEventArgs)
        {
            if (loginEventArgs.Data != null)
            {
                GoogleUser googleUser = loginEventArgs.Data;

                Person person = new Person
                {
                    GoogleId = googleUser.Id,
                    GoogleEmail = googleUser.Email
                };

                AlreadyCheckLogin(person);
            }
            else
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync(loginEventArgs.Message, okText: "확인");
            }

            _googleService.OnLogin -= OnLoginCompleted;
        }

        private async void AlreadyCheckLogin(Person person)
        {
            bool error = false;
            Exception error_ex = null;
            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetFindPerson.php");

                string phone_num = DependencyService.Get<IDeviceInfo>().GetPhoneNumber();
#if DEBUG
                if (Common.simul_mode)
                    phone_num = Common.TEST_PHONE_NUM;
#endif

                var param = new Dictionary<string, string> 
                {
                    { "google_id", person.GoogleId },
                    { "google_email", person.GoogleEmail },
                    { "phone_num", phone_num }
                };

                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                        error = true;
                    else
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                        person.Id = dicRes["p_id"];
                        person.PersonImage = dicRes["profile_url"];
                        person.PersonName = dicRes["person_name"];
                        person.Grade = dicRes["grade"];
                        person.PhoneNum = dicRes["phone_num"];
                        person.Etc = dicRes["etc"];

                        Common.MyInfo = person;
                        await DataPerson.UpdateItemAsync(person);

                        UserSettings.UserPhone = person.PhoneNum;
                        UserSettings.UserName = person.PersonName;

                        Background.Instance.GetPhoneList();
                    }
                }
                else
                    error = true;
            }
            catch (Exception ex)
            {
                error = true;
                error_ex = ex;
            }

            if (error)
            {
                UserDialogs.Instance.HideLoading();

                if (_googleService.IsLoggedIn)
                    _googleService.Logout();

                if (Common.OP)
                {
                    string errorMsg = "";
                    while (error_ex != null)
                    {
                        errorMsg += error_ex.Message + "\n";
                        error_ex = error_ex.InnerException;
                    }

                    if (string.IsNullOrEmpty(errorMsg) == false)
                        await UserDialogs.Instance.AlertAsync(errorMsg, "알림", "확인");
                }

                string desc = "로그인에 실패했습니다\n다시 로그인해주세요";
                await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");
            }
            else
            {
                if (string.IsNullOrEmpty(person.PersonName))
                    await Shell.Current.GoToAsync($"//{nameof(WriteMyInfoPage)}");
                else
                    await Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");

                UserDialogs.Instance.HideLoading();
            }            
        }

        private async void OnJoinClicked(object obj)
        {
            await Shell.Current.GoToAsync($"{nameof(JoinPage)}");
        }
    }
}
