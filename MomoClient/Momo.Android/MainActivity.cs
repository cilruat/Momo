using Android;
using Android.Net;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Database;
using Android.Gms.Common;
using Android.Gms.Tasks;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using Android.Provider;
using AndroidX.Work;

using Java.Util.Concurrent;

using Xamarin.Forms;

using Firebase.Iid;
using Acr.UserDialogs;

using Plugin.CurrentActivity;
using Plugin.GoogleClient;

using System.Collections.Generic;

namespace Momo.Droid
{
    [Activity(Label = "Momo", Icon = "@drawable/momo_logo_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, ScreenOrientation = ScreenOrientation.Portrait )]
    [IntentFilter(new[] { "MAIN" }, Categories = new[] { "android.intent.category.DEFAULT" })]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IOnSuccessListener
    {
        internal static MainActivity Instance { get; private set; }

        private const int REQUEST_PERMISSIONS = 1;
        private App mainApp;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            Instance = this;

            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            CrossCurrentActivity.Current.Init(this, savedInstanceState);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(true);
            UserDialogs.Init(this);
            GoogleClientManager.Initialize(this);
            Rg.Plugins.Popup.Popup.Init(this);
            
            IsPlayServicesAvailable();
            CreateNotificationChannel();

            Firebase.FirebaseApp.InitializeApp(this);

#if DEBUG
            string radioV = Build.RadioVersion;
            Common.simul_mode = string.IsNullOrEmpty(radioV);
#endif

            if (CheckAppPermissions())
                LoadApp();
        }


        private void LoadApp()
        {
            mainApp = new App();
            LoadApplication(mainApp);

            GetFCMToken();
        }

        private bool CheckAppPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return false;

#if DEBUG
            if (Common.simul_mode)
                return true;
#endif

            Permission writeExternalStorage = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteExternalStorage);
            Permission readExternalStorage = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage);
            Permission readPhoneNumber = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneNumbers);
            Permission writeContacts = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts);
            Permission readContacts = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts);            

            bool writeGranted = writeExternalStorage == (int)Permission.Granted;
            bool readGranted = readExternalStorage == (int)Permission.Granted;
            bool readPhoneGranted = readPhoneNumber == (int)Permission.Granted;
            bool writeContactsGranted = writeContacts == (int)Permission.Granted;
            bool readContactsGranted = readContacts == (int)Permission.Granted;

            if (!writeGranted || !readGranted || !readPhoneGranted || !writeContactsGranted || !readContactsGranted)
            {
                if (!writeGranted || !readGranted)
                {
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage) ||
                        ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage))
                    {
                        new AlertDialog.Builder(this)
                            .SetTitle("알림")
                            .SetMessage("이 앱을 실행하려면 외부 저장소 접근 권한이 필요합니다.")
                            .SetPositiveButton("확인", (senderAlert, args) => { RequestPermissions(writeGranted, readGranted, true, true, true); })
                            .Show();
                    }
                    else
                        RequestPermissions(writeGranted, readGranted, true, true, true);
                }
                else if (!readPhoneGranted)
                {
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadPhoneNumbers))
                    {
                        new AlertDialog.Builder(this)
                            .SetTitle("알림")
                            .SetMessage("이 앱을 실행하려면 전화 접근 권한이 필요합니다.")
                            .SetPositiveButton("확인", (senderAlert, args) => { RequestPermissions(true, true, readPhoneGranted, true, true); })
                            .Show();
                    }
                    else
                        RequestPermissions(true, true, readPhoneGranted, true, true);
                }
                else if (!writeContactsGranted || !readContactsGranted)
                {
                    if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteContacts) ||
                        ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadContacts))
                    {
                        new AlertDialog.Builder(this)
                            .SetTitle("알림")
                            .SetMessage("이 앱을 실행하려면 연락처 접근 권한이 필요합니다.")
                            .SetPositiveButton("확인", (senderAlert, args) => { RequestPermissions(true, true, true, writeContactsGranted, readContactsGranted); })
                            .Show();
                    }
                    else
                        RequestPermissions(true, true, true, writeContactsGranted, readContactsGranted);
                }

                return false;
            }

            return true;
        }

        private void RequestPermissions(bool writeGranted, bool readGranted, bool readPhoneGranted, bool writeContactsGranted, bool readContactsGranted)
        {
            List<string> listPermissions = new List<string>();

            if (!writeGranted) listPermissions.Add(Manifest.Permission.WriteExternalStorage);
            if (!readGranted) listPermissions.Add(Manifest.Permission.ReadExternalStorage);
            if (!readPhoneGranted) listPermissions.Add(Manifest.Permission.ReadPhoneNumbers);
            if (!writeContactsGranted) listPermissions.Add(Manifest.Permission.WriteContacts);
            if (!readContactsGranted) listPermissions.Add(Manifest.Permission.ReadContacts);

            var permissions = listPermissions.ToArray();
            if (permissions.Length > 0)
                ActivityCompat.RequestPermissions(this, permissions, REQUEST_PERMISSIONS);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == REQUEST_PERMISSIONS)
            {
                bool check_result = true;
                for(int i = 0; i < grantResults.Length; i++)
                {
                    if (grantResults[i] != Permission.Granted)
                    {
                        check_result = false;
                        break;
                    }
                }

                if (check_result)
                {
                    Permission readPhoneNumber = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneNumbers);
                    Permission writeContacts = ContextCompat.CheckSelfPermission(this, Manifest.Permission.WriteContacts);
                    Permission readContacts = ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadContacts);
                    bool readPhoneGrated = readPhoneNumber == (int)Permission.Granted;
                    bool writeContactsGranted = writeContacts == (int)Permission.Granted;
                    bool readContactsGranted = readContacts == (int)Permission.Granted;
                    if (!readPhoneGrated)
                        RequestPermissions(true, true, readPhoneGrated, true, true);
                    else if (!writeContactsGranted || !readContactsGranted)
                        RequestPermissions(true, true, true, writeContactsGranted, readContactsGranted);
                    else
                        LoadApp();
                }
                else
                {
                    string desc = "";
                    bool grantedWriteExternal = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteExternalStorage);
                    bool grantedReadExternal = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadExternalStorage);
                    bool grantedPhoneNum = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadPhoneNumbers);
                    bool grantedWriteContacts = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.WriteContacts);
                    bool grantedReadContacts = ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.ReadContacts);
                    if (grantedWriteExternal || grantedReadExternal || grantedPhoneNum || grantedWriteContacts || grantedReadContacts)
                        desc = "권한이 거부되었습니다. 앱을 다시 실행하여 권한을 허용해주세요.";
                    else
                        desc = "권한이 거부되었습니다. 설정(앱 정보)에서 권한을 허용해야 합니다.";

                    new AlertDialog.Builder(this)
                        .SetTitle("알림")
                        .SetMessage(desc)
                        .SetPositiveButton("확인", (senderAlert, args) => { OnFinishActivity(); })
                        .Show();
                }
            }
        }

        public void OnStartActivity(Intent intent)
        {
            StartActivity(intent);
            OnFinishActivity();
        }

        public void OnFinishActivity()
        {
            this.FinishAffinity();
        }

        protected override void OnResume()
        {
            base.OnResume();
            IsPlayServicesAvailable();

            if (FindNoticeInfo(Intent))
                RequestNotification();
        }

        public override void OnTrimMemory([GeneratedEnum] TrimMemory level)
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced);
            base.OnTrimMemory(level);
        }

        public override void OnLowMemory()
        {
            FFImageLoading.ImageService.Instance.InvalidateMemoryCache();
            System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced);
            base.OnLowMemory();
        }

        public static int OPENGALLERYCODE = 100;
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            GoogleClientManager.OnAuthCompleted(requestCode, resultCode, data);

            //If we are calling multiple image selection, enter into here and return photos and their filepaths.
            if (requestCode == OPENGALLERYCODE && resultCode == Result.Ok)
            {
                List<string> images = new List<string>();

                if (data != null)
                {
                    //Separate all photos and get the path from them all individually.
                    ClipData clipData = data.ClipData;
                    if (clipData != null)
                    {
                        for (int i = 0; i < clipData.ItemCount; i++)
                        {
                            ClipData.Item item = clipData.GetItemAt(i);
                            Uri uri = item.Uri;
                            var path = GetRealPathFromURI(uri);

                            if (path != null)
                                images.Add(path);
                        }
                    }
                    else
                    {
                        Uri uri = data.Data;
                        var path = GetRealPathFromURI(uri);

                        if (path != null)
                            images.Add(path);
                    }

                    MessagingCenter.Send<App, List<string>>((App)Xamarin.Forms.Application.Current, "ImagesSelectedAndroid", images);
                }
            }
        }

        internal static readonly string CHANNEL_ID = "my_notification_channel";
        internal static readonly int NOTIFICATION_ID = 100;

        public bool IsPlayServicesAvailable()
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("알림");

            int resultCode = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this);
            if (resultCode != ConnectionResult.Success)
            {
                GoogleApiAvailability.Instance.MakeGooglePlayServicesAvailable(this);
                return false;
            }

            return true;
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
                return;

            var channel = new NotificationChannel(CHANNEL_ID, "FCM Notifications", NotificationImportance.Default)
            {
                Description = "Firebase Cloud Messages appear in this channel"
            };

            var notificationManager = (NotificationManager)GetSystemService(Android.Content.Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }

        public void GetFCMToken()
        {
            FirebaseInstanceId.Instance.GetInstanceId().AddOnSuccessListener(this);
        }

        public void OnSuccess(Java.Lang.Object result)
        {
            var token = result.Class.GetMethod("getToken").Invoke(result).ToString();
            NotiService.Instance.SetDeviceToken(token);
            RefreshDeviceToken();
        }

        public static void RefreshDeviceToken()
        {
            Instance?.mainApp?.RefreshToken();
        }

        public static void RequestNotification()
        {
            Instance?.mainApp?.RequestNotification();
        }

        private bool FindNoticeInfo(Intent intent)
        {
            bool ret = false;

            Bundle bundle = intent.Extras;
            if (bundle != null)
            {
                object value = bundle.Get("key");
                if (value != null)
                {
                    NotiInfo info = new NotiInfo { action = intent.Action };

                    Dictionary<string, string> dicData = new Dictionary<string, string>();
                    SetBundleData("key", bundle, ref dicData);

                    switch (value.ToString())
                    {
                        case "chat":
                            // chat room info
                            SetBundleData("room_id", bundle, ref dicData);
                            SetBundleData("room_name", bundle, ref dicData);
                            SetBundleData("person_ids", bundle, ref dicData);
                            SetBundleData("group_id", bundle, ref dicData);

                            // chat info
                            SetBundleData("chat_id", bundle, ref dicData);
                            SetBundleData("person_id", bundle, ref dicData);
                            SetBundleData("msg", bundle, ref dicData);
                            SetBundleData("time", bundle, ref dicData);
                            break;

                        case "notice":
                        case "comment":
                            SetBundleData("notice_id", bundle, ref dicData);
                            break;
                        case "schedule":
                            SetBundleData("group_id", bundle, ref dicData);
                            break;
                    }

                    info.dicData = dicData;
                    NotiService.Instance.SetInfo(info);
                    ret = true;
                }

                intent.SetAction("");
                intent.RemoveExtra("key");
            }

            return ret;
        }

        private void SetBundleData(string key, Bundle bundle, ref Dictionary<string, string> dicData)
        {
            if (dicData == null)
                return;

            object value = bundle.Get(key);
            if (value != null)
                dicData.Add(key, value.ToString());
        }

        public string GetRealPathFromURI(Uri contentURI)
        {
            try
            {
                ICursor imageCursor = null;
                string fullPathToImage = "";

                imageCursor = ContentResolver.Query(contentURI, null, null, null, null);
                imageCursor.MoveToFirst();
                int idx = imageCursor.GetColumnIndex(MediaStore.Images.ImageColumns.Data);

                if (idx != -1)
                    fullPathToImage = imageCursor.GetString(idx);
                else
                {
                    ICursor cursor = null;
                    var docID = DocumentsContract.GetDocumentId(contentURI);
                    var id = docID.Split(':')[1];
                    var whereSelect = MediaStore.Images.ImageColumns.Id + "=?";
                    var projections = new string[] { MediaStore.Images.ImageColumns.Data };

                    cursor = ContentResolver.Query(MediaStore.Images.Media.InternalContentUri, projections, whereSelect, new string[] { id }, null);
                    if (cursor.Count == 0)
                    {
                        cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri, projections, whereSelect, new string[] { id }, null);
                    }
                    var colData = cursor.GetColumnIndexOrThrow(MediaStore.Images.ImageColumns.Data);
                    cursor.MoveToFirst();
                    fullPathToImage = cursor.GetString(colData);
                }
                return fullPathToImage;
            }
            catch
            {
            }

            return null;
        }
    }
}