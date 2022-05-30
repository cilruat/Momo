using Momo.ViewModels;
using Xamarin.Forms;

using Acr.UserDialogs;

namespace Momo.Views
{
    public partial class TapSettingsPage : ContentPage
    {
        readonly TapSettingsViewModel _viewModel;

        public TapSettingsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TapSettingsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override bool OnBackButtonPressed()
        {
            OnBackButton();
            return true;
        }

        private async void OnBackButton()
        {
            await Shell.Current.GoToAsync($"//{nameof(TapGroupsPage)}");
        }
    }
}