using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class MyInfoDetailPage : ContentPage
    {
        readonly MyInfoDetailViewModel _viewModel;

        public MyInfoDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MyInfoDetailViewModel(MyImage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}