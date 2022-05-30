using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class NoticeDetailPage : ContentPage
    {
        NoticeDetailViewModel _viewModel;

        public NoticeDetailPage(string noticeId)
        {
            InitializeComponent();
            BindingContext = _viewModel = new NoticeDetailViewModel(noticeId, MediaList);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}