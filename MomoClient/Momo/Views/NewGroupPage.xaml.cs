using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class NewGroupPage : ContentPage
    {
        NewGroupViewModel _viewModel;

        public NewGroupPage(bool isUpdate)
        {
            InitializeComponent();
            BindingContext = _viewModel = new NewGroupViewModel(GroupImage, CheckPrivate, CheckPublic, isUpdate);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisAppearing();
        }

        private void CheckPrivate_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (CheckPrivate.IsChecked != e.Value)
                _viewModel.OnPrivate();
            else if (CheckPublic.IsChecked == e.Value)
                CheckPublic.IsChecked = !e.Value;
        }

        private void CheckPublic_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (CheckPublic.IsChecked != e.Value)
                _viewModel.OnPublic();
            else if (CheckPrivate.IsChecked == e.Value)
                CheckPrivate.IsChecked = !e.Value;
        }
    }
}