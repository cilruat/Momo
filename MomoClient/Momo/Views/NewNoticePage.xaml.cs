using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class NewNoticePage : ContentPage
    {
        readonly NewNoticeViewModel _viewModel;

        public NewNoticePage(string groupID, bool update = false, string updateNoticeId = "")
        {
            InitializeComponent();
            BindingContext = _viewModel = new NewNoticeViewModel(groupID, update, updateNoticeId);

            _viewModel.InputEditor = NewNoticeEditor;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnDisappearing();
        }
    }
}