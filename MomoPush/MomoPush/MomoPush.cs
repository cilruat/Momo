using System;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace MomoPush
{
    public partial class MomoPush : Form
    {
        private bool enableSetTimer = false;
        private bool sendingNotify = false;
        private List<ScheduleInfo> listTotal = new List<ScheduleInfo>();

        public MomoPush()
        {
            InitializeComponent();
            initHttp();
            //TestLoad();
        }

        private void WriteLog(string log)
        {
            WriteLog("-", DateTime.Now, log);
        }

        private void WriteLog(string id, DateTime time, string log)
        {
            string[] row = { time.ToString(), log, id };
            ListViewItem item = new ListViewItem(row);
            listLog.Items.Add(item);
        }

        private void sendPush_Click(object sender, EventArgs e)
        {
            if (titleBox.Text.Trim() == "" || msgBox.Text.Trim() == "" || deviceIdBox.Text.Trim() == "")
            {
                var okCancel = MessageBox.Show("Please enter box", "Warning");
                if (okCancel == DialogResult.OK)
                {
                    if (titleBox.Text.Trim() == "") titleBox.Focus();
                    else if (msgBox.Text.Trim() == "") msgBox.Focus();
                    else if (deviceIdBox.Text.Trim() == "") deviceIdBox.Focus();

                    return;
                }
            }

            string[] splits = deviceIdBox.Text.Split(',');
            List<string> ids = new List<string>(splits);

            notifyAsync(null, ids);
        }

        private void loadSchedule_Click(object sender, EventArgs e)
        {
            loadSchedule();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            curTime.Text = DateTime.Now.ToString();

            if (enableSetTimer == false)
            {
                if (DateTime.Now.Second == 0)
                {
                    enableSetTimer = true;
                    SetTimer.Interval = 1000 * 2;
                    SetTimer.Start();
                }
            }
        }

        private void RemainTimer_Tick(object sender, EventArgs e)
        {
            if (sendingNotify)
            {
                remainTime.Text = "Push 보내는 중...";
                return;
            }

            if (listTotal.Count == 0)
                return;

            TimeSpan timeDiff = listTotal[0].Time - DateTime.Now;
            long ticks = timeDiff.Ticks;

            if (ticks < 0)
                return;

            DateTime remain = new DateTime(timeDiff.Ticks);

            if (timeDiff.Days > 0)
                remainTime.Text = remain.ToString("d일 h시간 m분 s초 남음");
            else
            {
                if (timeDiff.Hours > 0)
                    remainTime.Text = remain.ToString("h시간 m분 s초 남음");
                else if (timeDiff.Minutes > 0)
                    remainTime.Text = remain.ToString("m분 s초 남음");
                else if (timeDiff.Seconds > 0)
                    remainTime.Text = remain.ToString("s초 남음");
            }
        }

        private void SetTimer_Tick(object sender, EventArgs e)
        {
            if (listTotal.Count == 0)
                return;

            if (sendingNotify)
                return;

            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);
            DateTime first = listTotal[0].Time;

            if (DateTime.Compare(first, now) <= 0)
                loadGroupPersonAndNotifyAsync();
        }

        private void ServerStart_Click(object sender, EventArgs e)
        {
            if (ipBox.Text.Trim() == "")
            {
                var okCancel = MessageBox.Show("Please enter IP", "Warning");
                if (okCancel == DialogResult.OK)
                    ipBox.Focus();

                return;
            }

            if (portBox.Text.Trim() == "")
            {
                var okCancel = MessageBox.Show("Please enter Port", "Warning");
                if (okCancel == DialogResult.OK)
                    portBox.Focus();

                return;
            }

            int port = 0;
            if (int.TryParse(portBox.Text, out port) == false)
            {
                var okCancel = MessageBox.Show("Please number of Port", "Warning");
                if (okCancel == DialogResult.OK)
                    portBox.Focus();

                return;
            }

            httpServer = new ServerProcess(this, ipBox.Text, port);
            httpServer.Reqeust += httpServer_Request;

            daemonThread = new Thread(new ThreadStart(httpServer.listen));
            daemonThread.Start();

            loadSchedule();

            ipBox.Enabled = false;
            portBox.Enabled = false;
            btnStart.Enabled = false;

            WriteLog("서버가 구동되었습니다");
        }
    }
}
