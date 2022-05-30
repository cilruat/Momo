using System;
using System.Threading;
using System.Windows.Forms;

namespace MomoPush
{
    public partial class MomoPush : Form
    {
        private Thread daemonThread;
        private ServerProcess httpServer;

        private void initHttp()
        {
            this.FormClosing += fromClosing;
        }

        private void httpServer_Request(object sender, HttpRequestEventArgs e)
        {
            if (e.Request.PostParameters.Count == 0)
                return;

            string id = e.Request.PostParameters["s_id"];
            string group_id = e.Request.PostParameters["group_id"];
            string title = e.Request.PostParameters["title"];
            string desc = e.Request.PostParameters["desc"];
            string start_date = e.Request.PostParameters["start_date"];
            string alarm_type = e.Request.PostParameters["alarm_type"];

            string[] splits = start_date.Split(' ');
            string[] split_day = splits[0].Split('-');
            string[] split_time = splits[1].Split(':');

            int year = 0, month = 0, day = 0, hour = 0, min = 0;
            int.TryParse(split_day[0], out year);
            int.TryParse(split_day[1], out month);
            int.TryParse(split_day[2], out day);
            int.TryParse(split_time[0], out hour);
            int.TryParse(split_time[1], out min);

            DateTime time = new DateTime(year, month, day, hour, min, 0);
            DateTime alarmTime = time;

            int alarm_type_int = 0;
            int.TryParse(alarm_type, out alarm_type_int);
            EAlarmTimeType eType = (EAlarmTimeType)alarm_type_int;
            switch (eType)
            {
                case EAlarmTimeType.Min10: alarmTime = time.AddMinutes(-10); break;
                case EAlarmTimeType.Min30: alarmTime = time.AddMinutes(-30); break;
                case EAlarmTimeType.Hour1: alarmTime = time.AddHours(-1); break;
                case EAlarmTimeType.Day1: alarmTime = time.AddDays(-1); break;
            }

            TimeSpan diff = time - DateTime.Now;
            if (diff.TotalMinutes < 0)
                return;

            ScheduleInfo info = new ScheduleInfo
            {
                Id = id,
                GroupId = group_id,
                Title = title,
                Desc = desc,
                EType = eType,
                Time = alarmTime
            };

            listTotal.Add(info);
            listTotal.Sort((x, y) => DateTime.Compare(x.Time, y.Time));

            refreshScheduleList();

            WriteLog(id, DateTime.Now, "Request notification!");
        }

        private void refreshScheduleList()
        {
            listSchedule.Items.Clear();
            for (int i = 0; i < listTotal.Count; i++)
            {
                string id = listTotal[i].Id;
                string group_id = listTotal[i].GroupId;
                string title = listTotal[i].Title;
                DateTime time = listTotal[i].Time;

                string[] row = { id, group_id, title, time.ToString() };
                ListViewItem item = new ListViewItem(row);
                listSchedule.Items.Add(item);
            }
        }

        private void fromClosing(object sender, FormClosingEventArgs e)
        {
            if (daemonThread == null)
                return;

            if (daemonThread.IsAlive)
                httpServer.Stop();

            daemonThread.Abort();
        }
    }
}
