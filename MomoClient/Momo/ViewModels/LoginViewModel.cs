using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using System.Windows.Input;

using Momo.Views;
using Momo.Models;

using Xamarin.Forms;

using Newtonsoft.Json;
using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        public ICommand AuthenticateCommand { get; set; }

        private string _userphone;
        private string _username;
        private bool _areCredentialsInvalid;

        public LoginViewModel(Page page)
        {
            AuthenticateCommand = new Command(async () =>
            {
                int n_phone_num = -1;
                if (int.TryParse(_userphone, out n_phone_num) == false)
                {
                    await UserDialogs.Instance.AlertAsync("휴대폰 번호는 숫자만 입력해주세요", okText: "확인");                    
                    return;
                }

                if (string.IsNullOrEmpty(_userphone) || string.IsNullOrEmpty(_username))
                {
                    await UserDialogs.Instance.AlertAsync("내용을 입력해주세요", okText: "확인");
                    return;
                }                    

                try
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "LoginCheck.php");

                    var param = new Dictionary<string, string>
                    {
                        { "phone", _userphone },
                        { "name", _username }
                    };

                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (jsonResponse.StartsWith("null"))
                        {
                            IsBusy = false;
                            await UserDialogs.Instance.AlertAsync("전화번호 또는 이름을 정확히 입력하여 주십시오", okText: "확인");
                            return;
                        }

                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                        string phone_num = dicRes["phone_num"];
                        string name = dicRes["person_name"];

                        if (phone_num == _userphone && name == _username)
                        {
                            Person person = new Person
                            {
                                Id = dicRes["p_id"],
                                PersonImage = dicRes["profile_url"],
                                PersonName = name,
                                Grade = dicRes["grade"],
                                PhoneNum = phone_num,
                                Etc = dicRes["etc"],
                                GoogleId = dicRes["google_id"],
                                GoogleEmail = dicRes["google_email"]
                            };

                            Common.MyInfo = person;

                            await DataPerson.UpdateItemAsync(person);

                            UserSettings.UserPhone = person.PhoneNum;
                            UserSettings.UserName = person.PersonName;

                            Background.Instance.GetPhoneList();
                        }
                        else
                        {
                            AreCredentialsInvalid = true;
                            return;
                        }
                    }
                    else
                    {
                        await Common.ErrorAlertWithMoveLogin();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await Common.ErrorAlertWithMoveLogin(ex);
                    return;
                }

                Shell.Current.Navigation.RemovePage(page);
                await Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");
            });

            AreCredentialsInvalid = false;
        }
        
        public string UserPhone
        {
            get => _userphone;
            set
            {
                if (value == _userphone)
                    return;

                _userphone = value;
                OnPropertyChanged(nameof(UserPhone));
            }
        }

        public string UserName
        {
            get => _username;
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public bool AreCredentialsInvalid
        {
            get => _areCredentialsInvalid;
            set
            {
                if (value == _areCredentialsInvalid)
                    return;

                _areCredentialsInvalid = value;
                OnPropertyChanged(nameof(AreCredentialsInvalid));
            }
        }
    }
}
