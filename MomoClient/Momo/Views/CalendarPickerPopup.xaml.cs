using Rg.Plugins.Popup.Pages;
using Momo.Models;
using Momo.ViewModels;
using System;
using Xamarin.Forms.Xaml;

namespace Momo.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarPickerPopup : PopupPage
    {
        private readonly Action<CalendarPickerResult> _onClosedPopup;
        CalendarPickerPopupViewModel _veiwModel;

        public CalendarPickerPopup(Action<CalendarPickerResult> onClosedPopup, bool setMinimum, DateTime minimumDate)
        {
            _onClosedPopup = onClosedPopup;
            InitializeComponent();
            BindingContext = _veiwModel = new CalendarPickerPopupViewModel(setMinimum, minimumDate);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is CalendarPickerPopupViewModel vm)
                vm.Closed += _onClosedPopup;
        }

        protected override void OnDisappearing()
        {
            if (BindingContext is CalendarPickerPopupViewModel vm)
                vm.Closed -= _onClosedPopup;

            base.OnDisappearing();
        }
    }
}