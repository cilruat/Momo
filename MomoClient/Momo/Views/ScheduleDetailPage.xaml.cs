using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class ScheduleDetailPage : ContentPage
    {
        readonly ScheduleDetailViewModel _viewModel;

        public ScheduleDetailPage(string Id)
        {
            InitializeComponent();
            BindingContext = _viewModel = new ScheduleDetailViewModel(Id);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}