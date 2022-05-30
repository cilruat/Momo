using System;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class ChatDetailViewModel : BaseViewModel
    {
        public ObservableCollection<Chat> Chats { get; }
        public Command LoadChatsCommand { get; }
        public Command LoadBottomCommand { get; }
        public Command SendChatCommand { get; }

        private string _roomName;
        private string _groupName;
        private string _msg;
        
        private string room_id;
        private string room_name;
        private string group_id;
        private string person_ids;
        private string update_cnt;

        private bool isReload = true;
        private int bottom_load_cnt = 0;

        private HashSet<Person> hashPersons;


        public ChatDetailViewModel(string roomId, string roomName, string personIds, string groupId)
        {
            this.room_id = roomId;
            this.room_name = roomName;
            this.group_id = groupId;
            this.person_ids = personIds;

            Chats = new ObservableCollection<Chat>();
            LoadChatsCommand = new Command(ExcuteLoadChatsCommand);
            SendChatCommand = new Command(OnSendChat);

            hashPersons = new HashSet<Person>();

            ExcuteLoadChatsCommand();
        }

        async void ExcuteLoadChatsCommand()
        {
            try
            {
                bottom_load_cnt = 0;

                string[] person_split = person_ids.Split(',');

                string combine_name = "";
                string[] split_name = room_name.Split(',');
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

                RoomName = combine_name;

                var group = await DataGroup.GetItemAsync(group_id);
                if (group != null)
                    GroupName = group.Name;

                bool empty_info = false;
                for (int i = 0; i < person_split.Length; i++)
                {
                    string person_id = person_split[i];
                    var person = await DataPerson.GetItemAsync(person_id);
                    if (person != null)
                    {
                        hashPersons.Add(person);
                        continue;
                    }

                    hashPersons.Clear();
                    empty_info = true;
                    break;
                }

                if (empty_info)
                {
                    HttpClient client_p = new HttpClient();
                    Uri uri_p = new Uri(Common.UrlServerPHP + "GetPersonInfo.php");

                    var param_p = new Dictionary<string, string> { { "person_ids", person_ids } };
                    var content_p = new FormUrlEncodedContent(param_p);

                    Task<HttpResponseMessage> task_response_p = client_p.PostAsync(uri_p, content_p);
                    HttpResponseMessage response_p = task_response_p.Result;
                    if (response_p.IsSuccessStatusCode)
                    {
                        Task<string> task_jsonResponse = response_p.Content.ReadAsStringAsync();
                        string jsonResponse = task_jsonResponse.Result;
                        if (jsonResponse.StartsWith("null"))
                        {
                            IsBusy = false;
                            return;
                        }

                        JArray jArray = JArray.Parse(jsonResponse);
                        foreach(JObject e in jArray)
                        {
                            Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                            Person person_p = new Person
                            {
                                Id = dicRes["p_id"],
                                PersonImage = dicRes["profile_url"],
                                PersonName = dicRes["person_name"],
                                Grade = dicRes["grade"],
                                PhoneNum = dicRes["phone_num"],
                                Etc = dicRes["etc"]
                            };

                            hashPersons.Add(person_p);
                            _ = DataPerson.UpdateItemAsync(person_p);
                        }
                    }
                }

                await Task.Delay(1000);

                if (isReload == false)
                {
                    var chats = await DataChat.GetItemsAsync();
                    if (chats != null)
                    {
                        Chats.Clear();
                        foreach (Chat c in chats)
                        {
                            if (c.RoomId != room_id)
                                continue;

                            Chat chat = new Chat
                            {
                                Id = c.Id,
                                RoomId = c.RoomId,
                                PersonId = c.PersonId,
                                PersonName = c.PersonName,
                                PersonImage = c.PersonImage,
                                Msg = c.Msg,
                                Time = c.Time,
                                MyChat = c.MyChat,
                                OtherChat = c.OtherChat
                            };

                            Chats.Add(chat);
                        }
                    }

                    IsBusy = false;
                    return;
                }

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetChatList.php");

                var param = new Dictionary<string, string> { { "room_id", room_id } };
                var content = new FormUrlEncodedContent(param);

                Task<HttpResponseMessage> response_t = client.PostAsync(uri, content);
                HttpResponseMessage response = response_t.Result;
                if (response.IsSuccessStatusCode)
                {
                    Chats.Clear();

                    Task<string> jsonResponse_t = response.Content.ReadAsStringAsync();
                    string jsonResponse = jsonResponse_t.Result;
                    if (jsonResponse.StartsWith("null"))
                    {
                        IsBusy = false;
                        return;
                    }

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {
                        Chat chat = new Chat();

                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());
                        foreach (KeyValuePair<string, string> pair in dicRes)
                        {
                            chat.Id = dicRes["id"];
                            chat.RoomId = dicRes["room_id"];
                            chat.PersonId = dicRes["person_id"];

                            Person find = hashPersons.Where((Person arg) => arg.Id == chat.PersonId).FirstOrDefault();
                            if (find != null)
                            {
                                chat.PersonName = find.PersonName;
                                chat.PersonImage = find.PersonImage;
                            }

                            chat.Msg = dicRes["msg"];
                            chat.Time = dicRes["time"];

                            bool isMyChat = chat.PersonId == Common.MyInfo.Id;
                            chat.MyChat = isMyChat;
                            chat.OtherChat = !isMyChat;
                        }

                        _ = DataChat.UpdateItemAsync(chat);
                        Chats.Add(chat);
                    }

                    _ = DataChat.SortItemAsync();

                    isReload = false;
                    Device.StartTimer(TimeSpan.FromSeconds(5), () =>
                    {
                        isReload = true;
                        return false;
                    });

                    bottom_load_cnt = Chats.Count;
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnSendChat()
        {
            if (IsBusy)
            {
                await UserDialogs.Instance.AlertAsync("잠시후에 시도해주세요", okText: "확인");
                return;
            }

            if (string.IsNullOrEmpty(_msg))
            {
                await UserDialogs.Instance.AlertAsync("내용을 입력해주세요", okText: "확인");
                return;
            }

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "NewChat.php");

                var param = new Dictionary<string, string>
                {
                    { "room_id", room_id },
                    { "room_name", room_name },
                    { "person_id", Common.MyInfo.Id },
                    { "person_name", Common.MyInfo.PersonName },
                    { "person_ids", person_ids },
                    { "group_id", group_id},
                    { "msg", Msg }
                };

                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null") == false)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonResponse);

                        Chat chat = new Chat
                        {
                            Id = dicRes["id"],
                            RoomId = room_id,
                            PersonId = Common.MyInfo.Id,
                            PersonName = Common.MyInfo.PersonName,
                            PersonImage = Common.MyInfo.PersonImage,
                            Msg = Msg,
                            Time = dicRes["time"],
                            MyChat = true,
                            OtherChat = false
                        };
                        
                        Chats.Add(chat);
                        await DataChat.UpdateItemAsync(chat);

                        Msg = "";
                    }
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        public string Msg
        {
            get => _msg;
            set
            {
                if (value == _msg)
                    return;

                _msg = value;
                OnPropertyChanged(nameof(Msg));
            }
        }

        public string RoomName
        {
            get => _roomName;
            set
            {
                if (value == _roomName)
                    return;

                _roomName = value;
                OnPropertyChanged(nameof(RoomName));
            }
        }

        public string GroupName
        {
            get => _groupName;
            set
            {
                if (value == _groupName)
                    return;

                _groupName = value;
                OnPropertyChanged(nameof(GroupName));
            }
        }

        public async void AddChat(string roomId, Chat chat)
        {
            if (roomId != this.room_id)
                return;

            Person find = hashPersons.Where((Person arg) => arg.Id == chat.PersonId).FirstOrDefault();
            if (find != null)
            {
                chat.PersonName = find.PersonName;
                chat.PersonImage = find.PersonImage;
            }

            bool isMyChat = chat.PersonId == Common.MyInfo.Id;
            chat.MyChat = isMyChat;
            chat.OtherChat = !isMyChat;

            await DataChat.UpdateItemAsync(chat);
            Chats.Add(chat);
        }
    }
}
