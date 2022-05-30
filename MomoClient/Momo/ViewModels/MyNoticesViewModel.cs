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
    public class MyNoticesViewModel : BaseViewModel
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

        public ObservableCollection<Notice> Notices { get; }
        public Command LoadBottomCommand { get; }

        
        private bool isBottomLoading = false;
        private int bottom_load_cnt = 0;        


        public MyNoticesViewModel()
        {
            LoadThreshold = -1;

            Notices = new ObservableCollection<Notice>();
            LoadBottomCommand = new Command(ExecuteLoadBottomCommand);

            ExecuteLoadNoticesCommand();
        }

        private async void ExecuteLoadNoticesCommand()
        {
            try
            {
                LoadThreshold = -1;
                bottom_load_cnt = 0;

                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                await Task.Delay(500);

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetMyNoticeAllList.php");

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

                    Notices.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);

                    int notice_cnt = jArray.Count;
                    bottom_load_cnt = notice_cnt;

                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());
                        
                        Notice notice = new Notice
                        {
                            Id = dicRes["id"],
                            GroupId = dicRes["group_id"],
                            GroupName = dicRes["group_name"],
                            PersonId = dicRes["person_id"],
                            PersonImage = Common.MyInfo.PersonImage,
                            PersonName = dicRes["person_name"],
                            Time = dicRes["time"],
                            IsVisibleDesc = string.IsNullOrEmpty(dicRes["description"]) == false,
                            Desc = dicRes["description"],
                            CommentCnt = dicRes["comment_cnt"]
                        };

                        if (string.IsNullOrEmpty(dicRes["attach_image_cnt"]) == false)
                        {
                            int attach_cnt = int.Parse(dicRes["attach_image_cnt"]);
                            notice.IsMoreShow = attach_cnt > 4;

                            int min_attach_cnt = Math.Min(attach_cnt, 4);
                            switch (min_attach_cnt)
                            {
                                case 1: notice.AttachImage_1 = true; break;
                                case 2: notice.AttachImage_2 = true; break;
                                case 3: notice.AttachImage_3 = true; break;
                                case 4: notice.AttachImage_4 = true; break;
                            }

                            notice.AttachImageUrl_1 = string.IsNullOrEmpty(dicRes["attach_image_url_1"]) ? "" : dicRes["attach_image_url_1"];
                            notice.AttachImageUrl_2 = string.IsNullOrEmpty(dicRes["attach_image_url_2"]) ? "" : dicRes["attach_image_url_2"];
                            notice.AttachImageUrl_3 = string.IsNullOrEmpty(dicRes["attach_image_url_3"]) ? "" : dicRes["attach_image_url_3"];
                            notice.AttachImageUrl_4 = string.IsNullOrEmpty(dicRes["attach_image_url_4"]) ? "" : dicRes["attach_image_url_4"];
                        }

                        await DataNotice.UpdateItemAsync(notice);

                        Notices.Add(notice);
                    }

                    await DataNotice.SortItemAsync();

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
                Uri uri = new Uri(Common.UrlServerPHP + "GetMyNoticeAllList.php");

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

                        Notice notice = new Notice
                        {
                            Id = dicRes["id"],
                            GroupId = dicRes["group_id"],
                            GroupName = dicRes["group_name"],
                            PersonId = dicRes["person_id"],
                            PersonImage = Common.MyInfo.PersonImage,
                            PersonName = dicRes["person_name"],
                            Time = dicRes["time"],
                            IsVisibleDesc = string.IsNullOrEmpty(dicRes["description"]) == false,
                            Desc = dicRes["description"],
                            CommentCnt = dicRes["comment_cnt"]
                        };

                        if (string.IsNullOrEmpty(dicRes["attach_image_cnt"]) == false)
                        {
                            int attach_cnt = int.Parse(dicRes["attach_image_cnt"]);
                            notice.IsMoreShow = attach_cnt > 4;

                            int min_attach_cnt = Math.Min(attach_cnt, 4);
                            switch (min_attach_cnt)
                            {
                                case 1: notice.AttachImage_1 = true; break;
                                case 2: notice.AttachImage_2 = true; break;
                                case 3: notice.AttachImage_3 = true; break;
                                case 4: notice.AttachImage_4 = true; break;
                            }

                            notice.AttachImageUrl_1 = string.IsNullOrEmpty(dicRes["attach_image_url_1"]) ? "" : dicRes["attach_image_url_1"];
                            notice.AttachImageUrl_2 = string.IsNullOrEmpty(dicRes["attach_image_url_2"]) ? "" : dicRes["attach_image_url_2"];
                            notice.AttachImageUrl_3 = string.IsNullOrEmpty(dicRes["attach_image_url_3"]) ? "" : dicRes["attach_image_url_3"];
                            notice.AttachImageUrl_4 = string.IsNullOrEmpty(dicRes["attach_image_url_4"]) ? "" : dicRes["attach_image_url_4"];
                        }

                        await DataNotice.UpdateItemAsync(notice);

                        Notices.Add(notice);
                    }

                    await DataNotice.SortItemAsync();

                    LoadThreshold = jArray.Count > 20 ? 0 : -1;

                    bottom_load_cnt = Notices.Count;
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
