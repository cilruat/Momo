using System;
using System.Web;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

using Plugin.Media;
using Plugin.Media.Abstractions;

using Momo.Models;

using Xamarin.Forms;

using FFImageLoading.Forms;
using Newtonsoft.Json;
using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class MyInfoDetailViewModel : BaseViewModel
    {
        public new Command BackCommand { get; }
        public Command SelectImage { get; }
        public Command SaveCommand { get; }

        private string _image;
        private string _googleEmail;
        private string _name;
        private string _phone;
        private string _etc;
        private string _changeName;
        private string _changePhone;
        private string _changeEtc;

        private bool _isChangeInfo = false;

        private readonly CachedImage image;
        private Plugin.Media.Abstractions.MediaFile profile;

        public MyInfoDetailViewModel(CachedImage image)
        {
            this.image = image;

            BackCommand = new Command(OnBackCommand);
            SelectImage = new Command(OnSelectImage);
            SaveCommand = new Command(OnSaveInfo);

            LoadInfo();
        }

        public string MyImage
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public string MyGoogleEmail
        {
            get => _googleEmail;
            set => SetProperty(ref _googleEmail, value);
        }

        public string MyPhone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        public string MyName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string MyEtc
        {
            get => _etc;
            set => SetProperty(ref _etc, value);
        }

        public string ChangePhone
        {
            get => _changePhone;
            set
            {
                if (value == _changePhone)
                    return;

                _changePhone = value;
                _isChangeInfo = true;
                OnPropertyChanged(nameof(ChangePhone));
            }
        }

        public string ChangeName
        {
            get => _changeName;
            set
            {
                if (value == _changeName)
                    return;

                _changeName = value;
                _isChangeInfo = true;
                OnPropertyChanged(nameof(ChangeName));
            }
        }

        public string ChangeEtc
        {
            get => _changeEtc;
            set
            {
                if (value == _changeEtc)
                    return;

                _changeEtc = value;
                _isChangeInfo = true;
                OnPropertyChanged(nameof(ChangeEtc));
            }
        }

        private void LoadInfo()
        {
            if (Common.MyInfo == null)
                return;

            MyImage = Common.MyInfo.PersonImage;
            MyGoogleEmail = Common.MyInfo.GoogleEmail;
            MyPhone = Common.MyInfo.PhoneNum;
            MyName = Common.MyInfo.PersonName;
            MyEtc = Common.MyInfo.Etc;
        }

        private async void OnSelectImage()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await UserDialogs.Instance.AlertAsync("해당 기능을 사용할 수 없는 기종입니다", okText: "확인");
                Common.IsClickActioning = false;
                return;
            }

            profile = await CrossMedia.Current.PickPhotoAsync(new PickMediaOptions { PhotoSize = PhotoSize.Medium });
            if (profile == null)
            {
                Common.IsClickActioning = false;
                return;
            }
            
            ImageSource source = ImageSource.FromStream(() => profile.GetStream());            

            if (image != null)
            {
                _isChangeInfo = true;
                image.Source = source;
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnSaveInfo()
        {
            if (_isChangeInfo == false)
                return;

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                if (profile != null)
                {
                    string filePath = profile.Path;
                    int cutIdx = filePath.LastIndexOf('/') + 1;
                    string fileName = filePath.Substring(cutIdx);

                    fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);

                    StreamContent content = new StreamContent(profile.GetStream());
                    content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "fileToUpload",
                        FileName = fileName
                    };

                    form.Add(content);
                }

                string existImg = profile != null ? "true" : "false";
                StringContent strContent = new StringContent(existImg);
                form.Add(strContent, "existImg");

                string my_id = Common.MyInfo.Id;
                strContent = new StringContent(my_id);
                form.Add(strContent, "id");

                string change_name = string.IsNullOrEmpty(_changeName) ? _name : _changeName;
                strContent = new StringContent(change_name);
                form.Add(strContent, "name");

                string change_phone = string.IsNullOrEmpty(_changePhone) ? _phone : _changePhone;
                strContent = new StringContent(change_phone);
                form.Add(strContent, "phone");

                string change_etc = string.IsNullOrEmpty(_changeEtc) ? _etc : _changeEtc;
                strContent = new StringContent(change_etc);
                form.Add(strContent, "etc");

                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };

                Uri uri = new Uri(Common.UrlServerPHP + "UploadProfile.php");
                HttpResponseMessage response = await client.PostAsync(uri, form);
                    
                var result = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                string reason = dicRes["reason"];
                if (dicRes["status"] == "Success")
                {
                    if (profile != null)
                    {
                        Common.MyInfo.PersonImage = reason;

                        var notices = await DataNotice.GetItemsAsync();
                        foreach (Notice n in notices)
                        {
                            if (n.PersonId != my_id)
                                continue;

                            n.PersonImage = reason;
                        }

                        profile.Dispose();
                    }

                    Common.MyInfo.PersonName = change_name;
                    Common.MyInfo.PhoneNum = change_phone;
                    Common.MyInfo.Etc = change_etc;

                    _isChangeInfo = false;
                    await DataPerson.UpdateItemAsync(Common.MyInfo);

                    await Task.Delay(300);
                    UserDialogs.Instance.HideLoading();

                    await UserDialogs.Instance.AlertAsync("내 정보가 수정되었습니다", okText: "확인");

                    if (TapSettingsViewModel.Single != null)
                        TapSettingsViewModel.Single.LoadInfo();

                    if (PersonDetailViewModel.Single != null)
                        PersonDetailViewModel.Single.ChangeInfo();

                    await Shell.Current.Navigation.PopModalAsync(true);
                }
                else if (dicRes["status"] == "Bad")
                {
                    UserDialogs.Instance.HideLoading();
                    await UserDialogs.Instance.AlertAsync(reason, okText: "확인");
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                UserDialogs.Instance.HideLoading();
                return;
            }
        }

        private new async void OnBackCommand(object obj)
        {
            if (_isChangeInfo)
            {
                bool isAccept = await UserDialogs.Instance.ConfirmAsync("변경된 사항을 저장하시겠습니까?", okText: "저장", cancelText: "나가기");
                if (isAccept)
                    OnSaveInfo();
                else
                {
                    _isChangeInfo = false;
                    base.OnBackCommand(obj);
                }
            }
            else
                base.OnBackCommand(obj);
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
