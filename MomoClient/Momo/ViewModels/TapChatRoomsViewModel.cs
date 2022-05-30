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
    public class TapChatRoomsViewModel : BaseViewModel
    {
        private bool isLoading;
        private bool isEmptyList;
        private bool isRoomList;

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

        public bool IsRoomList
        {
            get => isRoomList;
            set
            {
                if (value == isRoomList)
                    return;

                isRoomList = value;
                OnPropertyChanged(nameof(IsRoomList));
            }
        }        

        public ObservableCollection<ChatRoom> Rooms { get; }
        public Command LoadRoomsCommand { get; }
        public Command AddChatRoomCommand { get; }
        public Command<ChatRoom> ChatRoomTapped { get; }

        private bool isReload = true;


        public TapChatRoomsViewModel()
        {
            IsLoading = true;
            IsEmptyList = false;
            IsRoomList = false;

            Rooms = new ObservableCollection<ChatRoom>();

            LoadRoomsCommand = new Command(async () => await ExecuteLoadRoomsCommand());
            AddChatRoomCommand = new Command(OnAddChatRoom);
            ChatRoomTapped = new Command<ChatRoom>(OnRoomSelected);            
        }

        async Task ExecuteLoadRoomsCommand()
        {
            try
            {
                IsBusy = false;

                if (Rooms.Count == 0)
                {
                    UserDialogs.Instance.ShowLoading("", MaskType.Gradient);
                    await Task.Delay(500);
                }

                if (isReload == false)
                {
                    var rooms = await DataChatRoom.GetItemsAsync();
                    if (rooms != null && DataChatRoom.GetCount() > 0)
                    {
                        Rooms.Clear();
                        foreach (ChatRoom r in rooms)
                            Rooms.Add(r);
                    }
                    else
                    {
                        IsLoading = false;
                        IsEmptyList = true;
                        IsRoomList = false;
                    }

                    UserDialogs.Instance.HideLoading();
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetChatRoomAllList.php");

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
                        IsRoomList = false;

                        UserDialogs.Instance.HideLoading();
                        return;
                    }

                    Rooms.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {                        
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        string[] person_split = dicRes["person_ids"].Split(',');

                        string combine_name = "";
                        string[] split_name = dicRes["name"].Split(',');
                        for (int i = 0; i < split_name.Length; i++)
                        {
                            string[] split = split_name[i].Split(':');
                            if (split[0] != Common.MyInfo.Id)
                            {
                                if (string.IsNullOrEmpty(combine_name) == false)
                                    combine_name += ", ";

                                combine_name += split[1];
                            }
                        }

                        if (person_split.Length > 2)
                        {
                            if (person_split.Length > 4)
                                combine_name += "...";

                            combine_name += "  (" + person_split.Length.ToString() + ")";
                        }

                        ChatRoom room = new ChatRoom()
                        {
                            Id = dicRes["id"],                            
                            Name = dicRes["name"],
                            ViewName = combine_name,
                            GroupId = dicRes["group_id"],
                            GroupName = dicRes["group_name"],
                            PersonIds = dicRes["person_ids"],
                            PersonImgs = dicRes["person_imgs"],
                            LastChatMsg = dicRes["last_chat_msg"],
                            LastTime = dicRes["last_time"],
                            UpdateCnt = short.Parse(dicRes["update_cnt"])
                        };

                        List<string> filter_imgs = new List<string>();
                        string[] split_img = room.PersonImgs.Split(',');
                        for (int i = 0; i < split_img.Length; i++)
                        {
                            string[] split = split_img[i].Split('=');
                            if (split[0] != Common.MyInfo.Id)
                                filter_imgs.Add(split[1]);
                        }

                        if (filter_imgs.Count > 0)
                        {
                            switch (filter_imgs.Count)
                            {
                                case 1: room.Profile_1 = true; break;
                                case 2: room.Profile_2 = true; break;
                                case 3: room.Profile_3 = true; break;
                                case 4: room.Profile_4 = true; break;
                                default: room.Profile_4 = true; break;

                            }

                            for (int i = 0; i < filter_imgs.Count; i++)
                            {
                                switch (i)
                                {
                                    case 0: room.Profile_Person_1 = filter_imgs[i]; break;
                                    case 1: room.Profile_Person_2 = filter_imgs[i]; break;
                                    case 2: room.Profile_Person_3 = filter_imgs[i]; break;
                                    case 3: room.Profile_Person_4 = filter_imgs[i]; break;
                                }
                            }
                        }

                        /*if (string.IsNullOrEmpty(dicRes["person_cnt"]) == false)
                        {
                            int person_cnt = int.Parse(dicRes["person_cnt"]);
                            switch (person_cnt)
                            {
                                case 1:
                                    room.Profile_1 = true;
                                    room.Profile_Person_1 = string.IsNullOrEmpty(dicRes["profile_image_url_1"]) ? "" : dicRes["profile_image_url_1"]; 
                                    break;
                                case 2:
                                    room.Profile_2 = true;
                                    room.Profile_Person_2 = string.IsNullOrEmpty(dicRes["profile_image_url_2"]) ? "" : dicRes["profile_image_url_2"]; 
                                    break;
                                case 3:
                                    room.Profile_3 = true;
                                    room.Profile_Person_3 = string.IsNullOrEmpty(dicRes["profile_image_url_3"]) ? "" : dicRes["profile_image_url_3"]; 
                                    break;
                                case 4:
                                    room.Profile_4 = true;
                                    room.Profile_Person_4 = string.IsNullOrEmpty(dicRes["profile_image_url_4"]) ? "" : dicRes["profile_image_url_4"]; 
                                    break;
                            }
                        }*/

                        await DataChatRoom.UpdateItemAsync(room);
                        Rooms.Add(room);
                    }

                    await DataChatRoom.SortItemAsync();

                    isReload = false;
                    Device.StartTimer(TimeSpan.FromSeconds(5), () =>
                    {
                        isReload = true;
                        return false;
                    });

                    IsEmptyList = Rooms.Count == 0;
                    IsRoomList = Rooms.Count > 0;
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithMoveLogin(ex);
#endif
                UserDialogs.Instance.Toast("채팅을 불러오는데 실패했습니다");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void OnAddChatRoom()
        {
            if (DataGroup.GetCount() == 0)
            {
                await UserDialogs.Instance.AlertAsync("채팅이 가능한 모임이 없습니다", okText: "확인");
                return;
            }

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            Common.eMSType = Common.EMultiSelectType.NewChatRoom;
            await Shell.Current.Navigation.PushModalAsync(new SelectGroupPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnRoomSelected(ChatRoom room)
        {
            if (Common.IsClickActioning || room == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new ChatDetailPage(room.Id, room.Name, room.PersonIds, room.GroupId));
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
