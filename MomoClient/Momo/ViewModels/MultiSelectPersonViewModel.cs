using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;
using Xamarin.Essentials;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

using Plugin.ContactService;
using Plugin.ContactService.Shared;

namespace Momo.ViewModels
{
    public partial class MultiSelectPersonViewModel : BaseViewModel
    {
        public ObservableCollection<SelectableItemPerson> Persons { get; }
        public Command LoadPersonsCommand { get; }
        public Command AllSelectCommand { get; }
        public Command<SelectableItemPerson> PersonSelect { get; }
        public Command OptionCommand { get; }

        private HashSet<SelectableItemPerson> HashPersons;

        private string _groupName;
        private string _optionName;
        private string _allSelectIconName;
        
        private bool _allSelect = false;

        public MultiSelectPersonViewModel()
        {
            _allSelect = false;
            AllSelectIconName = _allSelect ? "icon_check.png" : "icon_uncheck.png";

            switch(Common.eMSType)
            {
                case Common.EMultiSelectType.All:           OptionName = "。。。";  break;
                case Common.EMultiSelectType.NewNotice:     OptionName = "";        break;
                case Common.EMultiSelectType.NewChatRoom:   OptionName = "초대";    break;
                case Common.EMultiSelectType.AddPerson:     OptionName = "보내기";  break;
            }

            Persons = new ObservableCollection<SelectableItemPerson>();
            HashPersons = new HashSet<SelectableItemPerson>();

            LoadPersonsCommand = new Command(async() => await ExcuteLoadPersonsCommand());
            
            AllSelectCommand = new Command(OnAllSelect);
            OptionCommand = new Command(OnOption);
            PersonSelect = new Command<SelectableItemPerson>(OnPersonSelect);            
        }

        private async Task ExcuteLoadPersonsCommand()
        {
            try
            {
                if (string.IsNullOrEmpty(Common.ViewGroupID))
                    return;

                IsBusy = true;

                var group = await DataGroup.GetItemAsync(Common.ViewGroupID);
                if (group == null)
                {
                    IsBusy = false;
                    return;
                }

                GroupName = group.Name;

                Persons.Clear();
                HashPersons.Clear();

                if (group.Persons.Count > 0)
                {
                    foreach (Person p in group.Persons)
                    {
                        if (p.Id == Common.MyInfo.Id)
                            continue;

                        var item = new SelectableItemPerson(p);
                        Persons.Add(item);
                        HashPersons.Add(item);
                    }
                }
                else
                {
                    await Task.Delay(500);

                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetPersonList.php");

                    var param = new Dictionary<string, string> { { "group_id", Common.ViewGroupID } };
                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        Persons.Clear();
                        HashPersons.Clear();

                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        if (jsonResponse.StartsWith("null"))
                        {
                            IsBusy = false;
                            return;
                        }

                        JArray jArray = JArray.Parse(jsonResponse);
                        foreach (JObject e in jArray)
                        {
                            Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                            if (dicRes["p_id"] == Common.MyInfo.Id)
                                continue;

                            Person person = new Person
                            {
                                Id = dicRes["p_id"],
                                PersonImage = dicRes["profile_url"],
                                PersonName = dicRes["person_name"],
                                Grade = dicRes["grade"],
                                PhoneNum = dicRes["phone_num"],
                                Etc = dicRes["etc"]
                            };

                            _ = DataPerson.UpdateItemAsync(person);

                            SelectableItemPerson selectPerson = new SelectableItemPerson(person);
                            Persons.Add(selectPerson);
                            HashPersons.Add(selectPerson);

                            if (group != null)
                            {
                                group.Persons.Add(person);
                                await DataGroup.UpdateItemAsync(group);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnOption()
        {
            IEnumerator<SelectableItemPerson> list = Persons.GetEnumerator();
            
            Common.EMultiSelectType eType = Common.eMSType;
            Common.eMSType = Common.EMultiSelectType.All;

            switch (eType)
            {
                case Common.EMultiSelectType.All:
                    string[] actions = { "채팅방 만들기", "문자 보내기", "초대문자 보내기", "연락처 저장" };

                    string action = await UserDialogs.Instance.ActionSheetAsync("", "", "", null, actions);
                    if (action == actions[0])
                        MakeChatRoom(list);
                    else if (action == actions[1])
                        SendSMS(list);
                    else if (action == actions[2])
                        SendAddSMS(list);
                    else if (action == actions[3])
                        SaveContacts(list);

                    break;
                case Common.EMultiSelectType.NewNotice:
                    break;
                case Common.EMultiSelectType.NewChatRoom:
                    MakeChatRoom(list);
                    break;
                case Common.EMultiSelectType.AddPerson:
                    SendAddSMS(list);
                    break;
            }
        }

        private async void MakeChatRoom(IEnumerator<SelectableItemPerson> list)
        {
            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            await Task.Delay(500);

            try
            {
                Group group = await DataGroup.GetItemAsync(Common.ViewGroupID);

                string room_name = "", person_ids = "", person_imgs = "";
                int person_cnt = 0;
                while (list.MoveNext())
                {
                    SelectableItemPerson item = list.Current;
                    if (item.IsSelected)
                    {
                        if (string.IsNullOrEmpty(person_ids) == false)
                        {
                            person_ids += ",";

                            if (person_cnt < 4)
                            {
                                person_imgs += ",";
                                room_name += ",";
                            }
                        }

                        person_ids += item.Person.Id;

                        person_cnt++;
                        if (person_cnt <= 4)
                        {
                            person_imgs += item.Person.Id + "=" + item.Person.PersonImage;
                            room_name += item.Person.Id + ":" + item.Person.PersonName;                    
                        }
                    }
                }

                if (person_cnt == 0)
                {
                    await UserDialogs.Instance.AlertAsync("선택된 멤버가 없습니다", okText: "확인");
                    return;
                }

                person_ids += "," + Common.MyInfo.Id;
                person_imgs += "," + Common.MyInfo.Id + "=" + Common.MyInfo.PersonImage;
                room_name += "," + Common.MyInfo.Id + ":" + Common.MyInfo.PersonName;

                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "CheckChatRoom.php");

                var param = new Dictionary<string, string>
                {
                    { "room_name", room_name },
                    { "group_id", group.Id },
                    { "group_name", group.Name },
                    { "person_ids", person_ids },
                    { "person_imgs", person_imgs },
                    { "person_cnt", person_cnt.ToString() }
                };

                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    string roomID = jsonResponse;
                    if (jsonResponse.Contains("\""))
                        roomID = jsonResponse.Replace("\"", "");

                    _ = Shell.Current.Navigation.PopModalAsync(true);
                    await Shell.Current.Navigation.PushModalAsync(new ChatDetailPage(roomID, room_name, person_ids, group.Id));
                    await Task.Delay(100);

                    UserDialogs.Instance.HideLoading();
                }
                else
                {
                    UserDialogs.Instance.HideLoading();
                    await Common.ErrorAlertWithPopModal();
                    return;
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private async void SendSMS(IEnumerator<SelectableItemPerson> list)
        {
            try
            {
                List<string> recipients = new List<string>();
                while (list.MoveNext())
                {
                    SelectableItemPerson item = list.Current;
                    if (item.IsSelected)
                        recipients.Add(item.Person.PhoneNum);
                }

                if (recipients.Count == 0)
                {
                    await UserDialogs.Instance.AlertAsync("선택된 멤버가 없습니다", okText: "확인");
                    return;
                }

                _ = Shell.Current.Navigation.PopModalAsync(true);

                var message = new SmsMessage("", recipients);
                await Sms.ComposeAsync(message);
            }
            catch (FeatureNotSupportedException)
            {
                await UserDialogs.Instance.AlertAsync("해당 기능을 사용할 수 없는 기종입니다", okText: "확인");
                return;
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private async void SendAddSMS(IEnumerator<SelectableItemPerson> list)
        {
            try
            {
                List<string> recipients = new List<string>();
                while (list.MoveNext())
                {
                    SelectableItemPerson item = list.Current;
                    if (item.IsSelected)
                        recipients.Add(item.Person.PhoneNum);
                }

                if (recipients.Count == 0)
                {
                    await UserDialogs.Instance.AlertAsync("선택된 멤버가 없습니다", okText: "확인");
                    return;
                }

                string desc = "'" + _groupName + "' 모임으로 초대되었습니다. '모모'가 설치되지 않으신분은 아래 링크를 눌러 설치하시면 됩니다.\n\n" + Common.HTTPS_DOWN_LINK;
                var message = new SmsMessage(desc, recipients);
                await Sms.ComposeAsync(message);

                await Shell.Current.Navigation.PopModalAsync(true);
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithPopModal(ex);
#else
                await Shell.Current.Navigation.PopModalAsync(true);
#endif

                UserDialogs.Instance.Toast("초대문자 보내기가 실패했습니다");
            }
        }

        private async void SaveContacts(IEnumerator<SelectableItemPerson> list)
        {
            bool check_selected = false;            
            while (list.MoveNext())
            {
                if (list.Current.IsSelected == false)
                    continue;

                check_selected = true;
                break;
            }

            if (check_selected == false)
            {
                await UserDialogs.Instance.AlertAsync("선택된 멤버가 없습니다", okText: "확인");
                return;
            }

            try
            {
                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

                IList<Contact> contacts = await CrossContactService.Current.GetContactListAsync();

                List<SimpleContact> get_my_list = new List<SimpleContact>();
                foreach (Contact c in contacts)
                {
                    SimpleContact simple = new SimpleContact(c.Name, c.Number);
                    get_my_list.Add(simple);
                }

                int exist_cnt = 0;
                List<KeyValuePair<string, string>> selected = new List<KeyValuePair<string, string>>();

                list.Reset();
                while (list.MoveNext())
                {
                    SelectableItemPerson item = list.Current;
                    if (item.IsSelected == false)
                        continue;
                    
                    int find_idx = get_my_list.FindIndex(x => x.Name == item.Person.PersonName && x.Number == item.Person.PhoneNum);
                    if (find_idx == -1)
                        selected.Add(new KeyValuePair<string, string>(item.Person.PersonName, item.Person.PhoneNum));
                    else
                        exist_cnt++;
                }

                UserDialogs.Instance.HideLoading();

                string desc = "Wi-Fi에 연결되지 않은 경우 데이터 이용료가 발생할 수 있습니다.";
                if (exist_cnt > 0)
                    desc = "이미 저장되어 있는 " + exist_cnt.ToString() + "개의 연락처는 제외됩니다.\n" + desc;

                bool isAccept = await UserDialogs.Instance.ConfirmAsync(desc, "연락처를 저장하시겠습니까?", "연락처 저장", "취소");
                if (isAccept)
                {
                    _ = Shell.Current.Navigation.PopModalAsync(true);
                    DependencyService.Get<ISaveContactService>().SaveContact(selected);
                }
            }
            catch (Exception ex)
            {
                await Common.ErrorAlertWithPopModal(ex);
            }
        }

        private void OnAllSelect()
        {
            _allSelect = !_allSelect;
            AllSelectIconName = _allSelect ? "icon_check.png" : "icon_uncheck.png";

            IEnumerator<SelectableItemPerson> list = Persons.GetEnumerator();
            while (list.MoveNext())
            {
                SelectableItemPerson p = list.Current;

                p.IsSelected = _allSelect;
                p.ChangeImage = _allSelect ? "icon_check.png" : "icon_uncheck.png";
            }
        }

        private void OnPersonSelect(SelectableItemPerson person)
        {
            if (person == null)
                return;

            person.IsSelected = !person.IsSelected;
            person.ChangeImage = person.IsSelected ? "icon_check.png" : "icon_uncheck.png";
        }

        public void OnSearchText(object obj, TextChangedEventArgs e)
        {
            var res = Common.SearchText(e.NewTextValue, HashPersons);

            Persons.Clear();
            foreach (SelectableItemPerson p in res)
                Persons.Add(p);
        }

        public string GroupName
        {
            get => _groupName;
            set => SetProperty(ref _groupName, value);
        }

        public string OptionName
        {
            get => _optionName;
            set => SetProperty(ref _optionName, value);
        }

        public string AllSelectIconName
        {
            get => _allSelectIconName;
            set => SetProperty(ref _allSelectIconName, value);
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
