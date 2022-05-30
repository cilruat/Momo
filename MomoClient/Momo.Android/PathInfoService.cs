using System.IO;
using Android.OS;
using Xamarin.Forms;
using Momo.Droid;

[assembly: Dependency(typeof(PathInfoService))]
namespace Momo.Droid
{
    public class PathInfoService : IPathInfo
    {
        public string GetPath()
        {
            string path = "";

            try
            {
                path = Environment.GetExternalStoragePublicDirectory(Environment.DirectoryPictures).AbsolutePath;
                path += "/Momo";

                DirectoryInfo dir = new DirectoryInfo(path);
                if (!dir.Exists)
                    dir.Create();
            }
            catch {}

            return path;
        }
    }
}