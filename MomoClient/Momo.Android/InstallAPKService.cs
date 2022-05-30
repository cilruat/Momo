using System;
using System.IO;
using System.Net;

using Android.Content;

using Momo.Droid;
using Xamarin.Forms;
using Xamarin.Essentials;

[assembly: Dependency(typeof(InstallAPKService))]
namespace Momo.Droid
{
    public class InstallAPKService : IInstallAPKService
    {
        public void InstallAPKAsync(Uri uri)
        {
            using (WebClient request = new WebClient())
            {
                string path = MainActivity.Instance.GetExternalFilesDir(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
                string pathToApk = path + "/" + AppInfo.PackageName + ".apk";

                byte[] bytes = request.DownloadData(uri);
                FileStream fs = new FileStream(pathToApk, FileMode.Create);
                fs.Seek(0, SeekOrigin.Begin);
                fs.Write(bytes, 0, bytes.Length);
                fs.Flush();
                fs.Close();

                Java.IO.File apkFile = new Java.IO.File(pathToApk);
                Intent webIntent = null;
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.N)
                {
                    Android.Net.Uri apkUri = FileProvider.GetUriForFile(MainActivity.Instance, "com.cccd.momo.provider", apkFile);
                    webIntent = new Intent(Intent.ActionInstallPackage);
                    webIntent.SetFlags(ActivityFlags.GrantReadUriPermission);
                    webIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                }
                else
                {
                    Android.Net.Uri apkUri = Android.Net.Uri.FromFile(apkFile);
                    webIntent = new Intent(Intent.ActionView);
                    webIntent.SetFlags(ActivityFlags.NewTask);
                    webIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                }

                MainActivity.Instance.OnStartActivity(webIntent);
            }
        }
    }
}