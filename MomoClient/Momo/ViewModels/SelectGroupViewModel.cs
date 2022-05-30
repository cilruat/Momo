using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;

namespace Momo.ViewModels
{
    public class SelectGroupViewModel : BaseViewModel
    {
        public enum EType
        {
            NewNotice,
            NewChatRoom,
        }

        public ObservableCollection<Group> Groups { get; }
        public Command LoadGroupsCommand { get; }
        public Command<Group> SelectGroup { get; }


        public SelectGroupViewModel()
        {
            Groups = new ObservableCollection<Group>();
            LoadGroupsCommand = new Command(ExecuteLoadGroupsCommand);
            SelectGroup = new Command<Group>(OnSelect);
        }

        async void ExecuteLoadGroupsCommand()
        {
            try
            {
                IsBusy =  true;

                var groups = await DataGroup.GetItemsAsync();
                if (groups != null && DataGroup.GetCount() > 0)
                {
                    Groups.Clear();
                    foreach (Group g in groups)
                        Groups.Add(g);
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Common.ErrorAlertWithMoveLogin(ex);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnSelect(Group group)
        {
            if (Common.IsClickActioning || group == null)
                return;

            Common.IsClickActioning = true;

            _ = Shell.Current.Navigation.PopModalAsync(true);

            switch (Common.eMSType)
            {
                case Common.EMultiSelectType.NewNotice:
                    await Shell.Current.Navigation.PushModalAsync(new NewNoticePage(group.Id));
                    break;
                case Common.EMultiSelectType.NewChatRoom:
                    Common.ViewGroupID = group.Id;                    
                    await Shell.Current.Navigation.PushModalAsync(new MultiSelectPersonPage());
                    break;
            }

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
