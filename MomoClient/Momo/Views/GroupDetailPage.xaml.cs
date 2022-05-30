using Momo.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;

namespace Momo.Views
{
    public partial class GroupDetailPage : Xamarin.Forms.TabbedPage
    {
        GroupDetailViewModel _viewModel;

        public GroupDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new GroupDetailViewModel(this);            

            SelectedTabColor = Color.White;
            UnselectedTabColor = new Color(1,1,1,0.4);

            On<Android>().SetToolbarPlacement(ToolbarPlacement.Bottom);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }

        public void MoveCalendarPage()
        {
            CurrentPage = Children[3];
        }
    }
}