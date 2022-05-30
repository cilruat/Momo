using System;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;
using Xamarin.Essentials;

using FFImageLoading.Forms;
using Acr.UserDialogs;
using Newtonsoft.Json;

using Plugin.Media;
using Plugin.Media.Abstractions;


namespace Momo.ViewModels
{
    public partial class NewGroupViewModel : BaseViewModel
    {
        static public List<string> listPhoneNum = new List<string>();

        public new Command BackCommand { get; }
        public Command SelectImage { get; }
        public Command SelectPersons { get; }
        public Command PrivateCommand { get; }
        public Command PublicCommand { get; }
        public Command ConfirmCommand { get; }

        private bool _isNew;
        private bool _isUpdate;

        private string _name;
        private string _frameBackCol;

        private bool _originFree;
        private string _originName;

        private CachedImage image;
        private Plugin.Media.Abstractions.MediaFile profile;
        
        private CheckBox checkPrivate;
        private CheckBox checkPublic;

        public NewGroupViewModel(CachedImage image, CheckBox checkPrivate, CheckBox checkPublic, bool update)
        {
            this.image = image;
            this.checkPrivate = checkPrivate;
            this.checkPublic = checkPublic;

            IsNew = !update;
            IsUpdate = update;

            BackCommand = new Command(OnBackCommand);
            SelectImage = new Command(OnSelectImage);
            SelectPersons = new Command(OnSelectPersons);
            PrivateCommand = new Command(OnPrivate);
            PublicCommand = new Command(OnPublic);
            ConfirmCommand = new Command(OnConfirm);

            FrameBackColor = "#eee";

            if (update)
                LoadInfo();
        }

        public bool IsNew
        {
            get => _isNew;
            set => SetProperty(ref _isNew, value);
        }

        public bool IsUpdate
        {
            get => _isUpdate;
            set => SetProperty(ref _isUpdate, value);
        }

        public string GroupName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string FrameBackColor
        {
            get => _frameBackCol;
            set => SetProperty(ref _frameBackCol, value);
        }

        private async void LoadInfo()
        {
            var group = await DataGroup.GetItemAsync(Common.ViewGroupID);
            if (group != null)
            {
                GroupName = group.Name;

                _originFree = group.Free;
                _originName = group.Name;

                if (_originFree)
                {
                    if (checkPrivate.IsChecked)
                        checkPrivate.IsChecked = false;

                    if (!checkPublic.IsChecked)
                        checkPublic.IsChecked = true;
                }

                Uri uri = new Uri(group.Image);
                image.Source = ImageSource.FromUri(uri);
            }
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
                image.Source = source;
                FrameBackColor = "Transparent";
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnSelectPersons()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            NewGroupAddPersonViewModel.EType = AddPersonType.PassOver;
            await Shell.Current.Navigation.PushModalAsync(new NewGroupAddPersonPage(listPhoneNum));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnConfirm()
        {
            if (CheckChangeInfo() == false)
            {
                await Shell.Current.Navigation.PopModalAsync(true);
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

                if (_isNew)
                {
                    strContent = new StringContent(Common.MyInfo.Id);
                    form.Add(strContent, "leader_id");
                }

                if (_isUpdate)
                {
                    strContent = new StringContent(Common.ViewGroupID);
                    form.Add(strContent, "groupId");
                }

                strContent = new StringContent(_name);
                form.Add(strContent, "name");

                if (_isNew)
                {
                    string strPersonNums = Common.MyInfo.PhoneNum;
                    if (listPhoneNum.Count > 0)
                        strPersonNums += ",";

                    for (int i = 0; i < listPhoneNum.Count; i++)
                    {
                        strPersonNums += listPhoneNum[i];

                        if (i < listPhoneNum.Count - 1)
                            strPersonNums += ",";
                    }

                    strContent = new StringContent(strPersonNums);
                    form.Add(strContent, "personNums");
                }

                string strFree = "0";
                if (checkPublic.IsChecked && !checkPrivate.IsChecked)
                    strFree = "1";

                strContent = new StringContent(strFree.ToString());
                form.Add(strContent, "free");

                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };

                string phpFile = _isNew ? "NewGroup.php" : "UpdateGroup.php";
                Uri uri = new Uri(Common.UrlServerPHP + phpFile);
                HttpResponseMessage response = await client.PostAsync(uri, form);

                var result = response.Content.ReadAsStringAsync().Result;
                if (string.IsNullOrEmpty(result))
                {
                    UserDialogs.Instance.HideLoading();

                    string errMsg = _isNew ? "모임 생성에 실패했습니다" : "모임 수정에 실패했습니다";
                    await UserDialogs.Instance.AlertAsync(errMsg, okText: "확인");
                    return;
                }

                Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                string reason = dicRes["reason"];
                if (dicRes["status"] == "Success")
                {
                    UserDialogs.Instance.HideLoading();

                    Group group = null;
                    if (_isNew)
                    {
                        string[] split = reason.Split(',');
                        group = new Group
                        {
                            Id = split[0],
                            LeaderId = Common.MyInfo.Id,
                            Name = _name,
                            Image = split[1],
                            Count = listPhoneNum.Count + 1,
                            Free = strFree == "1" ? true : false
                        };

                        bool isAccept = await UserDialogs.Instance.ConfirmAsync("모임이 생성되었습니다\n추가된 멤버들에게 문자를 보내시겠습니까?", "초대 문자 보내기", "보내기", "취소");
                        if (isAccept)
                        {
                            string desc = "'" + _name + "' 모임으로 초대되었습니다. '모모'가 설치되지 않으신분은 아래 링크를 눌러 설치하시면 됩니다.\n\n" + Common.HTTPS_DOWN_LINK;
                            var message = new SmsMessage(desc, listPhoneNum);
                            await Sms.ComposeAsync(message);
                        }
                    }
                    else
                        await UserDialogs.Instance.AlertAsync("모임 정보가 변경되었습니다", okText:"확인");

                    if (_isUpdate)
                    {
                        group = await DataGroup.GetItemAsync(Common.ViewGroupID);
                        group.Name = _name;
                        group.Free = strFree == "1" ? true : false;

                        if (profile != null)
                        {
                            group.Image = reason;
                            profile.Dispose();
                        }

                        GroupDetailViewModel.RefreshInfo = true;
                    }

                    await DataGroup.UpdateItemAsync(group);
                    await Shell.Current.Navigation.PopModalAsync(true);

                    if (_isNew)
                        base.OnGroupSelected(group);
                }
                else if (dicRes["status"] == "Bad")
                {
                    UserDialogs.Instance.HideLoading();
                    await UserDialogs.Instance.AlertAsync(reason, okText: "확인");
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private bool CheckChangeInfo()
        {
            bool change = profile != null || string.IsNullOrEmpty(_name) == false;
            if (_isUpdate)
            {
                bool isPublic = checkPublic.IsChecked;
                bool changeName = _originName != _name;
                change = profile != null || _originFree != isPublic || changeName;
            }

            return change;
        }

        private new async void OnBackCommand(object obj)
        {
            if (CheckChangeInfo())
            {
                bool isAccept = await UserDialogs.Instance.ConfirmAsync("변경된 사항을 저장하시겠습니까?", okText: "저장", cancelText: "나가기");
                if (isAccept)
                    OnConfirm();
                else
                    base.OnBackCommand(obj);
            }
            else
                base.OnBackCommand(obj);
        }

        public void OnAppearing() { }
        public void OnDisAppearing() { listPhoneNum.Clear(); }

        public void OnPrivate()
        {
            if (!checkPrivate.IsChecked)
                checkPrivate.IsChecked = true;

            if (checkPublic.IsChecked)
                checkPublic.IsChecked = false;
        }

        public void OnPublic()
        {
            if (checkPrivate.IsChecked)
                checkPrivate.IsChecked = false;

            if (!checkPublic.IsChecked)
                checkPublic.IsChecked = true;
        }
    }    
}
