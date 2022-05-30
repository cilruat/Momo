using System;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Data.Common;

using Newtonsoft.Json;
using MySql.Data.MySqlClient;

namespace MomoPush
{
    public enum EAlarmTimeType
    {
        None = 0,
        Min10,
        Min30,
        Hour1,
        Day1
    }

    internal class ScheduleInfo
    {
        public string Id { get; set; }
        public string GroupId { get; set; }
        public string Title { get; set; }
        public string Desc { get; set; }
        public EAlarmTimeType EType { get; set; }
        public DateTime Time { get; set; }
    }

    internal class MessageData
    {
        [JsonProperty(PropertyName = "registration_ids", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> RegistrationIds { get; set; }
        [JsonProperty(PropertyName = "notification", NullValueHandling = NullValueHandling.Ignore)]
        public Notification Notification { get; set; }
        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Data { get; set; }

        public MessageData()
        {
            RegistrationIds = new List<string>();
            Data = new Dictionary<string, string> { { "key", "" } };
        }
    }

    internal class Notification
    {
        [JsonProperty(PropertyName = "body", NullValueHandling = NullValueHandling.Ignore)]
        public string Body { get; set; }
        [JsonProperty(PropertyName = "title", NullValueHandling = NullValueHandling.Ignore)]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "click_action", NullValueHandling = NullValueHandling.Ignore)]
        public string ClickAction { get; set; }

        public Notification() { ClickAction = "MAIN"; }
    }

    public partial class MomoPush : Form
    {
        private void TestLoad()
        {
            DateTime now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            ScheduleInfo info = new ScheduleInfo
            {
                Id = "1",
                GroupId = "25",
                Title = "1분 테스트",
                Desc = "1분 테스트 성공",
                Time = now.AddMinutes(1)
            };

            listTotal.Add(info);

            string[] row = { info.Id, info.GroupId, info.Title, info.Time.ToString() };
            ListViewItem item = new ListViewItem(row);
            listSchedule.Items.Add(item);

            info = new ScheduleInfo
            {
                Id = "1",
                GroupId = "25",
                Title = "1분 테스트 추가",
                Desc = "1분 테스트 추가 성공",
                Time = now.AddMinutes(1)
            };

            listTotal.Add(info);

            string[] row2 = { info.Id, info.GroupId, info.Title, info.Time.ToString() };
            item = new ListViewItem(row2);
            listSchedule.Items.Add(item);
        }

        private async void loadSchedule()
        {
            listTotal.Clear();
            listSchedule.Items.Clear();

            using (MySqlConnection connect = new MySqlConnection("Server=localhost;Port=3306;Database=momo_db;Uid=out_user;Pwd=Aden20170523!"))
            {
                try
                {
                    connect.Open();
                    string sql = "select id,group_id,name,description,start_date,alarm_type from momo_db.schedule where alarm_type != '0' and `delete`='0' order by start_date asc";

                    MySqlCommand cmd = new MySqlCommand(sql, connect);
                    DbDataReader table = await cmd.ExecuteReaderAsync();

                    while (await table.ReadAsync())
                    {
                        string id = table.GetString(0);
                        string group_id = table.GetString(1);
                        string title = table.GetString(2);
                        string desc = table.GetString(3);

                        string start_date = table.GetString(4);
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

                        short alarm_type = table.GetInt16(5);
                        EAlarmTimeType eType = (EAlarmTimeType)alarm_type;
                        switch (eType)
                        {
                            case EAlarmTimeType.Min10: alarmTime = time.AddMinutes(-10); break;
                            case EAlarmTimeType.Min30: alarmTime = time.AddMinutes(-30); break;
                            case EAlarmTimeType.Hour1: alarmTime = time.AddHours(-1); break;
                            case EAlarmTimeType.Day1: alarmTime = time.AddDays(-1); break;
                        }

                        TimeSpan diff = alarmTime - DateTime.Now;
                        if (diff.TotalMinutes < 0)
                            continue;

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
                    }

                    listTotal.Sort((x, y) => DateTime.Compare(x.Time, y.Time));
                    refreshScheduleList();

                    table.Close();
                }
                catch (Exception ex)
                {
                    string log = $"Exception thrown in loadSchedule(): {ex}";
                    WriteLog(log);
                }
                finally
                {
                    connect.Close();
                }
            }
        }

        private async void loadGroupPersonAndNotifyAsync()
        {
            sendingNotify = true;
            ScheduleInfo info = listTotal[0];

            using (MySqlConnection connect = new MySqlConnection("Server=localhost;Port=3306;Database=momo_db;Uid=out_user;Pwd=Aden20170523!"))
            {
                try
                {
                    await connect.OpenAsync();
                    string sql = $"select token from momo_db.person where p_id in (select person_id from momo_db.group_join where group_id='{info.GroupId}')";

                    MySqlCommand cmd = new MySqlCommand(sql, connect);
                    DbDataReader table = await cmd.ExecuteReaderAsync();

                    List<string> listToken = new List<string>();
                    while (table.Read())
                    {
                        string token = table.GetString(0);
                        listToken.Add(token);
                    }

                    notifyAsync(info, listToken);

                    if (listTotal.Count > 0)
                        listTotal.RemoveAt(0);

                    if (listSchedule.Items.Count > 0)
                        listSchedule.Items.RemoveAt(0);

                    if (listTotal.Count == 0)
                        remainTime.Text = "-";
                }
                catch (Exception ex)
                {
                    string log = $"Exception thrown in loadGroupPersonAndNotifyAsync(): {ex}";
                    WriteLog(log);
                }
                finally
                {
                    connect.Close();
                    sendingNotify = false;
                }
            }
        }

        private async void notifyAsync(ScheduleInfo info, List<string> deviceIds)
        {
            try
            {
                string api_access_key = "AAAAAWCXkU4:APA91bH74lcg8twbbpu8i8OJ6K1pL9tK449WzUk5XJD4g8j0yDtGj2CAG1ypVKXZDSk0yoOqDJxDaQdPsymsDWrZ7J_5UHI_lxvXyk8RRaf__gzPE0whCzCsWpkd7HELPuHDFyLMe8Kj";
                var serverKey = string.Format("key={0}", api_access_key);

                string title = info != null ? info.Title : titleBox.Text;

                string str_type = "";
                switch(info.EType)
                {
                    case EAlarmTimeType.Min10:  str_type = "10분";   break;
                    case EAlarmTimeType.Min30:  str_type = "30분";   break;
                    case EAlarmTimeType.Hour1:  str_type = "1시간";   break;
                    case EAlarmTimeType.Day1:   str_type = "1일";   break;
                }

                string push_msg = "일정 " + str_type + " 전 " + "'" + info.Title + "'";
                string msg = info != null ? push_msg : msgBox.Text;

                MessageData data = createMessage(title, msg, deviceIds);
                var jsonBody = JsonConvert.SerializeObject(data);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://fcm.googleapis.com/fcm/send"))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var result = await httpClient.SendAsync(httpRequest);
                        string log = result.IsSuccessStatusCode ? "Success notification" : $"Error notification. Code: {result.StatusCode}";
                        WriteLog(info.Id, info.Time, log);
                    }
                }
            }
            catch (Exception ex)
            {
                string log = $"Exception thrown in notifyAsync(): {ex}";
                WriteLog(log);
            }
        }

        private MessageData createMessage(string title, string msg, List<string> listIds)
        {
            return new MessageData()
            {
                RegistrationIds = listIds,
                Notification = new Notification()
                {
                    Body = msg,
                    Title = title,
                }
            };
        }
    }
}
