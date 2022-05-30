using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
            BindingContext = new LoginViewModel(this);

            UserPhoneEntry.Completed += (sender, args) => { UserNameEntry.Focus(); };
        }
    }
}