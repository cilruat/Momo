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

namespace Momo.ViewModels
{
    public class TapGroupsViewModel : BaseViewModel
    {
        private bool isReload = true;

        private bool isLoading;
        private bool isEmptyList;
        private bool isGroupList;

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

        public bool IsGroupList
        {
            get => isGroupList;
            set
            {
                if (value == isGroupList)
                    return;

                isGroupList = value;
                OnPropertyChanged(nameof(IsGroupList));
            }
        }

        public ObservableCollection<Group> Groups { get; }
        public Command LoadGroupsCommand { get; }        
        public Command AddGroupCommand { get; }


        public TapGroupsViewModel()
        {
            IsLoading = true;
            IsEmptyList = false;
            IsGroupList = false;

            Groups = new ObservableCollection<Group>();
            LoadGroupsCommand = new Command(ExecuteLoadGroupsCommand);
            AddGroupCommand = new Command(OnAddGroup);
        }

        async void ExecuteLoadGroupsCommand()
        {
            try
            {
                IsBusy = false;

                if (Groups.Count == 0)
                {
                    UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                    await Task.Delay(500);
                }

                if (isReload == false)
                {
                    var groups = await DataGroup.GetItemsAsync();
                    if (groups != null && DataGroup.GetCount() > 0)
                    {
                        Groups.Clear();
                        foreach (Group g in groups)
                            Groups.Add(g);
                    }
                    else
                    {
                        IsLoading = false;
                        IsEmptyList = true;
                        IsGroupList = false;
                    }

                    UserDialogs.Instance.HideLoading();
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetGroupList.php");

                var param = new Dictionary<string, string> { { "person_id", Common.MyInfo.Id } };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    IsLoading = false;

                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsEmptyList = true;
                        IsGroupList = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    Groups.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        Group group = new Group();
                        group.Id = dicRes["id"];
                        group.LeaderId = dicRes["leader_id"];
                        group.Name = dicRes["name"];
                        group.Image = dicRes["profile_url"];

                        if (int.TryParse(dicRes["count"], out int group_cnt))
                            group.Count = group_cnt;

                        group.Free = dicRes["free"] != "0";

                        await DataGroup.UpdateItemAsync(group);
                        Groups.Add(group);
                    }

                    await DataGroup.SortItemAsync();

                    isReload = false;
                    Device.StartTimer(TimeSpan.FromSeconds(10), () =>
                    {
                        isReload = true;
                        return false;
                    });

                    IsEmptyList = Groups.Count == 0;
                    IsGroupList = Groups.Count > 0;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithMoveLogin(ex);
#endif
                UserDialogs.Instance.Toast("모임을 불러오는데 실패했습니다");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }

            Background.Instance.GetScheduleList();

            // update token
            UpdateToken();

            // check click noti
            CheckClickNoti();
        }

        async void UpdateToken()
        {
            try
            {
                string token = App.TOKEN;

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "UpdateToken.php");

                var param = new Dictionary<string, string>
                {
                    { "person_id", Common.MyInfo.Id },
                    { "token", token }
                };

                var content = new FormUrlEncodedContent(param);
                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    Task<string> task_jsonResponse = response.Content.ReadAsStringAsync();
                    string jsonResponse = task_jsonResponse.Result;

                    if (bool.Parse(jsonResponse))
                        App.TOKEN = token;
                }
            }
            catch { }
        }

        async void CheckClickNoti()
        {
            if (Common.FirstCheckClickNoti)         return;
            if (App.NotiInfo.dicData == null)       return;
            if (App.NotiInfo.dicData.Count == 0)    return;

            //Common.FirstCheckClickNoti = true;

            string groupId = "";
            string key = App.NotiInfo.dicData["key"];
            switch (key)
            {
                case "notice":
                case "comment":
                    string noticeId = App.NotiInfo.dicData["notice_id"];

                    App.NotiInfo.Empty();
                    await Shell.Current.Navigation.PushModalAsync(new NoticeDetailPage(noticeId));
                    break;
                case "chat":
                    string roomId = App.NotiInfo.dicData["room_id"];
                    string roomName = App.NotiInfo.dicData["room_name"];
                    string personIds = App.NotiInfo.dicData["person_ids"];
                    groupId = App.NotiInfo.dicData["group_id"];

                    App.NotiInfo.Empty();
                    await Shell.Current.Navigation.PushModalAsync(new ChatDetailPage(roomId, roomName, personIds, groupId));
                    break;
                case "schedule":
                    groupId = App.NotiInfo.dicData["group_id"];

                    App.NotiInfo.Empty();
                    await Shell.Current.GoToAsync($"{nameof(GroupDetailPage)}?{nameof(GroupDetailViewModel.GroupId)}={groupId}");
                    break;
            }
        }

        private async void OnAddGroup()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new NewGroupPage(false));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        public void OnAppearing()
        {
            if (isReload)
                IsBusy = true;
        }
    }
}
