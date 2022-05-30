using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;
using Xamarin.Essentials;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class PersonDetailViewModel : BaseViewModel
    {
        internal static PersonDetailViewModel Single { get; private set; }

        private string _userId;
        private string _userImage;
        private string _userName;
        private string _userPhone;
        private string _userEtc;
        private string _groupId;

        private bool isLeader;
        private bool isMyInfo;
        private bool isOtherInfo;

        public string UserImage
        {
            get => _userImage;
            set => SetProperty(ref _userImage, value);
        }

        public string UserName
        {
            get => _userName;
            set => SetProperty(ref _userName, value);
        }

        public string UserPhone
        {
            get => _userPhone;
            set => SetProperty(ref _userPhone, value);
        }

        public string UserEtc
        {
            get => _userEtc;
            set => SetProperty(ref _userEtc, value);
        }

        public bool IsLeader
        {
            get => isLeader;
            set
            {
                if (value == isLeader)
                    return;

                isLeader = value;
                OnPropertyChanged(nameof(IsLeader));
            }
        }

        public bool IsMyInfo
        {
            get => isMyInfo;
            set
            {
                if (value == isMyInfo)
                    return;

                isMyInfo = value;
                OnPropertyChanged(nameof(IsMyInfo));
            }
        }

        public bool IsOtherInfo
        {
            get => isOtherInfo;
            set
            {
                if (value == isOtherInfo)
                    return;

                isOtherInfo = value;
                OnPropertyChanged(nameof(IsOtherInfo));
            }
        }

        public Command ImageTabCommand { get; }
        public Command CallCommand { get; }
        public Command SendSMSCommand { get; }
        public Command SavePhoneCommand { get; }
        public Command StartChatCommand { get; }
        public Command MyInfoCommand { get; }
        public Command MyWriteCommand { get; }


        public PersonDetailViewModel(string userId, string groupId)
        {
            Single = this;

            UserId = userId;
            _groupId = groupId;

            SetLeader();

            IsMyInfo = Common.MyInfo.Id == userId;
            IsOtherInfo = Common.MyInfo.Id != userId;

            CallCommand = new Command(OnCallCommand);
            SendSMSCommand = new Command(OnSendSMSCommand);
            SavePhoneCommand = new Command(OnSavePhoneCommand);
            StartChatCommand = new Command(OnStartChatCommand);
            MyInfoCommand = new Command(OnMyInfoCommand);
            MyWriteCommand = new Command(OnMyWriteCommand);
            ImageTabCommand = new Command(OnImageTapped);
        }

        private async void SetLeader()
        {
            string groupId = string.IsNullOrEmpty(_groupId) ? Common.ViewGroupID : _groupId;
            Group group = await DataGroup.GetItemAsync(groupId);
            if (group != null)
                IsLeader = group.LeaderId == _userId;
        }

        private async void OnImageTapped()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new ImageDetailPage(_userImage));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnCallCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            try
            {
                _ = Shell.Current.Navigation.PopModalAsync(true);
                PhoneDialer.Open(_userPhone);
            }
            catch (ArgumentNullException)
            {
                await UserDialogs.Instance.AlertAsync("잘못된 전화번호입니다", okText: "확인");
            }
            catch (FeatureNotSupportedException)
            {
                await UserDialogs.Instance.AlertAsync("해당 기능을 사용할 수 없는 기종입니다", okText: "확인");
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnSendSMSCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            try
            {
                _ = Shell.Current.Navigation.PopModalAsync(true);

                var message = new SmsMessage("", _userPhone);
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await UserDialogs.Instance.AlertAsync("해당 기능을 사용할 수 없는 기종입니다", okText: "확인");
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnSavePhoneCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            try
            {
                _ = Shell.Current.Navigation.PopModalAsync(true);
                DependencyService.Get<ISaveContactService>().SaveContact(_userName, _userPhone);
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnStartChatCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            try
            {
                _ = Shell.Current.Navigation.PopModalAsync(true);

                string groupId = Common.ViewGroupID;
                if (string.IsNullOrEmpty(groupId))
                    groupId = _groupId;

                Group group = await DataGroup.GetItemAsync(groupId);
                if (group == null)
                {
                    Common.IsClickActioning = false;
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "CheckChatRoom.php");

                string room_name = _userId + ":" + _userName + "," + Common.MyInfo.Id + ":" + Common.MyInfo.PersonName;
                string person_ids = _userId + "," + Common.MyInfo.Id;
                string person_imgs = _userId + "=" + _userImage + "," + Common.MyInfo.Id + "=" + Common.MyInfo.PersonImage;

                var param = new Dictionary<string, string>
                {
                    { "room_name", room_name },
                    { "group_id", group.Id },
                    { "group_name", group.Name },
                    { "person_ids", person_ids },
                    { "person_imgs", person_imgs },
                    { "person_cnt", "2" }
                };

                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    string roomID = jsonResponse;
                    if (jsonResponse.Contains("\""))
                        roomID = jsonResponse.Replace("\"", "");

                    await Shell.Current.Navigation.PushModalAsync(new ChatDetailPage(roomID, room_name, _userId, group.Id));
                }
                else
                {
                    await Common.ErrorAlertWithPopModal();
                    Common.IsClickActioning = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                Common.IsClickActioning = false;
                return;
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnMyInfoCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new MyInfoDetailPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnMyWriteCommand(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            //await Shell.Current.Navigation.PushModalAsync(new MyWriteNoticePage());
            await UserDialogs.Instance.AlertAsync("준비중인 기능입니다", okText: "확인");
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        public string UserId
        {
            get { return _userId; }
            set
            {
                _userId = value;
                LoadUser(value);
            }
        }

        public async void LoadUser(string userId)
        {
            IsBusy = true;

            try
            {
                var data = await DataPerson.GetItemAsync(userId);
                if (data != null)
                {
                    if (string.IsNullOrEmpty(UserId) || UserId != data.Id)
                        UserId = data.Id;

                    UserImage = data.PersonImage;
                    UserName = data.PersonName;
                    UserPhone = data.PhoneNum;
                    UserEtc = data.Etc;
                }
                else
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetPersonInfo.php");

                    var param = new Dictionary<string, string> { { "person_ids", userId } };
                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (jsonResponse.StartsWith("null"))
                        {
                            await Common.ErrorAlertWithPopModal();
                            return;
                        }

                        JArray jArray = JArray.Parse(jsonResponse);
                        foreach(JObject e in jArray)
                        {
                            Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                            Person person = new Person
                            {
                                Id = dicRes["p_id"],
                                PersonImage = dicRes["profile_url"],
                                PersonName = dicRes["person_name"],
                                Grade = dicRes["grade"],
                                PhoneNum = dicRes["phone_num"],
                                Etc = dicRes["etc"]
                            };

                            UserImage = person.PersonImage;
                            UserName = person.PersonName;
                            UserPhone = person.PhoneNum;
                            UserEtc = person.Etc;

                            await DataPerson.UpdateItemAsync(person);
                            break;
                        }
                    }
                    else
                    {
                        await Common.ErrorAlertWithPopModal();
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }

            IsBusy = false;
        }

        public async void ChangeInfo()
        {
            var data = await DataPerson.GetItemAsync(Common.MyInfo.Id);
            if (data != null)
            {
                UserImage = data.PersonImage;
                UserName = data.PersonName;
                UserPhone = data.PhoneNum;
            }
        }
    }
}
