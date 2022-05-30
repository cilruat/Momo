using Momo.Models;
using Momo.ViewModels;

using Xamarin.Forms;

namespace Momo.Views
{
    public partial class PersonDetailPage : ContentPage
    {
        PersonDetailViewModel _viewModel;

        public PersonDetailPage(string id)
        {
            InitializeComponent();
            BindingContext = _viewModel = new PersonDetailViewModel(id, "");
        }

        public PersonDetailPage(string id, string groupId)
        {
            InitializeComponent();
            BindingContext = _viewModel = new PersonDetailViewModel(id, groupId);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}