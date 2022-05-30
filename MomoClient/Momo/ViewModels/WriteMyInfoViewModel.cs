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
using Momo.Views;

using Xamarin.Forms;

using FFImageLoading.Forms;
using Newtonsoft.Json;
using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class WriteMyInfoViewModel : BaseViewModel
    {
        public new Command BackCommand { get; }
        public Command SelectImage { get; }
        public Command SaveCommand { get; }

        private bool _isClickSave;
        private string _image;
        private string _name;
        private string _etc;

        private readonly CachedImage image;
        private Plugin.Media.Abstractions.MediaFile profile;

        public WriteMyInfoViewModel(CachedImage image)
        {
            this.image = image;

            SelectImage = new Command(OnSelectImage);
            SaveCommand = new Command(OnSaveInfo);

            MyImage = "Icon_profile.png";
        }

        public bool IsClickSave
        {
            get => _isClickSave;
            set => SetProperty(ref _isClickSave, value);
        }

        public string MyImage
        {
            get => _image;
            set => SetProperty(ref _image, value);
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
                image.Source = source;

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnSaveInfo()
        {
            if (string.IsNullOrEmpty(_name))
            {
                await UserDialogs.Instance.AlertAsync("이름을 입력해주세요", okText:"확인");
                return;
            }

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
                
                strContent = new StringContent(_name);
                form.Add(strContent, "name");

                string existEtc = _etc != null ? _etc : "";
                strContent = new StringContent(existEtc);
                form.Add(strContent, "etc");

                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };

                Uri uri = new Uri(Common.UrlServerPHP + "WriteProfile.php");
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

                    Common.MyInfo.PersonName = _name;
                    Common.MyInfo.Etc = _etc;

                    await DataPerson.UpdateItemAsync(Common.MyInfo);

                    await Task.Delay(300);
                    UserDialogs.Instance.HideLoading();

                    await Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");
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

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
