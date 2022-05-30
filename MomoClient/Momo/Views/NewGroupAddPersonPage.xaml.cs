using System.Collections.Generic;
using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class NewGroupAddPersonPage : ContentPage
    {
        readonly NewGroupAddPersonViewModel _viewModel;

        public NewGroupAddPersonPage(List<string> alreadyCheckNums)
        {
            InitializeComponent();
            BindingContext = _viewModel = new NewGroupAddPersonViewModel(alreadyCheckNums);
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