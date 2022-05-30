using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class MyWriteTabsPage : ContentPage
    {
        readonly MyWriteTabsViewModel _viewModel;

        public MyWriteTabsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MyWriteTabsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}