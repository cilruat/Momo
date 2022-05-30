using System.Windows.Input;
using Xamarin.Forms;
using Momo.Views;

namespace Momo.ViewModels
{
    public class JoinViewModel : BaseViewModel
    {
        public ICommand JoinCommand { get; set; }

        private string _userphone;
        private string _password;
        private string _username;
        private string _birthday;
        private bool _areJoinInvalid;

        public JoinViewModel()
        {
            JoinCommand = new Command(() =>
            {
                AreJoinInvalid = !UserJoinChecked(UserPhone, Password);
                if (AreJoinInvalid)
                    return;

                Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");
            });

            _areJoinInvalid = false;
        }

        private bool UserJoinChecked(string userphone, string password)
        {
            if (string.IsNullOrEmpty(userphone) || string.IsNullOrEmpty(password))
                return false;

            return true;
        }

        public string UserPhone
        {
            get => _userphone;
            set
            {
                if (value == _userphone)
                    return;

                _userphone = value;
                OnPropertyChanged(nameof(UserPhone));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (value == _password)
                    return;

                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }

        public string UserName
        {
            get => _username;
            set
            {
                if (value == _username)
                    return;

                _username = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string BirthDay
        {
            get => _birthday;
            set
            {
                if (value == _birthday)
                    return;

                _birthday = value;
                OnPropertyChanged(nameof(BirthDay));
            }
        }

        public bool AreJoinInvalid
        {
            get => _areJoinInvalid;
            set
            {
                if (value == _areJoinInvalid)
                    return;

                _areJoinInvalid = value;
                OnPropertyChanged(nameof(AreJoinInvalid));
            }
        }
    }
}
