using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Xamarin.Forms;

using Momo.Models;
using Momo.Services;
using Momo.Views;

namespace Momo.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        static public BaseViewModel Instance { get; private set; }

        public IDataStore<Group> DataGroup => DependencyService.Get<IDataStore<Group>>();
        public IDataStore<Notice> DataNotice => DependencyService.Get<IDataStore<Notice>>();
        public IDataStore<Chat> DataChat => DependencyService.Get<IDataStore<Chat>>();
        public IDataStore<ChatRoom> DataChatRoom => DependencyService.Get<IDataStore<ChatRoom>>();
        public IDataStore<Person> DataPerson => DependencyService.Get<IDataStore<Person>>();
        public IDataStore<Comment> DataComment => DependencyService.Get<IDataStore<Comment>>();
        public IDataStore<MediaFile> DataMediaFile => DependencyService.Get<IDataStore<MediaFile>>();
        public IDataStore<Schedule> DataSchedule => DependencyService.Get<IDataStore<Schedule>>();

        public Command BackCommand { get; }

        public Command<Notice> NoticeTapped { get; }
        public Command<Notice> NoticeGroupTapped { get; }
        public Command<Notice> NoticePersonTapped { get; }

        public Command<Group> GroupTapped { get; }
        public Command<Person> PersonTapped { get; }

        public BaseViewModel()
        {
            Instance = this;

            BackCommand = new Command(OnBackCommand);

            NoticeTapped = new Command<Notice>(OnNoticeSelected);
            NoticeGroupTapped = new Command<Notice>(OnGroupSelected);
            NoticePersonTapped = new Command<Notice>(OnPersonSelected);

            GroupTapped = new Command<Group>(OnGroupSelected);
            PersonTapped = new Command<Person>(OnPersonSelected);
        }

        protected async void OnBackCommand(object obj)
        {
            await Shell.Current.Navigation.PopModalAsync();
        }

        protected async void OnNoticeSelected(Notice notice)
        {
            if (Common.IsClickActioning || notice == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new NoticeDetailPage(notice.Id));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        protected async void OnGroupSelected(Group group)
        {
            if (Common.IsClickActioning || group == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.GoToAsync($"{nameof(GroupDetailPage)}?{nameof(GroupDetailViewModel.GroupId)}={group.Id}");
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        protected async void OnGroupSelected(Notice notice)
        {
            if (Common.IsClickActioning || notice == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.GoToAsync($"{nameof(GroupDetailPage)}?{nameof(GroupDetailViewModel.GroupId)}={notice.GroupId}");
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        protected async void OnPersonSelected(Person person)
        {
            if (Common.IsClickActioning || person == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new PersonDetailPage(person.Id));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        protected async void OnPersonSelected(Notice notice)
        {
            if (Common.IsClickActioning || notice == null)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new PersonDetailPage(notice.PersonId, notice.GroupId));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }


        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public void ClearAllMockData()
        {
            DataGroup.Clear();
            DataChat.Clear();
            DataChatRoom.Clear();
            DataNotice.Clear();
            DataPerson.Clear();
            DataComment.Clear();
        }

        #region INotifyPropertyChanged
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
