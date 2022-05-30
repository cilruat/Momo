using System;
using System.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Momo.Models;

using Xamarin.Forms;

using Acr.UserDialogs;
using Newtonsoft.Json;

using Plugin.ContactService;
using Plugin.ContactService.Shared;

namespace Momo.ViewModels
{
    public enum AddPersonType
    {
        PassOver,
        Directly
    }

    public partial class NewGroupAddPersonViewModel: BaseViewModel
    {
        static public AddPersonType EType = AddPersonType.PassOver;

        public ObservableCollection<SelectableItemPerson> Persons { get; }
        public Command LoadPersonsCommand { get; }
        public Command ConfirmCommand { get; }
        public Command AllSelectCommand { get; }        
        public Command<SelectableItemPerson> PersonSelect { get; }

        private HashSet<SelectableItemPerson> HashPersons;

        private bool _allSelect = false;
        private bool _isSelected = false;
        private string _allSelectIconName;
        private string _selectCount;
        private List<string> alreadyCheckNums = new List<string>();

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string AllSelectIconName
        {
            get => _allSelectIconName;
            set => SetProperty(ref _allSelectIconName, value);
        }

        public string SelectCount
        {
            get => _selectCount;
            set => SetProperty(ref _selectCount, value);
        }

        public NewGroupAddPersonViewModel(List<string> alreadyCheckNums)
        {
            _allSelect = false;
            AllSelectIconName = _allSelect ? "icon_check.png" : "icon_uncheck.png";

            Persons = new ObservableCollection<SelectableItemPerson>();
            HashPersons = new HashSet<SelectableItemPerson>();

            LoadPersonsCommand = new Command(ExcuteLoadPersonsCommand);
            ConfirmCommand = new Command(OnConfirm);

            AllSelectCommand = new Command(OnAllSelect);
            PersonSelect = new Command<SelectableItemPerson>(OnPersonSelect);

            this.alreadyCheckNums = alreadyCheckNums;
            ExcuteLoadPersonsCommand();
        }

        private async void ExcuteLoadPersonsCommand()
        {
            try
            {
                IsBusy = false;

                UserDialogs.Instance.ShowLoading("연락처 가져오는중..", MaskType.Gradient);

                Persons.Clear();
                HashPersons.Clear();

                List<SimpleContact> get_list = UserSettings.UserContacts;
                if (get_list.Count == 0)
                {
                    IList<Contact> contacts = await CrossContactService.Current.GetContactListAsync();
                    foreach (Contact c in contacts)
                    {
                        SimpleContact simple = new SimpleContact(c.Name, c.Number);
                        get_list.Add(simple);
                    }
                }

                var temp = get_list.OrderBy(x => x.Name);
                List<SimpleContact> sort_list = new List<SimpleContact>();
                sort_list = temp.ToList();

                int count = 0;
                bool selected = false;
                for (int i = 0; i < sort_list.Count; i++)
                {
                    string number = sort_list[i].Number;
                    if (string.IsNullOrEmpty(number) == false)
                        number = number.Replace("-", "");

                    Person person = new Person
                    {
                        PersonName = sort_list[i].Name,
                        PhoneNum = number
                    };

                    SelectableItemPerson s_person = new SelectableItemPerson(person);

                    if (alreadyCheckNums.Count > 0)
                    {
                        int find = alreadyCheckNums.FindIndex(x => x == number);
                        if (find > -1)
                        {
                            if (EType == AddPersonType.PassOver)
                            {
                                s_person.IsSelected = true;
                                s_person.ChangeImage = "icon_check.png";

                                count++;
                                if (selected == false)
                                    selected = true;
                            }
                            else
                                continue;
                        }
                    }

                    Persons.Add(s_person);
                    HashPersons.Add(s_person);

                    IsSelected = selected;
                    SelectCount = count.ToString();
                }

                UserDialogs.Instance.HideLoading();
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.HideLoading();
                await Common.ErrorAlertWithPopModal(ex);
                return;
            }
        }

        private void OnAllSelect()
        {
            _allSelect = !_allSelect;
            AllSelectIconName = _allSelect ? "icon_check.png" : "icon_uncheck.png";

            int count = 0;
            IEnumerator<SelectableItemPerson> list = HashPersons.GetEnumerator();
            while (list.MoveNext())
            {
                SelectableItemPerson p = list.Current;

                p.IsSelected = _allSelect;
                p.ChangeImage = _allSelect ? "icon_check.png" : "icon_uncheck.png";
                count++;
            }

            IsSelected = _allSelect;
            SelectCount = count.ToString();
        }

        private void OnPersonSelect(SelectableItemPerson person)
        {
            if (person == null)
                return;

            person.IsSelected = !person.IsSelected;
            person.ChangeImage = person.IsSelected ? "icon_check.png" : "icon_uncheck.png";

            int count = 0;
            bool selected = false;
            IEnumerator<SelectableItemPerson> list = HashPersons.GetEnumerator();
            while (list.MoveNext())
            {
                if (list.Current.IsSelected)
                {
                    count++;
                    if (selected == false)
                        selected = true;
                }
            }

            IsSelected = selected;
            SelectCount = count.ToString();
        }

        private async void OnConfirm()
        {
            List<string> listPhoneNum = new List<string>();

            IEnumerator<SelectableItemPerson> list = HashPersons.GetEnumerator();
            while (list.MoveNext())
            {
                if (list.Current.IsSelected)
                    listPhoneNum.Add(list.Current.Person.PhoneNum);
            }

            if (EType == AddPersonType.PassOver)
            {
                NewGroupViewModel.listPhoneNum.Clear();
                NewGroupViewModel.listPhoneNum = listPhoneNum;

                await Shell.Current.Navigation.PopModalAsync(true);
            }
            else
            {
                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

                try
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "AddPerson.php");

                    string strPersonNums = "";
                    for (int i = 0; i < listPhoneNum.Count; i++)
                    {
                        if (string.IsNullOrEmpty(strPersonNums) == false)
                            strPersonNums += ",";

                        strPersonNums += listPhoneNum[i];                        
                    }

                    var param = new Dictionary<string, string>
                    {
                        { "group_id", Common.ViewGroupID },
                        { "personNums", strPersonNums }
                    };
                    var content = new FormUrlEncodedContent(param);

                    HttpResponseMessage response = await client.PostAsync(uri, content);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        if (string.IsNullOrEmpty(result))
                        {
                            UserDialogs.Instance.HideLoading();
                            await UserDialogs.Instance.AlertAsync("멤버를 추가하는데 실패하였습니다", okText: "확인");
                            return;
                        }

                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(result);

                        bool success = dicRes["status"] == "Success";
                        string strSuccess = success ?
                            "멤버를 추가하였습니다" :
                            "멤버를 추가하는데 실패하였습니다\n다시 진행해주세요";

                        if (success)
                        {
                            Group group = await DataGroup.GetItemAsync(Common.ViewGroupID);
                            if (group != null)
                            {
                                group.Count += listPhoneNum.Count;
                                await DataGroup.UpdateItemAsync(group);
                                GroupDetailViewModel.RefreshPersonCnt = true;
                            }
                        }

                        UserDialogs.Instance.HideLoading();

                        await UserDialogs.Instance.AlertAsync(strSuccess, okText: "확인");
                        await Shell.Current.Navigation.PopModalAsync(true);
                    }
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.HideLoading();
                    await Common.ErrorAlertWithPopModal(ex);
                    return;
                }
            }
        }

        public void OnAppearing() {}

        public void OnSearchText(object obj, TextChangedEventArgs e)
        {
            var res = Common.SearchText(e.NewTextValue, HashPersons);

            Persons.Clear();
            foreach (SelectableItemPerson p in res)
                Persons.Add(p);
        }
    }
}
