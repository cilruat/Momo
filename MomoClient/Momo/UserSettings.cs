using System.Collections.Generic;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using Plugin.ContactService.Shared;
using Newtonsoft.Json;

namespace Momo
{
    public static class UserSettings
    {
        static ISettings AppSettings
        {
            get { return CrossSettings.Current; }
        }

        public static string UserPhone
        {
            get => AppSettings.GetValueOrDefault(nameof(UserPhone), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(UserPhone), value);
        }

        public static string UserName
        {
            get => AppSettings.GetValueOrDefault(nameof(UserName), string.Empty);
            set => AppSettings.AddOrUpdateValue(nameof(UserName), value);
        }

        private const string userContactsKey = "user_contacts_key";

        public static List<SimpleContact> UserContacts
        {
            get
            {
                string val = AppSettings.GetValueOrDefault(userContactsKey, string.Empty);
                List<SimpleContact> list;
                if (string.IsNullOrEmpty(val))
                    list = new List<SimpleContact>();
                else
                    list = JsonConvert.DeserializeObject<List<SimpleContact>>(val);
                return list;
            }
            set
            {
                string val = JsonConvert.SerializeObject(value);
                AppSettings.AddOrUpdateValue(userContactsKey, val);
            }
        }

        public static void ClearAllData()
        {
            AppSettings.Clear();
        }
    }
}
