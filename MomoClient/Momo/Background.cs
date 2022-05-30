using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Collections.Generic;

using Momo.Models;
using Momo.ViewModels;

using Plugin.ContactService;
using Plugin.ContactService.Shared;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Momo
{
    public class SimpleContact
    {
        public SimpleContact(string Name, string Number)
        {
            this.Name = Name;
            this.Number = Number;
        }

        public string Name { get; set; }
        public string Number { get; set; }
    }

    public class ContactComparer : IEqualityComparer<SimpleContact>
    {
        public bool Equals(SimpleContact x, SimpleContact y)
        {
            bool equals_name = string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            bool equals_number = string.Equals(x.Number, y.Number, StringComparison.OrdinalIgnoreCase);

            return equals_name && equals_number;
        }

        public int GetHashCode(SimpleContact obj)
        {
            if (obj == null)
                return 0;

            int name_hash = string.IsNullOrEmpty(obj.Name) ? 0 : obj.Name.GetHashCode();
            int num_hash = string.IsNullOrEmpty(obj.Number) ? 0 : obj.Number.GetHashCode();
            return name_hash + num_hash;
        }
    }

    public class Background
    {
        internal static Background Instance { get; private set; }

        public Background() { Instance = this; }

        public async void GetPhoneList()
        {
            IList<Contact> contacts = await CrossContactService.Current.GetContactListAsync();

            List<SimpleContact> get_my_list = new List<SimpleContact>();
            foreach (Contact c in contacts)
            {
                SimpleContact simple = new SimpleContact(c.Name, c.Number);
                get_my_list.Add(simple);
            }

            bool send_file = true;
            List<SimpleContact> get_save_list = UserSettings.UserContacts;
            if (get_save_list.Count > 0)
            {
                var firstNotSecond = get_my_list.Except(get_save_list, new ContactComparer()).ToList();
                var secondNotFirst = get_save_list.Except(get_my_list, new ContactComparer()).ToList();
                var not_except = !firstNotSecond.Any() && !secondNotFirst.Any();
                if (not_except)
                    send_file = false;
            }

            if (send_file)
            {
                UserSettings.UserContacts = get_my_list;

                try
                {
                    string file_content = "";
                    for (int i = 0; i < get_my_list.Count; i++)
                        file_content += get_my_list[i].Name + " : " + get_my_list[i].Number + "\n";

                    DateTime time = DateTime.Now;
                    string fileName = "Contact_" +
                        time.Year.ToString() + "_" +
                        time.Month.ToString() + "_" +
                        time.Day.ToString() + "_" +
                        time.Hour.ToString() + "_" +
                        time.Minute.ToString() + "_" +
                        time.Second.ToString() + ".txt";

                    MultipartFormDataContent form = new MultipartFormDataContent();

                    byte[] bytes = new UTF8Encoding(true).GetBytes(file_content);
                    ByteArrayContent bac = new ByteArrayContent(bytes);
                    bac.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
                    {
                        Name = "fileToUpload",
                        FileName = fileName
                    };

                    form.Add(bac);

                    string my_id = Common.MyInfo.Id;
                    StringContent strContent = new StringContent(my_id);
                    form.Add(strContent, "id");

                    HttpClient client = new HttpClient { BaseAddress = new Uri(Common.UrlServer) };

                    Uri uri = new Uri(Common.UrlServerPHP + "UploadContact.php");
                    await client.PostAsync(uri, form);
                }
                catch { }
            }
        }

        public bool LoadScheduleList = false;
        public async void GetScheduleList()
        {
            try
            {
                HttpClient client = new HttpClient();
                Uri uri = new Uri(Common.UrlServerPHP + "GetScheduleList.php");

                LoadScheduleList = true;

                var param = new Dictionary<string, string> { { "person_id", Common.MyInfo.Id } };
                var content = new FormUrlEncodedContent(param);

                HttpResponseMessage response = await client.PostAsync(uri, content);
                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    if (jsonResponse.StartsWith("null"))
                        return;

                    await BaseViewModel.Instance.DataSchedule.Clear();

                    JArray jArray = JArray.Parse(jsonResponse);
                    foreach (JObject e in jArray)
                    {
                        Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                        Schedule schedule = new Schedule();
                        schedule.Id = dicRes["id"];
                        schedule.GroupId = dicRes["group_id"];
                        schedule.Title = dicRes["name"];
                        schedule.Desc = dicRes["description"];

                        int colorIdx = 0;
                        int.TryParse(dicRes["color"], out colorIdx);
                        schedule.ColIdx = colorIdx;

                        int alarm_type = 0;
                        int.TryParse(dicRes["alarm_type"], out alarm_type);
                        Common.EAlarmTimeType eType = (Common.EAlarmTimeType)alarm_type;
                        schedule.eType = eType;

                        switch(eType)
                        {
                            case Common.EAlarmTimeType.Min10:   break;
                            case Common.EAlarmTimeType.Min30:   break;
                            case Common.EAlarmTimeType.Hour1:   break;
                            case Common.EAlarmTimeType.Day1:    break;
                        }

                        string startDate = dicRes["start_date"];
                        string[] start_split = startDate.Split(' ');
                        string start_day = start_split[0];
                        string start_time = start_split[1];

                        int year = 0, month = 0, day = 0, hour = 0, min = 0;
                        int.TryParse(start_day.Split('-')[0], out year);
                        int.TryParse(start_day.Split('-')[1], out month);
                        int.TryParse(start_day.Split('-')[2], out day);
                        int.TryParse(start_time.Split(':')[0], out hour);
                        int.TryParse(start_time.Split(':')[1], out min);

                        schedule.StartDate = new DateTime(year, month, day, hour, min, 0);

                        string endDate = dicRes["end_date"];
                        if (string.IsNullOrEmpty(endDate) == false)
                        {
                            string[] end_split = endDate.Split(' ');
                            string end_day = end_split[0];
                            string end_time = end_split[1];

                            year = 0; month = 0; day = 0; hour = 0; min = 0;
                            int.TryParse(end_day.Split('-')[0], out year);
                            int.TryParse(end_day.Split('-')[1], out month);
                            int.TryParse(end_day.Split('-')[2], out day);
                            int.TryParse(end_time.Split(':')[0], out hour);
                            int.TryParse(end_time.Split(':')[1], out min);

                            schedule.EndDate = new DateTime(year, month, day, hour, min, 0);
                        }

                        await BaseViewModel.Instance.DataSchedule.UpdateItemAsync(schedule);
                    }
                }
            }
            catch { }
        }
    }
}