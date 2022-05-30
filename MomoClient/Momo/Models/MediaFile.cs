using System.ComponentModel;
using Xamarin.Forms;
using Momo.ViewModels;
using Acr.UserDialogs;

namespace Momo.Models
{
    public enum MediaFileType
    {
        Image,
        Video
    }

    public class MediaFile : INotifyPropertyChanged
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string NoticeId { get; set; }

        public string Path { get; set; }
        public MediaFileType Type { get; set; }

        public Command MediaTapCommand { get; }

        private string BASIC_COLOR = "Transparent";
        private string CHANGE_COLOR = "DodgerBlue";

        private string _changeCol;
        private NewNoticeViewModel _viewModel;

        public MediaFile()
        {
            ChangeColor = BASIC_COLOR;
            MediaTapCommand = new Command(OnMediaTap);
        }

        public MediaFile(string path, MediaFileType type, NewNoticeViewModel viewModel)
        {
            Path = path;
            Type = type;
            _viewModel = viewModel;

            ChangeColor = BASIC_COLOR;
            MediaTapCommand = new Command(OnMediaTap);
        }

        private async void OnMediaTap()
        {
            ChangeColor = CHANGE_COLOR;

            string[] actions = { "삭제" };
            string action = await UserDialogs.Instance.ActionSheetAsync("", "", "", null, actions);
            if (action == actions[0])
                _viewModel.Media.Remove(this);

            ChangeColor = BASIC_COLOR;
        }

        public string ChangeColor
        {
            get => _changeCol;
            set
            {
                if (value == _changeCol)
                    return;

                _changeCol = value;
                PropertyChanged(this, new PropertyChangedEventArgs("ChangeColor"));
            }
        }

        public void SetViewModel(NewNoticeViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };
    }
}
