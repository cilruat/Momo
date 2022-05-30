using Android.App;
using Android.Content;
using Firebase.Messaging;

using System.Collections.Generic;

namespace Momo.Droid
{
    [Service]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class FirebaseIIDService : FirebaseMessagingService
    {
        public override void OnNewToken(string token)
        {
            NotiService.Instance.SetDeviceToken(token);
            MainActivity.RefreshDeviceToken();
        }

        public override void OnMessageReceived(RemoteMessage message)
        {
            string clickAction = "";
            RemoteMessage.Notification noti = message.GetNotification();
            if (noti != null)
                clickAction = message.GetNotification().ClickAction;

            NotiInfo info = new NotiInfo { action = clickAction };

            Dictionary<string, string> dictData = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> pair in message.Data)
                dictData.Add(pair.Key, pair.Value);

            info.dicData = dictData;

            NotiService.Instance.SetInfo(info);
            MainActivity.RequestNotification();
        }
    }
}