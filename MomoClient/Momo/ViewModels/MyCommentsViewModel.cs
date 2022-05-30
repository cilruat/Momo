using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Xamarin.Forms;

using Momo.Models;
using Momo.Views;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class MyCommentsViewModel : BaseViewModel
    {
        private int loadThreshold;

        public int LoadThreshold
        {
            get => loadThreshold;
            set
            {
                if (value == loadThreshold)
                    return;

                loadThreshold = value;
                OnPropertyChanged(nameof(LoadThreshold));
            }
        }

        public ObservableCollection<Comment> Comments { get; }
        public Command LoadBottomCommand { get; }

        private bool isLoad = false;
        private bool isBottomLoading = false;
        private int bottom_load_cnt = 0;
        
        public MyCommentsViewModel()
        {
            LoadThreshold = -1;

            Comments = new ObservableCollection<Comment>();
            LoadBottomCommand = new Command(ExecuteLoadBottomCommand);
        }

        public void OnBindingContextChanged()
        {
            if (isLoad)
                return;

            isLoad = true;
            ExecuteLoadCommentsCommand();
        }

        private async void ExecuteLoadCommentsCommand()
        {
            try
            {
                LoadThreshold = -1;
                bottom_load_cnt = 0;

                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                await Task.Delay(500);

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetMyCommentAllList.php");

                var param = new Dictionary<string, string> 
                { 
                    { "person_id", Common.MyInfo.Id },
                    { "start_pos", "0" }
                };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                    {
                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    Comments.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);

                    int notice_cnt = jArray.Count;
                    bottom_load_cnt = notice_cnt;

                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        Comment comment = new Comment
                        {
                            Id = dicRes["id"],
                            Parent_Id = string.IsNullOrEmpty(dicRes["parent_id"]) ? "" : dicRes["parent_id"],
                            NoticeId = dicRes["notice_id"],
                            NoticeDesc = dicRes["notice_desc"],
                            PersonId = dicRes["person_id"],
                            GroupId = string.IsNullOrEmpty(dicRes["group_id"]) ? "" : dicRes["group_id"],
                            GroupName = dicRes["name"],
                            PersonImage = Common.MyInfo.PersonImage,
                            PersonName = Common.MyInfo.PersonName,
                            TagCommentId = dicRes["tag_comment_id"],
                            TagPersonName = string.IsNullOrEmpty(dicRes["tag_person_name"]) ? "" : dicRes["tag_person_name"],
                            Time = dicRes["time"],
                            Desc = dicRes["description"]
                        };

                        Comments.Add(comment);
                        await DataComment.UpdateItemAsync(comment);
                    }

                    LoadThreshold = jArray.Count > 20 ? 0 : - 1;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithPopModal(ex);
#endif
                UserDialogs.Instance.Toast("게시글을 불러오는데 실패했습니다");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }
        
        private async void ExecuteLoadBottomCommand()
        {
            if (isBottomLoading)
                return;

            isBottomLoading = true;

            try
            {
                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                await Task.Delay(500);

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetMyCommentAllList.php");

                var param = new Dictionary<string, string>
                {
                    { "person_id", Common.MyInfo.Id },
                    { "start_pos", bottom_load_cnt.ToString() }
                };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                    {
                        isBottomLoading = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        Comment comment = new Comment
                        {
                            Id = dicRes["id"],
                            Parent_Id = string.IsNullOrEmpty(dicRes["parent_id"]) ? "" : dicRes["parent_id"],
                            NoticeId = dicRes["notice_id"],
                            NoticeDesc = dicRes["notice_desc"],
                            PersonId = dicRes["person_id"],
                            GroupId = string.IsNullOrEmpty(dicRes["group_id"]) ? "" : dicRes["group_id"],
                            GroupName = dicRes["name"],
                            PersonImage = dicRes["profile_url"],
                            PersonName = dicRes["person_name"],
                            TagCommentId = dicRes["tag_comment_id"],
                            TagPersonName = string.IsNullOrEmpty(dicRes["tag_person_name"]) ? "" : dicRes["tag_person_name"],
                            Time = dicRes["time"],
                            Desc = dicRes["description"]
                        };

                        Comments.Add(comment);
                        await DataComment.UpdateItemAsync(comment);
                    }

                    LoadThreshold = jArray.Count > 20 ? 0 : -1;

                    bottom_load_cnt = Comments.Count;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithPopModal(ex);
#endif
                UserDialogs.Instance.Toast("게시글을 불러오는데 실패했습니다");
            }
            finally
            {
                isBottomLoading = false;
                UserDialogs.Instance.HideLoading();
            }
        }
    }
}
