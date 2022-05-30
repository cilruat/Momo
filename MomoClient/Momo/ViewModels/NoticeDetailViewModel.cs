using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;
using FFImageLoading.Forms;

namespace Momo.ViewModels
{
    public class NoticeDetailViewModel : BaseViewModel
    {
        public ObservableCollection<CustomComment> Comments { get; }
        public Command LoadCommentsCommand { get; }
        public Command ShowDetailPerson { get; }
        public Command UpdateNoticeCommand { get; }
        public Command DeleteNoticeCommand { get; }
        public Command SendCommentCommand { get; }

        public NoticeDetailViewModel(string noticeId, StackLayout mediaList)
        {
            imageList = mediaList;
            NoticeId = noticeId;            

            Comments = new ObservableCollection<CustomComment>();
            LoadCommentsCommand = new Command(ExecuteLoadCommentsCommand);
            ShowDetailPerson = new Command(OnShowDetailPerson);
            UpdateNoticeCommand = new Command(OnUpdateNoticeComment);
            DeleteNoticeCommand = new Command(OnDeleteNoticeComment);
            SendCommentCommand = new Command(OnSendComment);

            ExecuteLoadCommentsCommand();
        }

        private string noticeId;
        private string userImage;
        private string userName;
        private string time;
        private string desc;
        private string groupId;
        private string groupName;
        private string personId;
        private string comment;
        private string commentCnt;
        private bool isVisibleDesc;
        private bool isCommentList;
        private bool isModifyBox;
        private Notice _noticeInfo;

        private StackLayout imageList { get; set; }

        public string Id { get; set; }

        public string UserImage
        {
            get => userImage;
            set => SetProperty(ref userImage, value);
        }

        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string Time
        {
            get => time;
            set => SetProperty(ref time, value);
        }

        public string Desc
        {
            get => desc;
            set => SetProperty(ref desc, value);
        }

        public string GroupName
        {
            get => groupName;
            set => SetProperty(ref groupName, value);
        }

        public string NoticeId
        {
            get { return noticeId; }
            set
            {
                noticeId = value;
                LoadNoticeId(value);
            }
        }        

        public string Comment
        {
            get => comment;
            set
            {
                if (value == comment)
                    return;

                comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        public string CommentCnt
        {
            get => commentCnt;
            set
            {
                if (value == commentCnt)
                    return;

                commentCnt = value;
                OnPropertyChanged(nameof(CommentCnt));
            }
        }

        public bool IsVisibleDesc
        {
            get => isVisibleDesc;
            set
            {
                if (value == isVisibleDesc)
                    return;

                isVisibleDesc = value;
                OnPropertyChanged(nameof(IsVisibleDesc));
            }
        }

        public bool IsCommentList
        {
            get => isCommentList;
            set
            {
                if (value == isCommentList)
                    return;

                isCommentList = value;
                OnPropertyChanged(nameof(IsCommentList));
            }
        }

        public bool IsModifyBox
        {
            get => isModifyBox;
            set
            {
                if (value == isModifyBox)
                    return;

                isModifyBox = value;
                OnPropertyChanged(nameof(IsModifyBox));
            }
        }

        public async void LoadNoticeId(string noticeId)
        {
            try
            {
                _noticeInfo = await DataNotice.GetItemAsync(noticeId);
                if (_noticeInfo == null)
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetNoticeInfo.php");

                    var param = new Dictionary<string, string> { { "notice_id", noticeId } };
                    var content = new FormUrlEncodedContent(param);

                    Task<HttpResponseMessage> task_response = client.PostAsync(uri, content);
                    HttpResponseMessage response = task_response.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Task<string> task_jsonResponse = response.Content.ReadAsStringAsync();
                        string jsonResponse = task_jsonResponse.Result;
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                        Person person = new Person
                        {
                            Id = dicRes["p_id"],
                            PersonImage = dicRes["profile_url"],
                            PersonName = dicRes["person_name"],
                            Grade = dicRes["grade"],
                            PhoneNum = dicRes["phone_num"],
                            Etc = dicRes["etc"]
                        };

                        Notice notice = new Notice
                        {
                            Id = dicRes["id"],
                            GroupId = dicRes["group_id"],
                            GroupName = dicRes["group_name"],
                            PersonId = dicRes["person_id"],
                            PersonImage = dicRes["profile_url"],
                            PersonName = dicRes["person_name"],
                            Time = dicRes["time"],
                            IsVisibleDesc = string.IsNullOrEmpty(dicRes["description"]) == false,
                            Desc = dicRes["description"],
                            CommentCnt = dicRes["comment_cnt"]
                        };

                        _noticeInfo = notice;

                        _ = DataPerson.UpdateItemAsync(person);
                        _ = DataNotice.UpdateItemAsync(notice);
                    }
                }

                Id = _noticeInfo.Id;
                UserImage = _noticeInfo.PersonImage;
                UserName = _noticeInfo.PersonName;
                Time = _noticeInfo.Time;
                Desc = _noticeInfo.Desc;
                groupId = _noticeInfo.GroupId;
                GroupName = _noticeInfo.GroupName;
                personId = _noticeInfo.PersonId;
                IsVisibleDesc = _noticeInfo.IsVisibleDesc;
                IsModifyBox = _noticeInfo.PersonId == Common.MyInfo.Id;

                if (_noticeInfo.listFile.Count > 0)
                {
                    imageList.Children.Clear();
                    for (int i = 0; i < _noticeInfo.listFile.Count; i++)
                    {
                        string path = _noticeInfo.listFile[i].Path;
                        CachedImage attachImage = new CachedImage()
                        {
                            DownsampleToViewSize = true,
                            HeightRequest = 280,
                            Source = path,
                            HorizontalOptions = LayoutOptions.FillAndExpand
                        };

                        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += async (s, args) =>
                        {
                            await Shell.Current.Navigation.PushModalAsync(new ImageDetailPage(path));
                        };

                        attachImage.GestureRecognizers.Add(tapGesture);

                        imageList.Children.Add(attachImage);
                    }                    

                    return;
                }

                HttpClient client_a = new HttpClient();
                Uri uri_a = new Uri(Common.UrlServerPHP + "GetAttachFileList.php");

                var param_a = new Dictionary<string, string>
                {
                    { "group_id", groupId },
                    { "notice_id", noticeId },
                    { "type", MediaFileType.Image.ToString() }
                };
                var content_a = new FormUrlEncodedContent(param_a);

                Task<HttpResponseMessage> task_response_a = client_a.PostAsync(uri_a, content_a);
                HttpResponseMessage response_a = task_response_a.Result;
                if (response_a.IsSuccessStatusCode)
                {
                    Task<string> task_jsonResponse = response_a.Content.ReadAsStringAsync();
                    string jsonResponse = task_jsonResponse.Result;
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsBusy = false;
                        return;
                    }

                    imageList.Children.Clear();
                    _noticeInfo.listFile.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        string path = dicRes["url"];
                        if (path.StartsWith(".."))
                            path = Common.UrlServer + dicRes["url"].Substring(3);

                        MediaFile file = new MediaFile
                        {
                            Id = dicRes["id"],
                            GroupId = dicRes["group_id"],
                            NoticeId = dicRes["notice_id"],
                            Path = path,
                            Type = (MediaFileType)int.Parse(dicRes["id"])
                        };

                        CachedImage attachImage = new CachedImage()
                        {
                            DownsampleToViewSize = true,
                            HeightRequest = 280,
                            Source = path,
                            HorizontalOptions = LayoutOptions.FillAndExpand
                        };

                        TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                        tapGesture.Tapped += async (s, args) =>
                        {
                            await Shell.Current.Navigation.PushModalAsync(new ImageDetailPage(path));
                        };

                        attachImage.GestureRecognizers.Add(tapGesture);

                        imageList.Children.Add(attachImage);

                        _noticeInfo.listFile.Add(file);
                        _ = DataMediaFile.UpdateItemAsync(file);
                    }
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        public async void ExecuteLoadCommentsCommand()
        {
            CommentCnt = "0";

            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetCommentList.php");

                var param = new Dictionary<string, string> { { "notice_id", noticeId } };
                var content = new FormUrlEncodedContent(param);

                Task<HttpResponseMessage> task_response = client.PostAsync(uri, content);
                HttpResponseMessage response = task_response.Result;
                if (response.IsSuccessStatusCode)
                {
                    Comments.Clear();

                    Task<string> task_jsonResponse = response.Content.ReadAsStringAsync();
                    string jsonResponse = task_jsonResponse.Result;
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsCommentList = false;
                        return;
                    }

                    int comment_cnt = 0;
                    JArray jArray = JArray.Parse(jsonResponse);

                    List<CustomComment> makeList = new List<CustomComment>();
                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        Comment comment = new Comment
                        {
                            Id = dicRes["id"],
                            Parent_Id = string.IsNullOrEmpty(dicRes["parent_id"]) ? "" : dicRes["parent_id"],
                            NoticeId = dicRes["notice_id"],
                            PersonId = dicRes["person_id"],
                            GroupId = string.IsNullOrEmpty(dicRes["group_id"]) ? "" : dicRes["group_id"],
                            PersonImage = dicRes["profile_url"],
                            PersonName = dicRes["person_name"],
                            TagCommentId = dicRes["tag_comment_id"],
                            TagPersonName = string.IsNullOrEmpty(dicRes["tag_person_name"]) ? "" : dicRes["tag_person_name"],
                            Time = dicRes["time"],
                            Desc = dicRes["description"]
                        };

                        _ = DataComment.UpdateItemAsync(comment);
                        CustomComment custom = new CustomComment(comment, groupName, this);

                        string tag_id = comment.TagCommentId;
                        string parent_id = comment.Parent_Id;

                        if (string.IsNullOrEmpty(parent_id) || parent_id == "0")
                            makeList.Add(custom);                            
                        else
                        {
                            CustomComment find = makeList.Find(c => c.OriginComment.Id == parent_id);
                            if (find != null)
                            {
                                if (string.IsNullOrEmpty(tag_id) == false && tag_id != "0" && parent_id != tag_id)
                                {
                                    int find_last_idx = find.Child.FindLastIndex(c => c.OriginComment.TagCommentId == tag_id);
                                    if (find_last_idx > -1)
                                    {
                                        int calc_cnt = find.Child.Count - find_last_idx - 1;
                                        find.Child.Insert(find_last_idx + calc_cnt + 1, custom);
                                    }
                                    else
                                    {
                                        int find_idx = find.Child.FindIndex(c => c.OriginComment.Id == tag_id);
                                        if (find_idx > -1)
                                            find.Child.Insert(find_idx + 1, custom);
                                    }
                                }
                                else
                                    find.Child.Add(custom);
                            }
                        }

                        comment_cnt++;
                    }

                    for(int i = 0; i < makeList.Count; i++)
                        Comments.Add(makeList[i]);

                    IsCommentList = comment_cnt > 0;
                    CommentCnt = comment_cnt.ToString();
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private void OnShowDetailPerson()
        {
            base.OnPersonSelected(_noticeInfo);
        }

        private async void OnUpdateNoticeComment()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            _ = Shell.Current.Navigation.PopModalAsync(true);
            await Shell.Current.Navigation.PushModalAsync(new NewNoticePage(groupId, true, noticeId));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnDeleteNoticeComment()
        {
            bool isAccept = await UserDialogs.Instance.ConfirmAsync("글을 삭제하시겠습니까?", okText:"예", cancelText:"아니오");
            if (isAccept)
            {
                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

                try
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "DeleteNotice.php");

                    var param = new Dictionary<string, string> { { "notice_id", noticeId } };
                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        bool success = bool.Parse(jsonResponse);

                        await Task.Delay(300);
                        UserDialogs.Instance.HideLoading();

                        string strSuccess = success ? "글이 삭제되었습니다" : "글을 삭제하는데 실패하였습니다\n다시 시도해주세요";
                        await UserDialogs.Instance.AlertAsync(strSuccess, okText: "확인");

                        if (success)
                        {
                            await DataNotice.DeleteItemAsync(noticeId);
                            await Shell.Current.Navigation.PopModalAsync(true);
                        }
                    }
                    else
                    {
                        await Common.ErrorAlertWithPopModal();
                        UserDialogs.Instance.HideLoading();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await Common.ErrorAlertWithPopModal(ex);
                    UserDialogs.Instance.HideLoading();
                    return;
                }
            }
        }


        private async void OnSendComment()
        {
            if (string.IsNullOrEmpty(comment))
            {
                await UserDialogs.Instance.AlertAsync("내용을 입력해주세요", okText: "확인");
                return;
            }

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "NewComment.php");

                var param = new Dictionary<string, string>
                {
                    { "notice_id", noticeId },
                    { "person_id", Common.MyInfo.Id },
                    { "person_name", Common.MyInfo.PersonName },
                    { "comment", comment },
                    { "group_id", groupId },
                    { "group_name", groupName },
                    { "receive_id", personId },
                    { "parent_id", "0" },
                    { "tag_comment_id", "0" },
                    { "tag_person_name", "" }
                };

                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    bool success = bool.Parse(jsonResponse);
                    string strSuccess = success ?
                        "댓글을 등록하였습니다" :
                        "댓글을 등록하는데 실패하였습니다\n다시 작성해주세요";

                    UserDialogs.Instance.HideLoading();

                    await UserDialogs.Instance.AlertAsync(strSuccess, okText: "확인");
                    ExecuteLoadCommentsCommand();
                }

                Comment = "";
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                UserDialogs.Instance.HideLoading();
                return;
            }
        }
    }
}
