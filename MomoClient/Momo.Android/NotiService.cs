namespace Momo.Droid
{
    public class NotiService : INotificationService
    {
        public static readonly NotiService Instance = new NotiService();

        private string deviceToken;
        private NotiInfo notiInfo;

        public NotiService()
        {
            deviceToken = default(string);
            notiInfo.Empty();
        }

        public string GetDeviceToken() { return deviceToken; }
        public void SetDeviceToken(string token) { deviceToken = token; }

        public void SetInfo(NotiInfo info) { this.notiInfo = info.Copy(); }

        public NotiInfo GetInfo ()
        {
            NotiInfo temp = notiInfo.Copy();
            notiInfo.Empty();

            return temp;
        }
    }
}