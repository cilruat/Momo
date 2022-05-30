using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class GroupPersonPage : ContentPage
    {
        readonly GroupPersonViewModel _viewModel;

        public GroupPersonPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new GroupPersonViewModel(this);
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