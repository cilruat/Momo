using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Diagnostics;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Collections.Generic;

using Momo.Views;
using Momo.Models;

using Xamarin.Forms;
using Xamarin.Essentials;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Plugin.GoogleClient;
using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class SplashViewModel : BaseViewModel
    {
        private string _SplashImagePath;
        public string SplashImagePath
        {
            get => _SplashImagePath;
            set { SetProperty(ref _SplashImagePath, value); }
        }
        public ICommand SplashScreenCommand { get; set; }

        private IGoogleClientManager _googleService;

        public SplashViewModel()
        {
            string imgFile = "Splash.jpg";
            if (Device.RuntimePlatform == Device.UWP)
                imgFile = "Assets\\" + imgFile;

            SplashImagePath = imgFile;
            SplashScreenCommand = new Command(ExecuteSplashScreenCommand);

            _googleService = CrossGoogleClient.Current;
        }

        private async void ExecuteSplashScreenCommand(object obj)
        {
            Image splashImage;
            if (obj != null)
            {
                splashImage = obj as Image;

                await splashImage.FadeTo(0, 0);
                await splashImage.FadeTo(1, 700);
                await Task.Delay(300);
                await splashImage.FadeTo(0, 250);

                string readVersion = "";
                int check_version = 0;
                try
                {
                    Uri uri = new Uri(Common.UrlVersion);
                    using (WebClient request = new WebClient())
                    {
                        Stream data = await request.OpenReadTaskAsync(uri);
                        StreamReader reader = new StreamReader(data);
                        readVersion = reader.ReadToEnd();

                        int start = readVersion.IndexOf("<") + 1;
                        int end = readVersion.IndexOf(">");
                        readVersion = readVersion.Substring(start, end - start);

                        data.Close();
                        reader.Close();
                        request.Dispose();
                    }

                    check_version = int.Parse(readVersion);
                    if (Common.CurVersion < check_version)
                    {
                        string desc = "새로운 버전이 나왔습니다\n업데이트 하시겠습니까?\n\n" +
                                      "[현재 버전: " + Common.CurVersion + "]\n" +
                                      "[새로운 버전: " + readVersion + "]";
                        if (await UserDialogs.Instance.ConfirmAsync(desc, okText:"예", cancelText:"아니오"))
                        {
                            /*uri = new Uri(Common.UrlServer + "version/version_" + check_version + "/" + AppInfo.PackageName + ".apk");
                            DependencyService.Get<IInstallAPKService>().InstallAPKAsync(uri);*/
                            await Browser.OpenAsync(Common.MARKET_DOWN_LINK);
                        }
                        else
                            DependencyService.Get<ICloseAppService>().CloseApplication();

                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (Common.CurVersion != check_version)
                        await Common.ErrorAlertWithCloseApp(ex);
                    else
                        await Common.ErrorAlertWithMoveLogin(ex);

                    return;
                }

                await splashImage.FadeTo(0, 250);
                await AlreadyLoginCheck();
            }
        }

        private async Task AlreadyLoginCheck()
        {
            bool check_state = _googleService.IsLoggedIn;

#if DEBUG
            string get_phone_num = DependencyService.Get<IDeviceInfo>().GetPhoneNumber();
            //bool pass_google_login = get_phone_num == "01077932852";
            bool pass_google_login = true;

            if (Common.simul_mode || pass_google_login)
            {
                /*check_state =
                    string.IsNullOrEmpty(UserSettings.UserPhone) == false &&
                    string.IsNullOrEmpty(UserSettings.UserName) == false;*/
                check_state = true;
            }
#endif

            if (check_state)
            {
                try
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetFindPerson.php");
                    
                    var param = new Dictionary<string, string>();

                    string google_id = _googleService.IsLoggedIn ? _googleService.CurrentUser.Id : "";
                    string google_email = _googleService.IsLoggedIn ? _googleService.CurrentUser.Email : "";

                    param.Add("google_id", google_id);
                    param.Add("google_email", google_email);

                    string phone_num = DependencyService.Get<IDeviceInfo>().GetPhoneNumber();
#if DEBUG
                    if (Common.simul_mode)
                        phone_num = Common.TEST_PHONE_NUM;
#endif
                    param.Add("phone_num", phone_num);

                    var content = new FormUrlEncodedContent(param);

                    bool error = true;
                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (jsonResponse.StartsWith("null"))
                        {
                            if (_googleService.IsLoggedIn)
                            {
                                _googleService.OnLogout -= OnLogoutCompleted;
                                _googleService.Logout();
                            }
                            else
                                await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");

                            return;
                        }

                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                        Person person = new Person
                        {
                            Id = dicRes["p_id"],
                            PersonImage = dicRes["profile_url"],
                            PersonName = dicRes["person_name"],
                            Grade = dicRes["grade"],
                            PhoneNum = dicRes["phone_num"],
                            Etc = dicRes["etc"],
                            GoogleId = dicRes["google_id"],
                            GoogleEmail = dicRes["google_email"]
                        };

                        Common.MyInfo = person;
                        await DataPerson.UpdateItemAsync(person);

                        Background.Instance.GetPhoneList();                        

                        error = false;
                    }

                    if (error)
                    {
                        if (_googleService.IsLoggedIn)
                        {
                            _googleService.OnLogout -= OnLogoutCompleted;
                            _googleService.Logout();
                        }
                        else
                            await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Common.MyInfo.PersonName))
                            await Shell.Current.GoToAsync($"//{nameof(WriteMyInfoPage)}");
                        else
                            await Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");
                    }
                }
                catch (Exception ex)
                {
                    if (_googleService.IsLoggedIn)
                    {
                        _googleService.OnLogout -= OnLogoutCompleted;
                        _googleService.Logout();
                    }
                    else
                        await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");

                    return;
                }
            }
            else
                await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
        }

        private void OnLogoutCompleted(object sender, EventArgs args)
        {
            _googleService.OnLogout -= OnLogoutCompleted;
            Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
        }
    }
}
