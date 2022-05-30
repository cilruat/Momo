using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Xamarin.Forms;

using Momo.Views;
using Momo.Models;
using Momo.Services;

using Acr.UserDialogs;

namespace Momo
{
    public class Common
    {
        public enum EMultiSelectType
        {
            All,
            NewNotice,
            NewChatRoom,
            AddPerson,
        }

        public enum EAlarmTimeType
        {
            None = 0,
            Min10,
            Min30,
            Hour1,
            Day1
        }

        static public List<string> listAlarmTimeSTR = new List<string>() { 
            "없음",
            "10분 전",
            "30분 전",
            "1시간 전",
            "1일 전" 
        };

        static public bool OP = true;
        static public bool simul_mode = false;
        //static public string TEST_PHONE_NUM = "01010020578";
        static public string TEST_PHONE_NUM = "01077932852";
        //static public string TEST_PHONE_NUM = "01011111111";

        static public string MARKET_DOWN_LINK = "market://details?id=com.cccd.momo";
        static public string HTTPS_DOWN_LINK = "https://play.google.com/store/apps/details?id=com.cccd.momo";

        static public int CurVersion = 144;
        static public string UrlServer = "http://165.229.169.123/";

#if DEBUG
        static public string UrlVersion = UrlServer + "version/version_dev.txt";
#else
        static public string UrlVersion = UrlServer + "version/version.txt";
#endif

#if DEBUG
        static public string UrlServerPHP =  UrlServer + "php_dev/";
#else
        static public string UrlServerPHP = UrlServer + "php/";
#endif

        static public Person MyInfo = null;

        static public string ViewGroupID = "";
        static public string ViewGroupName = "";

        static public bool FirstCheckClickNoti = false;
        static public bool IsClickActioning = false;

        static public EMultiSelectType eMSType = EMultiSelectType.All;

        static public Color[] ScheduleColors = {
            Color.Orange,
            Color.Red,
            Color.YellowGreen,
            Color.Green,
            Color.DeepSkyBlue,
            Color.Violet,
            Color.Gray
        };

        static public string GetVersionSTR()
        {
            return (CurVersion / 100).ToString() + "." +
                (CurVersion % 100 / 10).ToString() +
                (CurVersion % 10).ToString();
        }

        static public async Task ErrorAlertWithCloseApp(Exception ex)
        {
            if (OP)
            {
                string errorMsg = "";
                while (ex != null)
                {
                    errorMsg += ex.Message + "\n";
                    ex = ex.InnerException;
                }

                if (string.IsNullOrEmpty(errorMsg) == false)
                    await UserDialogs.Instance.AlertAsync(errorMsg, "알림", "확인");
            }

            string desc = "예기치못한 문제로 인해 앱을 종료합니다";
            await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");

            DependencyService.Get<ICloseAppService>().CloseApplication();
        }

        static public async Task ErrorAlertWithMoveLogin()
        {
            string desc = "예기치못한 문제로 인해 로그인 화면으로 이동합니다";
            await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");

            await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
        }

        static public async Task ErrorAlertWithMoveLogin(Exception ex)
        {
            if (OP)
            {
                string errorMsg = "";
                while (ex != null)
                {
                    errorMsg += ex.Message + "\n";
                    ex = ex.InnerException;
                }

                if (string.IsNullOrEmpty(errorMsg) == false)
                    await UserDialogs.Instance.AlertAsync(errorMsg, "알림", "확인");
            }

            string desc = "예기치못한 문제로 인해 로그인 화면으로 이동합니다";
            await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");

            await Shell.Current.GoToAsync($"//{nameof(LoginCheckPage)}");
        }

        static public async Task ErrorAlertWithPopModal()
        {
            string desc = "예기치못한 문제가 발생했습니다";
            await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");

            if (Shell.Current.Navigation.ModalStack.Count > 0)
                await Shell.Current.Navigation.PopModalAsync(true);
        }

        static public async Task ErrorAlertWithPopModal(Exception ex)
        {
            if (OP)
            {
                string errorMsg = "";
                while (ex != null)
                {
                    errorMsg += ex.Message + "\n";
                    ex = ex.InnerException;
                }

                if (string.IsNullOrEmpty(errorMsg) == false)
                    await UserDialogs.Instance.AlertAsync(errorMsg, "알림", "확인");
            }

            string desc = "예기치못한 문제가 발생했습니다";
            await UserDialogs.Instance.AlertAsync(desc, "알림", "확인");            

            if (Shell.Current.Navigation.ModalStack.Count > 0)
                await Shell.Current.Navigation.PopModalAsync(true);
        }

        static public IEnumerable<SelectableItemPerson> SearchText(string search, HashSet<SelectableItemPerson> hash)
        {
            char[] chr = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
            string[] str = { "가", "까", "나", "다", "따", "라", "마", "바", "빠", "사", "싸", "아", "자", "짜", "차", "카", "타", "파", "하" };
            int[] chrint = { 44032, 44620, 45208, 45796, 46384, 46972, 47560, 48148, 48736, 49324, 49912, 50500, 51088, 51676, 52264, 52852, 53440, 54028, 54616, 55204 };

            string x = search;
            string pattern = "";

            for (int i = 0; i < x.Length; i++)
            {
                // 초성만 입력되었을때
                if (x[i] >= 'ㄱ' && x[i] <= 'ㅎ')
                {
                    for (int j = 0; j < chr.Length; j++)
                    {
                        if (x[i] == chr[j])
                            pattern += string.Format("[{0}-{1}]", str[j], (char)(chrint[j + 1] - 1));
                    }
                }
                // 완성된 문자를 입력했을때
                else if (x[i] >= '가')
                {
                    // 받침이 있는지 검사
                    int magic = ((x[i] - '가') % 588);

                    // 받침이 없을때
                    if (magic == 0)
                        pattern += string.Format("[{0}-{1}]", x[i], (char)(x[i] + 27));
                    // 받침이 있을때
                    else
                    {
                        magic = 27 - (magic % 28);
                        pattern += string.Format("[{0}-{1}]", x[i], (char)(x[i] + magic));
                    }
                }
                // 영어를 입력했을때
                else if (x[i] >= 'A' && x[i] <= 'z')
                    pattern += x[i];
                // 숫자를 입력했을때
                else if (x[i] >= '0' && x[i] <= '9')
                    pattern += x[i];
            }

            var res = hash.Where((SelectableItemPerson arg) => System.Text.RegularExpressions.Regex.IsMatch(arg.Person.PersonName.ToString(), pattern));
            return res;
        }
    }

    public interface IInstallAPKService {
        void InstallAPKAsync(Uri uri);
    }

    public interface ICloseAppService
    {
        void CloseApplication();
    }

    public interface ISaveContactService
    {
        void SaveContact(string name, string number);
        void SaveContact(List<KeyValuePair<string, string>> contacts);
    }

    public struct NotiInfo
    {
        public string action;
        public Dictionary<string, string> dicData;

        public NotiInfo Copy()
        {
            NotiInfo temp;
            temp.action = action;
            temp.dicData = dicData;

            return temp;
        }

        public void Empty()
        {
            action = default;
            dicData = new Dictionary<string, string>();
        }
    }

    public interface INotificationService
    {
        string GetDeviceToken();
        NotiInfo GetInfo();
    }

    public interface ICompressImages
    {
        string CompressImage(string path);
    }

    public interface IMediaService
    {
        void OpenGallery();
        void ClearFileDirectory();
    }

    public interface IMultiMediaPickerService
    {
        event EventHandler<MediaFile> OnMediaPicked;
        event EventHandler<IList<MediaFile>> OnMediaPickedCompleted;

        Task<IList<MediaFile>> PickPhotosAsync();
        Task<IList<MediaFile>> PickVideosAsync();

        void Clean();
    }

    public interface IDeviceInfo { string GetPhoneNumber(); }

    public interface IPathInfo { string GetPath(); }

    public interface IMemoryService
    {
        MemoryInfo GetInfo();
    }

    public class MemoryInfo
    {
        public long FreeMemory { get; set; }
        public long MaxMemory { get; set; }
        public long TotalMemory { get; set; }

        public long UsedMemory { get { return (TotalMemory - FreeMemory); } }

        public double HeapUsage() { return (double)(UsedMemory) / (double)TotalMemory; }

        public double Usage() { return (double)(UsedMemory) / (double)MaxMemory; }
    }
}
