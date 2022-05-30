using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using Momo.Views;

using Xamarin.Forms;

using Acr.UserDialogs;
using Rg.Plugins.Popup.Services;

namespace Momo.ViewModels
{
    public class NewScheduleViewModel : BaseViewModel
    {
        public Command ConfirmCommand { get; }
        public Command StartDatePickCommand { get; }
        public Command EndDatePickCommand { get; }
        public Command StartTimePickCommand { get; }
        public Command EndTimePickCommand { get; }
        public Command SelectAlarmCommand { get; }

        private DateTime startDate = DateTime.Today;
        private DateTime endDate = DateTime.Today;

        private Label labelStartDate;
        private Label labelEndDate;

        private bool isSelectEndDate = false;
        private int alarmTimeIdx = 0;

        private bool _update;
        private string _updateId;

        private TimeSpan _startTime = TimeSpan.Zero;
        public TimeSpan SelectStartTime
        {
            get => _startTime;
            set
            {
                StartTimeColor = Color.Black;
                SetProperty(ref _startTime, value);
            }
        }

        private TimeSpan _endTime = TimeSpan.Zero;
        public TimeSpan SelectEndTime
        {
            get => _endTime;
            set
            {
                EndTimeColor = Color.Black;
                SetProperty(ref _endTime, value);
            }
        }

        private Color _startTimeColor;
        public Color StartTimeColor
        {
            get => _startTimeColor;
            set => SetProperty(ref _startTimeColor, value);
        }

        private Color _endTimeColor;
        public Color EndTimeColor
        {
            get => _endTimeColor;
            set => SetProperty(ref _endTimeColor, value);
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

        private bool _showPush;
        public bool ShowPush
        {
            get => _showPush;
            set => SetProperty(ref _showPush, value);
        }

        private bool _togglePush;
        public bool TogglePush
        {
            get => _togglePush;
            set => SetProperty(ref _togglePush, value);
        }

        private string _alarmDesc;
        public string AlarmDesc
        {
            get => _alarmDesc;
            set => SetProperty(ref _alarmDesc, value);
        }

        public NewScheduleViewModel(DateTime selectedDate, Label labelStartDate, Label labelEndDate, StackLayout colorList, bool update, string updateId)
        {
            _update = update;
            _updateId = updateId;

            ConfirmCommand = new Command(OnConfirm);
            StartDatePickCommand = new Command<string>(OnDatePick);
            EndDatePickCommand = new Command<string>(OnDatePick);
            SelectAlarmCommand = new Command(OnAlarm);

            MakeColor(colorList);
            OnLoad(selectedDate, labelStartDate, labelEndDate);
        }        

        private async void OnLoad(DateTime selectedDate, Label labelStartDate, Label labelEndDate)
        {
            this.labelStartDate = labelStartDate;
            this.labelEndDate = labelEndDate;

            if (_update == false)
            {
                startDate = selectedDate;

                labelStartDate.Text = MakeDateToString(startDate);
                labelStartDate.TextColor = Color.Black;

                SelectStartTime = new TimeSpan(12, 00, 00);
                StartTimeColor = Color.Black;

                AlarmDesc = "없음";
            }
            else
            {
                var schedule = await DataSchedule.GetItemAsync(_updateId);
                if (schedule != null)
                {
                    Name = schedule.Title;
                    Desc = schedule.Desc;

                    startDate = schedule.StartDate;
                    endDate = schedule.EndDate;

                    labelStartDate.Text = MakeDateToString(startDate);
                    labelStartDate.TextColor = Color.Black;

                    SelectStartTime = new TimeSpan(startDate.Hour, startDate.Minute, startDate.Second);
                    StartTimeColor = Color.Black;

                    if (endDate.Year != 1)
                    {
                        labelEndDate.Text = MakeDateToString(endDate);
                        labelEndDate.TextColor = Color.Black;

                        SelectEndTime = new TimeSpan(endDate.Hour, endDate.Minute, endDate.Second);
                        EndTimeColor = Color.Black;

                        isSelectEndDate = true;
                    }

                    Common.EAlarmTimeType eType = schedule.eType;
                    alarmTimeIdx = (int)eType;
                    AlarmDesc = Common.listAlarmTimeSTR[alarmTimeIdx];

                    for (int i = 0; i < imgList.Count; i++)
                        imgList[i].IsVisible = i == schedule.ColIdx;
                }
            }

            ShowPush = !_update;
            TogglePush = !_update;
        }

        private List<Image> imgList = new List<Image>();

        private void MakeColor(StackLayout colorList)
        {
            imgList.Clear();
            colorList.Children.Clear();

            for (int i = 0; i < 7; i++)
            {
                Frame frame = new Frame()
                {
                    Padding = 0,
                    HasShadow = false,
                    HeightRequest = 30,
                    WidthRequest = 30,
                    CornerRadius = 30,
                    BackgroundColor = Common.ScheduleColors[i]
                };

                Image img = new Image()
                {
                    IsVisible = i == 0,
                    Source = "icon_check_white.png",
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand
                };

                imgList.Add(img);
                frame.Content = img;

                int idx = i;
                TapGestureRecognizer tapGesture = new TapGestureRecognizer();
                tapGesture.Tapped += (s, args) => { SelectColor(idx); };
                
                frame.GestureRecognizers.Add(tapGesture);
                
                colorList.Children.Add(frame);
            }
        }

        private void SelectColor(int idxCol)
        {
            for (int i = 0; i < imgList.Count; i++)
                imgList[i].IsVisible = i == idxCol;
        }

        private string MakeDateToString(DateTime date)
        {
            string make = date.ToString("yyyy.M.dd");
            string getKorDay = "";
            switch (date.DayOfWeek)
            {
                case DayOfWeek.Monday:      getKorDay = "(월)";  break;
                case DayOfWeek.Tuesday:     getKorDay = "(화)";  break;
                case DayOfWeek.Wednesday:   getKorDay = "(수)";  break;
                case DayOfWeek.Thursday:    getKorDay = "(목)";  break;
                case DayOfWeek.Friday:      getKorDay = "(금)";  break;
                case DayOfWeek.Saturday:    getKorDay = "(토)";  break;
                case DayOfWeek.Sunday:      getKorDay = "(일)";  break;
            }

            return make + " " + getKorDay;
        }

        private async void OnConfirm()
        {
            if (string.IsNullOrEmpty(_name))
            {
                await UserDialogs.Instance.AlertAsync("제목을 입력해주세요", okText: "확인");
                return;
            }

            UserDialogs.Instance.ShowLoading("", MaskType.Gradient);

            try
            {
                HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };
                MultipartFormDataContent form = new MultipartFormDataContent();

                string startDateToString = startDate.ToString("yyyy-MM-dd");
                startDateToString += " ";
                startDateToString += _startTime.ToString(@"hh\:mm");

                string endDateToString = "";
                if (isSelectEndDate)
                {
                    endDateToString = endDate.ToString("yyyy-MM-dd");
                    endDateToString += " ";
                    endDateToString += _endTime.ToString(@"hh\:mm");
                }

                if (_update == false)
                {
                    StringContent strContent = new StringContent(Common.ViewGroupID);
                    form.Add(strContent, "group_id");

                    strContent = new StringContent(Common.ViewGroupName);
                    form.Add(strContent, "group_name");

                    strContent = new StringContent(Common.MyInfo.Id);
                    form.Add(strContent, "person_id");

                    strContent = new StringContent(Common.MyInfo.PersonName);
                    form.Add(strContent, "person_name");

                    strContent = new StringContent(_name);
                    form.Add(strContent, "name");

                    string description = string.IsNullOrEmpty(_desc) ? "" : _desc;
                    strContent = new StringContent(description);
                    form.Add(strContent, "description");

                    strContent = new StringContent(startDateToString);
                    form.Add(strContent, "start_date");

                    strContent = new StringContent(endDateToString);
                    form.Add(strContent, "end_date");

                    int findIdx = imgList.FindIndex(x => x.IsVisible);
                    strContent = new StringContent(findIdx.ToString());
                    form.Add(strContent, "color");

                    strContent = new StringContent(alarmTimeIdx.ToString());
                    form.Add(strContent, "alarm_type");

                    strContent = new StringContent(_togglePush.ToString());
                    form.Add(strContent, "toggle_push");
                }
                else
                {
                    StringContent strContent = new StringContent(_updateId);
                    form.Add(strContent, "id");

                    strContent = new StringContent(_name);
                    form.Add(strContent, "name");

                    string description = string.IsNullOrEmpty(_desc) ? "" : _desc;
                    strContent = new StringContent(description);
                    form.Add(strContent, "description");

                    strContent = new StringContent(startDateToString);
                    form.Add(strContent, "start_date");

                    strContent = new StringContent(endDateToString);
                    form.Add(strContent, "end_date");

                    int findIdx = imgList.FindIndex(x => x.IsVisible);
                    strContent = new StringContent(findIdx.ToString());
                    form.Add(strContent, "color");

                    strContent = new StringContent(alarmTimeIdx.ToString());
                    form.Add(strContent, "alarm_type");
                }

                string php = _update ? "UpdateSchedule.php" : "NewSchedule.php";
                Uri uri = new Uri(Common.UrlServerPHP + php);
                HttpResponseMessage response = await client.PostAsync(uri, form);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    bool success = bool.Parse(jsonResponse);

                    Background.Instance.GetScheduleList();

                    if (success)
                    {
                        GroupCalendarViewModel.RefreshCalendar = true;
                        await Task.Delay(1000);
                    }

                    string strSuccess = "";
                    if (success)
                        strSuccess = _update ? "일정이 수정되었습니다" : "일정을 등록하였습니다";
                    else
                        strSuccess = _update ? "일정을 수정하는데 실패하였습니다\n다시 작성해주세요" : "일정을 등록하는데 실패하였습니다\n다시 작성해주세요"; ;

                    UserDialogs.Instance.Toast(strSuccess);
                    await Shell.Current.Navigation.PopModalAsync(true);
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                await Common.ErrorAlertWithPopModal(ex);
#endif
                UserDialogs.Instance.Toast("일정을 등록하는데 실패했습니다");
            }
            finally
            {
                UserDialogs.Instance.HideLoading();
            }
        }

        private async void OnDatePick(string strStart)
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await PopupNavigation.Instance.PushAsync(
                new CalendarPickerPopup((calendarPickerResult) =>
                {
                    if (calendarPickerResult.IsSuccess)
                    {
                        string date = MakeDateToString(calendarPickerResult.SelectedDate);

                        if (strStart == "true")
                        {
                            startDate = calendarPickerResult.SelectedDate;
                            labelStartDate.Text = date;
                            labelStartDate.TextColor = Color.Black;
                        }
                        else if (strStart == "false")
                        {
                            isSelectEndDate = true;

                            endDate = calendarPickerResult.SelectedDate;
                            labelEndDate.Text = date;
                            labelEndDate.TextColor = Color.Black;

                            SelectEndTime = new TimeSpan(12, 00, 00);
                        }
                    }
                }, strStart == "false", startDate));

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        private async void OnAlarm()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            List<string> actions = Common.listAlarmTimeSTR;
            string action = await UserDialogs.Instance.ActionSheetAsync("미리 알림", "", "", null, actions.ToArray());

            if (string.IsNullOrEmpty(action))
                action = actions[0];

            AlarmDesc = action;
            alarmTimeIdx = actions.FindIndex(x => x == action);

            await Task.Delay(100);
            Common.IsClickActioning = false;
        }

        public void OnAppearing() {}
    }
}
