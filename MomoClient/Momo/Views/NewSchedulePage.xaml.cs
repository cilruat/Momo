using System;
using Momo.ViewModels;
using Xamarin.Forms;

namespace Momo.Views
{
    public partial class NewSchedulePage : ContentPage
    {
        readonly NewScheduleViewModel _viewModel;

        public NewSchedulePage(DateTime selectedDate, bool update = false, string updateId = "")
        {
            InitializeComponent();
            BindingContext = _viewModel = new NewScheduleViewModel(selectedDate, StartDate, EndDate, ColorList, update, updateId);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
        }
    }
}