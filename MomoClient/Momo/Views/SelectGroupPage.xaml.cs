using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class SelectGroupPage : ContentPage
    {
        SelectGroupViewModel _viewModel;

        public SelectGroupPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new SelectGroupViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}