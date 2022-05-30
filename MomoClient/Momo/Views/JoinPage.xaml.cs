using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class JoinPage : ContentPage
    {
        public JoinPage()
        {
            InitializeComponent();
            BindingContext = new JoinViewModel();

            UserPhoneEntry.Completed += (sender, args) => { PasswordEntry.Focus(); };
            PasswordEntry.Completed += (sender, args) => { UserNameEntry.Focus(); };
            UserNameEntry.Completed += (sender, args) => { BirthDayEntry.Focus(); };
        }
    }
}