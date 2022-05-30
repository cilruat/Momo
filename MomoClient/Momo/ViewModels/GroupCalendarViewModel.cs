using System;
using System.Windows.Input;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;

using Momo.Views;
using Momo.Models;

using Xamarin.Forms;
using Xamarin.Plugin.Calendar.Models;

namespace Momo.ViewModels
{
    class GroupCalendarViewModel : BaseViewModel
    {
        static public bool RefreshCalendar = false;

        public class SimpleDateTime
        {
            public int year = 0;
            public int month = 0;
            public int day = 0;

            public DateTime date;

            public SimpleDateTime(DateTime dateTime)
            {
                year = dateTime.Year;
                month = dateTime.Month;
                day = dateTime.Day;

                date = new DateTime(year, month, day);
            }

            public override bool Equals(object obj)
            {
                if (obj == null || !(obj is SimpleDateTime))
                    return false;

                return ((SimpleDateTime)obj).year == this.year &&
                       ((SimpleDateTime)obj).month == this.month &&
                       ((SimpleDateTime)obj).day == this.day;
            }

            public override int GetHashCode()
            {
                return (this.year + this.month + this.day).GetHashCode();
            }
        }


        public Command AddScheduleCommand { get; }
        public ICommand EventSelectedCommand => new Command(async (item) => await ExecuteEventSelectedCommand(item));

        private DateTime _monthYear = DateTime.Today;
        public DateTime MontyYear
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

        private CultureInfo _culture = CultureInfo.InvariantCulture;
        public CultureInfo Culture
        {
            get => _culture;
            set => SetProperty(ref _culture, value);
        }

        public EventCollection Events { get; }        

        public GroupCalendarViewModel()
        {
            RefreshCalendar = true;

            Culture = CultureInfo.CreateSpecificCulture("ko-KR");
            AddScheduleCommand = new Command(OnAddSchedule);

            Events = new EventCollection();
        }

        private async void OnLoad()
        {
            Events.Clear();
            Dictionary<SimpleDateTime, List<Schedule>> dic = new Dictionary<SimpleDateTime, List<Schedule>>();

            var schedules = await DataSchedule.GetItemsAsync();
            if (schedules != null && DataSchedule.GetCount() > 0)
            {
                foreach (Schedule s in schedules)
                {
                    if (s.GroupId != Common.ViewGroupID)
                        continue;

                    Schedule schedule = new Schedule();
                    schedule.Id = s.Id;
                    schedule.Title = s.Title;
                    schedule.Desc = s.Desc;
                    schedule.ColIdx = s.ColIdx;
                    schedule.eType = s.eType;

                    DateTime startDate = schedule.StartDate = schedule.UITime = s.StartDate;
                    DateTime endDate = schedule.EndDate = s.EndDate;

                    SimpleDateTime dateTime_start = new SimpleDateTime(startDate);
                    SimpleDateTime dateTime_end = new SimpleDateTime(endDate);

                    if (endDate.Year == 1)
                    {
                        schedule.FormatTT = startDate.ToString("tt");
                        schedule.Time = startDate.ToString("h:mm");
                        schedule.Type1 = true;
                        schedule.TypeEtc1 = "시작";
                        
                        List<Schedule> listSchedule;
                        if (dic.TryGetValue(dateTime_start, out listSchedule))
                        {
                            listSchedule.Add(schedule);
                            dic[dateTime_start] = listSchedule;
                        }
                        else
                        {
                            listSchedule = new List<Schedule>() { schedule };
                            dic.Add(dateTime_start, listSchedule);
                        }

                        continue;
                    }

                    TimeSpan timeDiff = dateTime_end.date - dateTime_start.date;
                    if (timeDiff.Days == 0)
                    {
                        schedule.Type3 = true;
                        schedule.TypeEtc1 = startDate.ToString("tt h:mm");
                        schedule.TypeEtc2 = endDate.ToString("tt h:mm");

                        SimpleDateTime dateTime = new SimpleDateTime(startDate);
                        List<Schedule> listSchedule;
                        if (dic.TryGetValue(dateTime, out listSchedule))
                        {
                            listSchedule.Add(schedule);
                            dic[dateTime] = listSchedule;
                        }
                        else
                        {
                            listSchedule = new List<Schedule>() { schedule };
                            dic.Add(dateTime, listSchedule);
                        }
                    }
                    else
                    {
                        int diff_days = Math.Abs(timeDiff.Days);
                        for (int i = 0; i <= diff_days; i++)
                        {
                            schedule = new Schedule();
                            schedule.Id = s.Id;
                            schedule.Title = s.Title;
                            schedule.Desc = s.Desc;
                            schedule.ColIdx = s.ColIdx;
                            schedule.eType = s.eType;

                            startDate = schedule.StartDate = s.StartDate;
                            endDate = schedule.EndDate = s.EndDate;

                            if (i == 0)
                            {
                                schedule.FormatTT = startDate.ToString("tt");
                                schedule.Time = startDate.ToString("h:mm");
                                schedule.Type1 = true;
                                schedule.TypeEtc1 = "시작";
                                schedule.UITime = dateTime_start.date;
                            }
                            else if (i == timeDiff.Days)
                            {
                                schedule.FormatTT = endDate.ToString("tt");
                                schedule.Time = endDate.ToString("h:mm");
                                schedule.Type1 = true;
                                schedule.TypeEtc1 = "종료";
                                schedule.UITime = dateTime_end.date;
                            }
                            else
                                schedule.Type2 = true;

                            DateTime makeDate = startDate.AddDays(i);
                            SimpleDateTime dateTime = new SimpleDateTime(makeDate);
                            if (i != 0 && i != timeDiff.Days)
                                schedule.UITime = dateTime.date;

                            List<Schedule> listSchedule;
                            if (dic.TryGetValue(dateTime, out listSchedule))
                            {
                                listSchedule.Add(schedule);
                                dic[dateTime] = listSchedule;
                            }
                            else
                            {
                                listSchedule = new List<Schedule>() { schedule };
                                dic.Add(dateTime, listSchedule);
                            }
                        }
                    }
                }
            }

            foreach (KeyValuePair<SimpleDateTime, List<Schedule>> pair in dic)
            {
                List<Schedule> list = pair.Value;
                list.Sort(delegate (Schedule x, Schedule y)
                {
                    if (x.UITime.Hour < y.UITime.Hour) return -1;
                    if (x.UITime.Hour > y.UITime.Hour) return 1;
                    if (x.UITime.Minute < y.UITime.Minute) return -1;
                    if (x.UITime.Minute > y.UITime.Minute) return 1;
                    return 0;
                });

                Color col = Color.Orange;
                Schedule find = list.Find(x => x.ColIdx != 0);
                if (find != null)
                    col = Common.ScheduleColors[find.ColIdx];

                Events.Add(pair.Key.date, new DayEventCollection<Schedule>(list) { 
                    EventIndicatorColor = col,
                    EventIndicatorSelectedColor = Color.White
                }); ;
            }
        }

        private async void OnAddSchedule()
        {
            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new NewSchedulePage(_selectedDate));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        private async Task ExecuteEventSelectedCommand(object item)
        {
            if (item is Schedule schedule)
            {
                if (Common.IsClickActioning)
                    return;

                Common.IsClickActioning = true;

                await Shell.Current.Navigation.PushModalAsync(new ScheduleDetailPage(schedule.Id));
                await Task.Delay(100);

                Common.IsClickActioning = false;
            }
        }

        public void OnAppearing()
        {
            if (RefreshCalendar)
            {
                RefreshCalendar = false;
                OnLoad();
            }
        }
    }
}
