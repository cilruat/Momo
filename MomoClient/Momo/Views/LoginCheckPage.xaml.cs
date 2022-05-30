using Momo.ViewModels;
using Momo.Services;

using Xamarin.Forms;

namespace Momo.Views
{
    public partial class LoginCheckPage : ContentPage
    {
        public LoginCheckPage()
        {
            InitializeComponent();
            BindingContext = new LoginCheckViewModel();
        }
    }
}