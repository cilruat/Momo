using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class TapNoticesPage : ContentPage
    {
        readonly TapNoticesViewModel _viewModel;

        public TapNoticesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TapNoticesViewModel();
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