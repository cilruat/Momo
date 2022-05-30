using Momo.Models;
using Momo.ViewModels;
using Momo.Views;

using Xamarin.Forms;

namespace Momo
{
    public partial class App
    {
        public static string TOKEN = "";
        public static NotiInfo NotiInfo;

        public bool IsInitToken { get; private set; }
        public bool IsInitNoti { get; private set; }

        public void RefreshToken()
        {
            if (IsInitToken == false)
                return;

            string token = DependencyService.Get<INotificationService>().GetDeviceToken();
            TOKEN = token;
        }

        public void RequestNotification()
        {
            if (IsInitNoti == false)
                return;

            NotiInfo info = DependencyService.Get<INotificationService>().GetInfo();
            if (info.dicData == null && info.dicData.Count == 0)
                return;

            NotiInfo = info;

            if (info.dicData["key"] == "chat")
            {
                if (Shell.Current.Navigation.ModalStack.Count == 0)
                    return;

                int idx = Shell.Current.Navigation.ModalStack.Count - 1;
                Page CurrentPage = Shell.Current.Navigation.ModalStack[idx];
                if (CurrentPage == null)
                    return;

                System.Type type = CurrentPage.GetType();
                if (type.Name == "ChatDetailPage")
                {
                    string room_id = info.dicData["room_id"];
                    Chat chat = new Chat
                    {
                        Id = info.dicData["chat_id"],
                        RoomId = room_id,
                        PersonId = info.dicData["person_id"],
                        Msg = info.dicData["msg"],
                        Time = info.dicData["time"]
                    };
                    
                    ((ChatDetailPage)CurrentPage).AddChat(room_id, chat);

                    NotiInfo.Empty();
                }
            }
        }
    }
}
