using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class MultiSelectPersonPage : ContentPage
    {
        readonly MultiSelectPersonViewModel _viewModel;

        public MultiSelectPersonPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MultiSelectPersonViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        private void search_TextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.OnSearchText(sender, e);
        }
    }
}