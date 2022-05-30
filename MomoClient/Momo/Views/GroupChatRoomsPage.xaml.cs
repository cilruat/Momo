using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class GroupChatRoomsPage : ContentPage
    {
        GroupChatRoomsViewModel _viewModel;

        public GroupChatRoomsPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new GroupChatRoomsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}