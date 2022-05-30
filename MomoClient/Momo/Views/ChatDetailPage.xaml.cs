using Momo.Models;
using Momo.ViewModels;

using Xamarin.Forms;

namespace Momo.Views
{
    public partial class ChatDetailPage : ContentPage
    {
        private readonly ChatDetailViewModel _viewModel;

        public ChatDetailPage(string roomId, string roomName, string personIds, string groupId)
        {
            InitializeComponent();
            BindingContext = _viewModel = new ChatDetailViewModel(roomId, roomName, personIds, groupId);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        public void AddChat(string roomId, Chat chat)
        {
            _viewModel.AddChat(roomId, chat);
        }
    }
}