using Rg.Plugins.Popup.Services;
using Momo.Models;
using System;
using System.Globalization;
using System.Windows.Input;
using Xamarin.Forms;

namespace Momo.ViewModels
{
    public class CalendarPickerPopupViewModel : BaseViewModel
    {
        public event Action<CalendarPickerResult> Closed;

        public ICommand SuccessCommand => new Command(async () =>
        {
            Closed?.Invoke(new CalendarPickerResult() { IsSuccess = true, SelectedDate = SelectedDate });
            await PopupNavigation.Instance.PopAsync();
        });

        public ICommand CancelCommand => new Command(async () =>
        {
            Closed?.Invoke(new CalendarPickerResult() { IsSuccess = false });
            await PopupNavigation.Instance.PopAsync();
        });

        private DateTime _monthYear = DateTime.Today;
        public DateTime MonthYear
        {
            get => _monthYear;
            set => SetProperty(ref _monthYear, value);
        }

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        private DateTime _minimumDate = new DateTime(1900, 1, 1);
        public DateTime MinimumDate
        {
            get => _minimumDate;
            set => SetProperty(ref _minimumDate, value);
        }

        private DateTime _maximumDate = DateTime.Today;

        public DateTime MaximumDate
        {
            get => _maximumDate;
            set => SetProperty(ref _maximumDate, value);
        }

        private CultureInfo _culture = CultureInfo.InvariantCulture;
        public CultureInfo Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        public CalendarPickerPopupViewModel(bool setMinimum, DateTime minimumDate)
        {
            Culture = CultureInfo.CreateSpecificCulture("ko-KR");

            DateTime Now = DateTime.Now;

            if (setMinimum)
            {
                SelectedDate = minimumDate;
                MinimumDate = minimumDate;
                MaximumDate = new DateTime(minimumDate.Year + 20, 12, 31);
            }
            else
            {
                SelectedDate = minimumDate;
                MinimumDate = Now;
                MaximumDate = new DateTime(Now.Year + 20, 12, 31);
            }
        }
    }
}
