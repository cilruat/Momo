using System;
using System.Net.Http;
using System.Threading.Tasks;

using Xamarin.Forms;

using Momo.Views;
using Momo.Models;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class ScheduleDetailViewModel : BaseViewModel
    {
        public Command OptionCommand { get; }

        private bool _showOption;
        public bool ShowOption
        {
            get => _showOption;
            set => SetProperty(ref _showOption, value);
        }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string _desc;
        public string Desc
        {
            get => _desc;
            set => SetProperty(ref _desc, value);
        }

        private string _timeDesc1;
        public string TimeDesc1
        {
            get => _timeDesc1;
            set => SetProperty(ref _timeDesc1, value);
        }

        private string _timeDesc2;
        public string TimeDesc2
        {
            get => _timeDesc2;
            set => SetProperty(ref _timeDesc2, value);
        }

        private string scheduleId;

        public ScheduleDetailViewModel(string Id)
        {
            scheduleId = Id;

            OptionCommand = new Command(OnOption);
            ShowOption = Common.MyInfo.Leader;

            OnLoad();
        }

        private async void OnLoad()
        {
            Schedule schedule = await DataSchedule.GetItemAsync(scheduleId);
            if (schedule != null)
            {
                Name = schedule.Title;
                Desc = schedule.Desc;

                DateTime start = schedule.StartDate;
                DateTime end = schedule.EndDate;

                TimeDesc1 = start.ToString("yyyy년 M월 d일 ddd요일 tt h:mm");
                if (end.Year != 1)
                {
                    GroupCalendarViewModel.SimpleDateTime startTime = new GroupCalendarViewModel.SimpleDateTime(start);
                    GroupCalendarViewModel.SimpleDateTime endTime = new GroupCalendarViewModel.SimpleDateTime(end);

                    TimeSpan timeDiff = endTime.date - startTime.date;
                    if (timeDiff.Days == 0)
                    {
                        TimeDesc1 += " ~ " + end.ToString("tt h:mm");
                    }
                    else
                    {
                        TimeDesc1 += " ~";
                        TimeDesc2 = end.ToString("yyyy년 M월 d일 ddd요일 tt h:mm");
                    }
                }
            }
        }

        private async void OnOption()
        {
            string[] actions = { "수정", "삭제하기" };
            string action = await UserDialogs.Instance.ActionSheetAsync("", "", "", null, actions);
            if (action == actions[0])
                OnUpdate();
            else if (action == actions[1])
                OnDelete();
        }

        private async void OnUpdate()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            _ = Shell.Current.Navigation.PopModalAsync(true);
            await Shell.Current.Navigation.PushModalAsync(new NewSchedulePage(DateTime.Today, true, scheduleId));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async void OnDelete()
        {
            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };
                MultipartFormDataContent form = new MultipartFormDataContent();

                StringContent strContent = new StringContent(scheduleId);
                form.Add(strContent, "id");

                Uri uri = new Uri(Common.UrlServerPHP + "DeleteSchedule.php");
                HttpResponseMessage response = await client.PostAsync(uri, form);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    bool success = bool.Parse(jsonResponse);
                    string strSuccess = success ?
                        "일정을 삭제하였습니다" :
                        "일정을 삭제하는데 실패하였습니다\n다시 작성해주세요";

                    if (success)
                    {
                        GroupCalendarViewModel.RefreshCalendar = true;
                        await DataSchedule.DeleteItemAsync(scheduleId);
                    }
                        

                    Background.Instance.GetScheduleList();

                    UserDialogs.Instance.Toast(strSuccess);
                    await Shell.Current.Navigation.PopModalAsync(true);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithPopModal(ex);
#endif
                UserDialogs.Instance.Toast("일정을 삭제하는데 실패했습니다");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        public void OnAppearing() {}
    }
}
