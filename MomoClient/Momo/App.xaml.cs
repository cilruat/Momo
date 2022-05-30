using Xamarin.Forms;

using Momo.Services;
using Momo.ViewModels;

namespace Momo
{
    public partial class App : Application
    {

        public App()
        {
            IsInitToken = false;
            IsInitNoti = false;

            InitializeComponent();

            DependencyService.Register<MockDataGroup>();
            DependencyService.Register<MockDataNotice>();
            DependencyService.Register<MockDataChat>();
            DependencyService.Register<MockDataChatRoom>();
            DependencyService.Register<MockDataPerson>();
            DependencyService.Register<MockDataComment>();
            DependencyService.Register<MockDataMediaFile>();
            DependencyService.Register<MockDataSchedule>();

            MainPage = new AppShell();
            _ = new Background();

            Sharpnado.Tabs.Initializer.Initialize(false, true);
            Sharpnado.Shades.Initializer.Initialize(false, true);
        }

        protected override void OnStart()
        {
            IsInitToken = true;
            IsInitNoti = true;

            TapNoticesViewModel.RefreshNotice = true;
            GroupDetailViewModel.RefreshNotice = true;
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
            TapNoticesViewModel.RefreshNotice = true;
            GroupDetailViewModel.RefreshNotice = true;
        }
    }
}
