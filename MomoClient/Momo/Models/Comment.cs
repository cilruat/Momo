using System;
using System.Net.Http;
using System.ComponentModel;
using System.Collections.Generic;

using Xamarin.Forms;

using Momo.ViewModels;

using FFImageLoading.Cache;
using FFImageLoading.Forms;

using Acr.UserDialogs;

namespace Momo.Models
{
    public class Comment
    {
        public string Id { get; set; }
        public string Parent_Id { get; set; }
        public string NoticeId { get; set; }
        public string NoticeDesc { get; set; }
        public string PersonId { get; set; }
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string PersonName { get; set; }
        public string TagCommentId { get; set; }
        public string TagPersonName { get; set; }

        private string time;
        public string Time
        {
            get
            {
                DateTime date = DateTime.Parse(time);
                return date.ToLongDateString() + " " + date.ToShortTimeString();
            }
            set { time = value; }
        }

        public string Desc { get; set; }

        private string _personImage;
        public string PersonImage
        {
            get => _personImage;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _personImage = "Icon_profile.png";
                    return;
                }

                string makeUrl = value;
                if (makeUrl.StartsWith(".."))
                    makeUrl = Common.UrlServer + value.Substring(3);

                if (_personImage == makeUrl)
                    return;

                if (string.IsNullOrEmpty(_personImage) == false)
                    CachedImage.InvalidateCache(_personImage, CacheType.All, true);

                _personImage = makeUrl;
            }
        }
    }

    public class CustomComment : INotifyPropertyChanged
    {
        public Comment OriginComment { get; set; }

        public Command ShowCommentCommand { get; }
        public Command WriteCommentCommand { get; }
        public Command CancelCommentCommand { get; }
        
        public List<CustomComment> Child { get; set; }

        private bool _isShow;
        private string _comment;
        private string _groupName;
        private NoticeDetailViewModel _viewModel;

        public CustomComment(Comment comment, string groupName, NoticeDetailViewModel viewModel)
        {
            _groupName = groupName;
            _viewModel = viewModel;

            IsShow = false;
            OriginComment = comment;
            
            Child = new List<CustomComment>();

            ShowCommentCommand = new Command(OnShow);
            WriteCommentCommand = new Command(OnWrite);
            CancelCommentCommand = new Command(OnCancel);
        }
        
        public bool IsShow
        {
            get => _isShow;
            set
            {
                _isShow = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsShow"));
            }
        }
        
        public string Comment
        {
            get => _comment;
            set
            {
                if (value == _comment)
                    return;

                _comment = value;
                PropertyChanged(this, new PropertyChangedEventArgs("Comment"));
            }
        }

        private void OnShow()
        {
            IsShow = !_isShow;
        }

        private async void OnWrite()
        {
            if (string.IsNullOrEmpty(_comment))
            {
                await UserDialogs.Instance.AlertAsync("내용을 입력해주세요", okText: "확인");
                return;
            }

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "NewComment.php");

                string tagName = "#" + OriginComment.PersonName;
                string parent_id = OriginComment.Parent_Id;
                if (string.IsNullOrEmpty(OriginComment.Parent_Id) || OriginComment.Parent_Id == "0")
                {
                    tagName = "";
                    parent_id = OriginComment.Id;
                }                    

                var param = new Dictionary<string, string>
                {
                    { "notice_id", OriginComment.NoticeId },
                    { "person_id", Common.MyInfo.Id },
                    { "person_name", Common.MyInfo.PersonName },
                    { "comment", _comment },
                    { "group_id", OriginComment.GroupId },
                    { "group_name", _groupName },
                    { "receive_id", OriginComment.PersonId },
                    { "parent_id", parent_id },
                    { "tag_comment_id", OriginComment.Id },
                    { "tag_person_name", tagName }
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
                    _viewModel.ExecuteLoadCommentsCommand();
                }

                OnCancel();
            }
            catch(Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await Common.ErrorAlertWithPopModal(ex);
            }
        }

        private void OnCancel()
        {
            IsShow = false;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}