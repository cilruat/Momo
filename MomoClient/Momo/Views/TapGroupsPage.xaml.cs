using Momo.ViewModels;
using Xamarin.Forms;

using Acr.UserDialogs;

namespace Momo.Views
{
    public partial class TapGroupsPage : ContentPage
    {
        TapGroupsViewModel _viewModel;

        public TapGroupsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new TapGroupsViewModel();
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
            bool isAccept = await UserDialogs.Instance.ConfirmAsync("종료하시겠습니까?", okText: "예", cancelText: "아니오");
            if (isAccept)
                DependencyService.Get<ICloseAppService>().CloseApplication();
        }
    }
}