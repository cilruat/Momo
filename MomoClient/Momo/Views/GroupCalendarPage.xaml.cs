using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class GroupCalendarPage : ContentPage
    {
        GroupCalendarViewModel _viewModel;

        public GroupCalendarPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new GroupCalendarViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}