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
    public class GroupPersonViewModel : BaseViewModel
    {
        public ObservableCollection<Person> Persons { get; }
        public Command LoadPersonsCommand { get; }
        public Command AddPersonCommand { get; }
        public Command MultiSelectCommand { get; }

        private HashSet<SelectableItemPerson> HashPersons;

        private string searchText;
        private bool isReload = true;
        private bool isLeader = false;

        private Page page;

        public GroupPersonViewModel(Page page)
        {
            Persons = new ObservableCollection<Person>();
            HashPersons = new HashSet<SelectableItemPerson>();

            LoadPersonsCommand = new Command(ExecuteLoadPersonsCommand);
            AddPersonCommand = new Command(OnAddPerson);
            MultiSelectCommand = new Command(OnMultiSelect);

            this.page = page;
            Title = "멤버 0명";
        }

        async void ExecuteLoadPersonsCommand()
        {
            if (string.IsNullOrEmpty(searchText) == false)
            {
                IsBusy = false;
                return;
            }            

            try
            {
                IsBusy = true;                

                Persons.Clear();
                HashPersons.Clear();

                Group group = await DataGroup.GetItemAsync(Common.ViewGroupID);
                if (group != null)
                {
                    isLeader = group.LeaderId == Common.MyInfo.Id;
                    if (isLeader)
                    {
                        page.ToolbarItems.Clear();

                        ToolbarItem item = new ToolbarItem
                        {
                            IconImageSource = ImageSource.FromFile("Icon_plus.png"),
                            Command = AddPersonCommand
                        };

                        page.ToolbarItems.Add(item);

                        item = new ToolbarItem
                        {
                            IconImageSource = ImageSource.FromFile("Icon_option.png"),
                            Command = MultiSelectCommand
                        };
                        page.ToolbarItems.Add(item);
                    }

                    if (isReload == false)
                    {
                        foreach (Person p in group.Persons)
                        {
                            p.Leader = group.LeaderId == p.Id;

                            Persons.Add(p);
                            HashPersons.Add(new SelectableItemPerson(p));
                        }

                        IsBusy = false;
                        return;
                    }
                    else
                        group.Persons.Clear();
                }

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

                    int personCnt = 0;

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

                            person.Leader = group.LeaderId == person.Id;
                        }

                        Persons.Add(person);
                        HashPersons.Add(new SelectableItemPerson(person));

                        personCnt++;
                        Title = "멤버 " + personCnt.ToString() + "명";
                    }

                    isReload = false;
                    Device.StartTimer(TimeSpan.FromSeconds(5), () => {
                        isReload = true;
                        return false;
                    });
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

        private async void OnAddPerson(object obj)
        {
            if (IsBusy)
            {
                await UserDialogs.Instance.AlertAsync("멤버 정보를 가져오는 중입니다\n잠시후에 시도해주세요", okText: "확인");
                return;
            }

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            List<string> listNumber = new List<string>();
            Group group = await DataGroup.GetItemAsync(Common.ViewGroupID);
            if (group != null)
            {
                for (int i = 0; i < group.Persons.Count; i++)
                    listNumber.Add(group.Persons[i].PhoneNum);
            }

            NewGroupAddPersonViewModel.EType = AddPersonType.Directly;
            await Shell.Current.Navigation.PushModalAsync(new NewGroupAddPersonPage(listNumber));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnMultiSelect(object obj)
        {
            if (IsBusy)
            {
                await UserDialogs.Instance.AlertAsync("멤버 정보를 가져오는 중입니다\n잠시후에 시도해주세요", okText: "확인");
                return;
            }

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new MultiSelectPersonPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }        

        public void OnSearchText(object obj, TextChangedEventArgs e)
        {
            var res = Common.SearchText(e.NewTextValue, HashPersons);

            Persons.Clear();
            foreach (SelectableItemPerson p in res)
                Persons.Add(p.Person);
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                if (value == searchText)
                    return;

                searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }
    }
}
