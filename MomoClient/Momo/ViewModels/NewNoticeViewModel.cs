using System;
using System.IO;
using System.Text;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;

using Xamarin.Forms;

using Plugin.Media;
using Acr.UserDialogs;

using Newtonsoft.Json;

namespace Momo.ViewModels
{
    public class NewNoticeViewModel : BaseViewModel
    {
        public ObservableCollection<MediaFile> Media { get; set; }
        public Command ConfirmCommand { get; }
        public Command TouchCommand { get; }
        public Command AttachPictureCommand { get; }
        public Command AttachMapCommand { get; }        

        public Editor InputEditor { get; set; }


        private string _pageTitle;
        private string _groupName;
        private string _newNotice;

        private bool _update;
        private string _updateNoticeId;
        private string _updateNoticeDesc;
        private string _updateCombineImgPath;

        private Group group;
        private List<MediaFile> listOriginFile = new List<MediaFile>();

        public NewNoticeViewModel(string groupId, bool update, string updateNoticeId)
        {
            _update = update;
            _updateNoticeId = updateNoticeId;            

            Media = new ObservableCollection<MediaFile>();

            LoadGroup(groupId);

            ConfirmCommand = new Command(OnNewNotice);
            TouchCommand = new Command(OnTouch);
            AttachPictureCommand = new Command(OnAttachPicture);
            AttachMapCommand = new Command(OnAttachMap);
        }

        private async void OnNewNotice()
        {
            if (IsBusy || group == null || Common.MyInfo == null)
                return;

            int attach_image_cnt = 0;
            string combineImgPath = "";            

            IEnumerator<MediaFile> list = Media.GetEnumerator();
            while (list.MoveNext())
            {
                combineImgPath += list.Current.Path;
                attach_image_cnt++;
            }                

            if (string.IsNullOrEmpty(combineImgPath))
            {
                if (string.IsNullOrEmpty(_newNotice))
                {
                    await UserDialogs.Instance.AlertAsync("내용을 입력해주세요", okText: "확인");
                    return;
                }
            }

            if (attach_image_cnt > 10)
            {
                UserDialogs.Instance.HideLoading();
                await UserDialogs.Instance.AlertAsync("사진은 10개 이하로만 업로드할 수 있습니다");
                return;
            }            

            if (_updateNoticeDesc == _newNotice && _updateCombineImgPath == combineImgPath)
            {
                await Shell.Current.Navigation.PopModalAsync(true);
                return;
            }

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };
                MultipartFormDataContent form = new MultipartFormDataContent();

                IEnumerator<MediaFile> list_send = Media.GetEnumerator();

                if (_update)
                {
                    string update_attach_urls = "";
                    int update_attach_cnt = 0;
                    while (list_send.MoveNext())
                    {
                        MediaFile m = list_send.Current;

                        MediaFile find = listOriginFile.Find(x => x.Id == m.Id);
                        if (find != null)
                        {
                            listOriginFile.Remove(find);

                            string path = m.Path.Replace(Common.UrlServer, "../");
                            if (string.IsNullOrEmpty(update_attach_urls) == false)
                                update_attach_urls += ",";

                            if (update_attach_cnt < 4)
                                update_attach_urls += path;

                            update_attach_cnt++;
                        }
                        else
                        {
                            string filePath = m.Path;
                            int cutIdx = filePath.LastIndexOf('/') + 1;
                            string fileName = filePath.Substring(cutIdx);

                            fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);

                            byte[] buff = File.ReadAllBytes(filePath);
                            MemoryStream ms = new MemoryStream(buff);

                            StreamContent content = new StreamContent(ms);
                            content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                            {
                                Name = "fileToUpload[]",
                                FileName = fileName
                            };

                            form.Add(content);
                        }
                    }

                    StringContent strContent = new StringContent(group.Id);
                    form.Add(strContent, "group_id");

                    strContent = new StringContent(_updateNoticeId);
                    form.Add(strContent, "notice_id");

                    strContent = new StringContent(_newNotice);
                    form.Add(strContent, "description");

                    strContent = new StringContent(((int)MediaFileType.Image).ToString());
                    form.Add(strContent, "attach_type");

                    strContent = new StringContent(attach_image_cnt.ToString());
                    form.Add(strContent, "attach_image_cnt");

                    strContent = new StringContent(update_attach_urls);
                    form.Add(strContent, "update_attach_urls");

                    string remove_img_ids = "", remove_imgs = "";
                    for(int i = 0; i < listOriginFile.Count; i++)
                    {
                        remove_img_ids += listOriginFile[i].Id;
                        remove_imgs += listOriginFile[i].Path.Replace(Common.UrlServer, "../");

                        if (i < listOriginFile.Count - 1)
                        {
                            remove_img_ids += ",";
                            remove_imgs += ",";
                        }
                    }

                    strContent = new StringContent(remove_img_ids);
                    form.Add(strContent, "remove_img_ids");

                    strContent = new StringContent(remove_imgs);
                    form.Add(strContent, "remove_images");
                }
                else
                {
                    while (list_send.MoveNext())
                    {
                        string filePath = list_send.Current.Path;
                        int cutIdx = filePath.LastIndexOf('/') + 1;
                        string fileName = filePath.Substring(cutIdx);

                        fileName = HttpUtility.UrlEncode(fileName, Encoding.UTF8);

                        byte[] buff = File.ReadAllBytes(filePath);
                        MemoryStream ms = new MemoryStream(buff);

                        StreamContent content = new StreamContent(ms);
                        content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = "fileToUpload[]",
                            FileName = fileName
                        };

                        form.Add(content);
                    }

                    StringContent strContent = new StringContent(group.Id);
                    form.Add(strContent, "group_id");

                    strContent = new StringContent(group.Name);
                    form.Add(strContent, "group_name");

                    strContent = new StringContent(Common.MyInfo.Id);
                    form.Add(strContent, "person_id");

                    strContent = new StringContent(Common.MyInfo.PersonName);
                    form.Add(strContent, "person_name");

                    strContent = new StringContent(_newNotice);
                    form.Add(strContent, "description");

                    strContent = new StringContent(((int)MediaFileType.Image).ToString());
                    form.Add(strContent, "attach_type");

                    strContent = new StringContent(attach_image_cnt.ToString());
                    form.Add(strContent, "attach_image_cnt");
                }

                string sendPHP = _update ? "UpdateNotice.php" : "NewNotice.php";
                Uri uri = new Uri(Common.UrlServerPHP + sendPHP);
                HttpResponseMessage response = await client.PostAsync(uri, form);

                var result = response.Content.ReadAsStringAsync().Result;
                Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                bool success = dicRes["status"] == "Success" ? true : false;
                if (_update == false)
                {
                    TapNoticesViewModel.RefreshNotice = success;
                    GroupDetailViewModel.RefreshNotice = success;
                }

                string strSuccess = "";
                if (success)
                    strSuccess = _update ? "글이 수정되었습니다" : "새로운 글이 등록되었습니다";
                else
                {
                    strSuccess = _update ? "글을 수정하는데 실패하였습니다\n다시 작성해주세요" : "새로운 글을 등록하는데 실패하였습니다\n다시 작성해주세요";

                    string reason = dicRes["reason"];
                    if (string.IsNullOrEmpty(reason) == false)
                        strSuccess += "\n[" + reason + "]";
                }

                if (_update)
                    await Task.Delay(300);

                UserDialogs.Instance.HideLoading();

                await UserDialogs.Instance.AlertAsync(strSuccess, okText: "확인");
                await Shell.Current.Navigation.PopModalAsync(true);
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private void OnTouch()
        {
            if (InputEditor != null && InputEditor.IsFocused == false)
                InputEditor.Focus();
        }

        private async void OnAttachPicture()
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

            DependencyService.Get<IMediaService>().OpenGallery();

            MessagingCenter.Unsubscribe<App, List<string>>((App)Application.Current, "ImagesSelectedAndroid");
            MessagingCenter.Subscribe<App, List<string>>((App)Application.Current, "ImagesSelectedAndroid", (s, images) =>
            {
                if (images.Count > 0)
                {
                    for(int i = 0; i < images.Count; i++)
                    {
                        MediaFile mediaFile = new MediaFile(images[i], MediaFileType.Image, this);
                        Media.Add(mediaFile);
                    }
                }
            });

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnAttachMap()
        {
            await UserDialogs.Instance.AlertAsync("준비중인 기능입니다", okText: "확인");
        }

        private async void LoadGroup(string groupId)
        {
            IsBusy = true;

            var data = await DataNotice.GetItemAsync(_updateNoticeId);
            if (data != null)
            {
                _updateNoticeDesc = data.Desc;
                for (int i = 0; i < data.listFile.Count; i++)
                {
                    data.listFile[i].SetViewModel(this);

                    Media.Add(data.listFile[i]);
                    listOriginFile.Add(data.listFile[i]);                    

                    _updateCombineImgPath += data.listFile[i].Path;
                }
            }

            PageTitle = _update ? "글수정" : "글쓰기";
            NewNotice = _update ? _updateNoticeDesc : "";            

            group = await DataGroup.GetItemAsync(groupId);
            if (group != null)
                GroupName = group.Name;

            IsBusy = false;
        }

        public string PageTitle
        {
            get => _pageTitle;
            set => SetProperty(ref _pageTitle, value);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public string NewNotice
        {
            get => _newNotice;
            set
            {
                if (value == _newNotice)
                    return;

                _newNotice = value;
                OnPropertyChanged(nameof(NewNotice));
            }
        }

        public void OnAppearing() {}

        public void OnDisappearing()
        {
            MessagingCenter.Unsubscribe<App, List<string>>((App)Application.Current, "ImagesSelectedAndroid");
            GC.Collect();
        }
    }
}
