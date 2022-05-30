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
    public class TapNoticesViewModel : BaseViewModel
    {
        static public bool RefreshNotice = false;

        private bool isLoading;
        private bool isEmptyList;
        private bool isNoticeList;
        private int loadThreshold;

        public bool IsLoading
        {
            get => isLoading;
            set
            {
                if (value == isLoading)
                    return;

                isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public bool IsEmptyList
        {
            get => isEmptyList;
            set
            {
                if (value == isEmptyList)
                    return;

                isEmptyList = value;
                OnPropertyChanged(nameof(IsEmptyList));
            }
        }

        public bool IsNoticeList
        {
            get => isNoticeList;
            set
            {
                if (value == isNoticeList)
                    return;

                isNoticeList = value;
                OnPropertyChanged(nameof(IsNoticeList));
            }
        }

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
        public Command LoadNoticesCommand { get; }
        public Command LoadBottomCommand { get; }
        public Command AddNoticeCommand { get; }

        
        private bool isReload = true;
        private bool isBottomLoading = false;
        private int bottom_load_cnt = 0;        


        public TapNoticesViewModel()
        {
            IsLoading = true;
            IsEmptyList = false;
            IsNoticeList = false;
            LoadThreshold = -1;

            Notices = new ObservableCollection<Notice>();

            LoadNoticesCommand = new Command(ExecuteLoadNoticesCommand);
            LoadBottomCommand = new Command(ExecuteLoadBottomCommand);
            AddNoticeCommand = new Command(OnAddNotice);

            RefreshNotice = true;
        }

        private async void ExecuteLoadNoticesCommand()
        {
            try
            {
                IsBusy = false;

                LoadThreshold = -1;
                bottom_load_cnt = 0;

                if (Notices.Count == 0)
                {
                    UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                    await Task.Delay(500);
                }

                if (isReload == false)
                {
                    var notices = await DataNotice.GetItemsAsync();
                    if (notices != null && DataNotice.GetCount() > 0)
                    {
                        Notices.Clear();
                        foreach (Notice n in notices)
                            Notices.Add(n);
                    }
                    else
                    {
                        IsLoading = false;
                        IsEmptyList = true;
                        IsNoticeList = false;
                    }
                    
                    UserDialogs.Instance.HideLoading();
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetNoticeAllList.php");

                var param = new Dictionary<string, string> 
                { 
                    { "person_id", Common.MyInfo.Id },
                    { "start_pos", "0" }
                };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    IsLoading = false;
                    
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsEmptyList = true;
                        IsNoticeList = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    Notices.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);

                    int notice_cnt = jArray.Count;
                    IsEmptyList = notice_cnt == 0;
                    IsNoticeList = notice_cnt > 0;
                    bottom_load_cnt = notice_cnt;

                    foreach (JObject e in jArray)
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

                        Notices.Add(notice);
                        await DataPerson.UpdateItemAsync(person);
                        await DataNotice.UpdateItemAsync(notice);
                    }

                    await DataNotice.SortItemAsync();

                    LoadThreshold = notice_cnt > 20 ? 0 : -1;

                    isReload = false;
                    Device.StartTimer(TimeSpan.FromSeconds(10), () =>
                    {
                        isReload = true;
                        return false;
                    });
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithMoveLogin(ex);
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
                Uri uri = new Uri(Common.UrlServerPHP + "GetNoticeAllList.php");

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
                        IsEmptyList = true;
                        IsNoticeList = false;
                        isBottomLoading = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
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

                        Notices.Add(notice);
                        await DataPerson.UpdateItemAsync(person);
                        await DataNotice.UpdateItemAsync(notice);
                    }

                    await DataNotice.SortItemAsync();

                    LoadThreshold = jArray.Count > 20 ? 0 : -1;

                    bottom_load_cnt = Notices.Count;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithMoveLogin(ex);
#endif
                UserDialogs.Instance.Toast("게시글을 불러오는데 실패했습니다");
            }
            finally
            {
                isBottomLoading = false;
                UserDialogs.Instance.HideLoading();
            }
        }

        public void OnAppearing()
        {
            if (isReload)
                RefreshNotice = true;

            if (RefreshNotice)
            {
                isReload = true;
                IsBusy = true;
                RefreshNotice = false;
            }
        }

        private async void OnAddNotice(object obj)
        {
            if (DataGroup.GetCount() == 0)
            {
                await UserDialogs.Instance.AlertAsync("글쓰기가 가능한 모임이 없습니다", okText: "확인");
                return;
            }

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            Common.eMSType = Common.EMultiSelectType.NewNotice;
            await Shell.Current.Navigation.PushModalAsync(new SelectGroupPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }
    }
}
