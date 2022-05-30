using Momo.ViewModels;
using Xamarin.Forms;

using Acr.UserDialogs;

namespace Momo.Views
{
    public partial class TapChatRoomsPage : ContentPage
    {
        TapChatRoomsViewModel _viewModel;

        public TapChatRoomsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TapChatRoomsViewModel();
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