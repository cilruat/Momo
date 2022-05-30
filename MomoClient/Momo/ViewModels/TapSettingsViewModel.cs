using System;
using System.Threading.Tasks;

using Momo.Views;
using Xamarin.Forms;

using Plugin.GoogleClient;
using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class TapSettingsViewModel : BaseViewModel
    {
        internal static TapSettingsViewModel Single { get; private set; }
        
        public Command MyInfoCommand { get; }
        public Command MyWriteCommand { get; }
        public Command LogoutCommand { get; }
        public Command NoticeCommand { get; }
        public Command ReportCommand { get; }

        private string _image;
        private string _name;
        private string _phoneNum;        
        private string _appVersion;

        public TapSettingsViewModel()
        {
            Single = this;

            MyInfoCommand = new Command(OnMyInfo);
            MyWriteCommand = new Command(OnMyWrite);
            LogoutCommand = new Command(OnLogout);
            NoticeCommand = new Command(OnNotice);
            ReportCommand = new Command(OnReport);
        }

        public string MyImage
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string PhoneNum
        {
            get => _phoneNum;
            set => SetProperty(ref _phoneNum, value);
        }

        public string AppVersion
        {
            get => _appVersion;
            set => SetProperty(ref _appVersion, value);
        }

        public void LoadInfo()
        {
            AppVersion = "v" + Common.GetVersionSTR();

            if (Common.MyInfo == null)
                return;

            MyImage = Common.MyInfo.PersonImage;
            PhoneNum = Common.MyInfo.PhoneNum;
            Name = Common.MyInfo.PersonName;
        }

        private async void OnMyInfo()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new MyInfoDetailPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnMyWrite()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new MyWriteTabsPage());
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnLogout()
        {
            bool isAccept = await UserDialogs.Instance.ConfirmAsync("정말 로그아웃하시겠습니까?", okText: "예", cancelText:"아니오");
            if (isAccept)
            {
                UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

                if (CrossGoogleClient.Current.IsLoggedIn)
                {
                    CrossGoogleClient.Current.OnLogout += OnLogoutCompleted;
                    CrossGoogleClient.Current.Logout();
                }
                else
                    ClearAndGoLoginCheck();
            }
        }

        private void OnLogoutCompleted(object sender, EventArgs args)
        {
            CrossGoogleClient.Current.OnLogout -= OnLogoutCompleted;
            ClearAndGoLoginCheck();
        }

        private async void ClearAndGoLoginCheck()
        {
            ClearAllMockData();
            UserSettings.ClearAllData();

            await Task.Delay(300);
            UserDialogs.Instance.HideLoading();

            await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
        }

        private async void OnNotice()
        {
            await UserDialogs.Instance.AlertAsync("준비중인 기능입니다", okText: "확인");
        }

        private async void OnReport()
        {
            await UserDialogs.Instance.AlertAsync("준비중인 기능입니다", okText: "확인");
        }

        public void OnAppearing()
        {
            LoadInfo();
        }
    }
}
