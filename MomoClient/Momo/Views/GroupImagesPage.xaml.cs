using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class GroupImagesPage : ContentPage
    {
        GroupImagesViewModel _viewModel;

        public GroupImagesPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new GroupImagesViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}