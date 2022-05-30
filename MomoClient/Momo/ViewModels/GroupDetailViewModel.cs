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
    [QueryProperty(nameof(GroupId), nameof(GroupId))]
    class GroupDetailViewModel : BaseViewModel
    {
        static public bool RefreshInfo = false;
        static public bool RefreshNotice = false;
        static public bool RefreshPersonCnt = false;

        private string groupId;
        private string groupImage;
        private string groupName;
        private string isFree;
        private int personCnt;

        private bool isEmptyList;
        private bool isNoticeList;
        private bool isLeader;
        private int loadThreshold;

        // schedule
        private bool showSchedule;
        private string scheduleYear;
        private string scheduleMonth;
        private string scheduleTitle;

        public string GroupImage
        {
            get => groupImage;
            set => SetProperty(ref groupImage, value);
        }

        public string GroupName
        {
            get => groupName;
            set => SetProperty(ref groupName, value);
        }

        public string IsFree
        {
            get => isFree;
            set => SetProperty(ref isFree, value);
        }

        public int PersonCnt
        {
            get => personCnt;
            set
            {
                if (value == personCnt)
                    return;

                personCnt = value;
                OnPropertyChanged(nameof(PersonCnt));
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

        public bool IsLeader
        {
            get => isLeader;
            set => SetProperty(ref isLeader, value);
        }

        public bool ShowSchedule
        {
            get => showSchedule;
            set => SetProperty(ref showSchedule, value);
        }

        public string ScheduleYear
        {
            get => scheduleYear;
            set => SetProperty(ref scheduleYear, value);
        }

        public string ScheduleMonth
        {
            get => scheduleMonth;
            set => SetProperty(ref scheduleMonth, value);
        }

        public string ScheduleTitle
        {
            get => scheduleTitle;
            set => SetProperty(ref scheduleTitle, value);
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
        public Command AddPersonCommand { get; }
        public Command AddSMSCommand { get; }
        public Command ShowCalendarCommand { get; }
        public Command SettingCommand { get; }

        private bool isReload = true;
        private bool isBottomLoading = false;
        private int bottom_load_cnt = 0;

        private GroupDetailPage currentPage;

        public GroupDetailViewModel(GroupDetailPage currentPage)
        {
            this.currentPage = currentPage;

            IsEmptyList = false;
            IsNoticeList = false;
            LoadThreshold = -1;

            Notices = new ObservableCollection<Notice>();

            LoadNoticesCommand = new Command(ExecuteLoadNoticesCommand);
            LoadBottomCommand = new Command(ExecuteLoadBottomCommand);

            AddNoticeCommand = new Command(OnAddNotice);
            AddPersonCommand = new Command(OnAddPerson);
            AddSMSCommand = new Command(OnAddSMS);

            ShowCalendarCommand = new Command(OnShowCalendar);
            SettingCommand = new Command(OnSetting);

            RefreshNotice = true;
        }

        async void ExecuteLoadNoticesCommand()
        {
            if (string.IsNullOrEmpty(groupId))
            {
                await Common.ErrorAlertWithPopModal();
                return;
            }

            try
            {
                IsBusy = false;

                LoadThreshold = -1;
                bottom_load_cnt = 0;

                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                await Task.Delay(100);

                // show schedule
                var scheduls = await DataSchedule.GetItemsAsync();
                if (scheduls != null)
                {
                    

                    DateTime now = DateTime.Now;
                    double minDiff = double.MaxValue;

                    Schedule nearest = null;
                    foreach (Schedule s in scheduls)
                    {
                        if (s.GroupId != groupId)
                            continue;

                        TimeSpan timeDiff = s.StartDate - now;
                        if (timeDiff.TotalMinutes > 0 && minDiff > timeDiff.TotalMinutes)
                        {
                            minDiff = timeDiff.TotalMinutes;
                            nearest = s;
                        }
                    }

                    if (nearest != null)
                    {
                        ShowSchedule = true;

                        ScheduleYear = nearest.StartDate.ToString("yy");
                        ScheduleMonth = "년/" + nearest.StartDate.ToString("M월");
                        ScheduleTitle = nearest.Title;
                    }
                }

                if (isReload == false)
                {
                    var notices = await DataNotice.GetItemsAsync();
                    if (notices != null)
                    {
                        Notices.Clear();
                        foreach (Notice n in notices)
                        {
                            if (n.GroupId == groupId)
                                Notices.Add(n);
                        }
                    }
                    else
                    {
                        IsEmptyList = true;
                        IsNoticeList = false;
                    }

                    UserDialogs.Instance.HideLoading();
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetNoticeList.php");

                var param = new Dictionary<string, string>
                {
                    { "group_id", groupId },
                    { "start_pos", bottom_load_cnt.ToString() }
                };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    Notices.Clear();

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsEmptyList = true;
                        IsNoticeList = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }                        

                    JArray jArray = JArray.Parse(jsonResponse);

                    int notice_cnt = jArray.Count;
                    IsEmptyList = notice_cnt == 0;
                    IsNoticeList = notice_cnt > 0;
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
                Uri uri = new Uri(Common.UrlServerPHP + "GetNoticeList.php");

                var param = new Dictionary<string, string>
                {
                    { "group_id", groupId },
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
            if (RefreshPersonCnt)
            {
                RefreshPersonCount();
                RefreshPersonCnt = false;
            }

            if (RefreshNotice)
            {
                isReload = true;
                IsBusy = true;
                RefreshNotice = false;
            }

            if (RefreshInfo)
            {
                LoadGroup(groupId);
                RefreshInfo = false;
            }
        }

        private async void RefreshPersonCount()
        {
            Group group = await DataGroup.GetItemAsync(groupId);
            if (group != null)
                PersonCnt = group.Count;
        }

        private async void OnAddNotice(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new NewNoticePage(Common.ViewGroupID));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnAddPerson(object obj)
        {
            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            await Task.Delay(500);

            List<string> listNumber = new List<string>();
            Group group = await DataGroup.GetItemAsync(Common.ViewGroupID);
            if (group != null)
            {
                if (group.Persons.Count == 0)
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetPersonList.php");

                    var param = new Dictionary<string, string> { { "group_id", Common.ViewGroupID } };
                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (jsonResponse.StartsWith("null"))
                            return;

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

                            await DataPerson.UpdateItemAsync(person);

                            if (group != null)
                            {
                                group.Persons.Add(person);
                                await DataGroup.UpdateItemAsync(group);
                            }
                        }
                    }
                }

                for (int i = 0; i < group.Persons.Count; i++)
                    listNumber.Add(group.Persons[i].PhoneNum);
            }

            NewGroupAddPersonViewModel.EType = AddPersonType.Directly;
            await Shell.Current.Navigation.PushModalAsync(new NewGroupAddPersonPage(listNumber));
            await Task.Delay(100);

            UserDialogs.Instance.HideLoading();
        }

        private async void OnAddSMS(object obj)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            Common.eMSType = Common.EMultiSelectType.AddPerson;
            await Shell.Current.Navigation.PushModalAsync(new MultiSelectPersonPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }
        
        private async void OnShowCalendar()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            currentPage.MoveCalendarPage();
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnSetting()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new NewGroupPage(true));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        public string GroupId
        {
            get { return groupId; }
            set
            {
                groupId = value;
                LoadGroup(value);
            }
        }

        public async void LoadGroup(string groupId)
        {
            try
            {
                Common.ViewGroupID = groupId;

                var data = await DataGroup.GetItemAsync(groupId);
                if (data != null)
                {
                    GroupImage = data.Image;
                    GroupName = data.Name;
                    PersonCnt = data.Count;
                    IsFree = data.Free ? "공개" : "비공개";
                    IsLeader = data.LeaderId == Common.MyInfo.Id;
                    if (isLeader)
                        Common.MyInfo.Leader = true;

                    Common.ViewGroupName = data.Name;
                }
                else
                {
                    await Common.ErrorAlertWithPopModal();
                    return;
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }
    }
}
