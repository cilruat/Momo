using System.IO;
using Android.Content;
using Android.OS;
using Momo.Droid;
using Xamarin.Forms;
using Plugin.CurrentActivity;

[assembly: Dependency(typeof(MediaService))]
namespace Momo.Droid
{
    public class MediaService : Java.Lang.Object, IMediaService
    {
        public static int OPENGALLERYCODE = 100;
        public void OpenGallery()
        {
            try
            {
                var imageIntent = new Intent(Intent.ActionPick);
                imageIntent.SetType("image/*");
                imageIntent.PutExtra(Intent.ExtraAllowMultiple, true);
                imageIntent.SetAction(Intent.ActionGetContent);
                CrossCurrentActivity.Current.Activity.StartActivityForResult(Intent.CreateChooser(imageIntent, "Select photo"), OPENGALLERYCODE);
            }
            catch
            {
            }
        }

        /// <summary>
        ///     Call this when you want to delete our temporary images.
        ///     Recommendation: Call this after successfully uploading images to Azure Blob Storage.
        /// </summary>
		void IMediaService.ClearFileDirectory()
        {
            var directory = new Java.IO.File(Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), ImageHelpers.collectionName).Path.ToString();

            if (Directory.Exists(directory))
            {
                var list = Directory.GetFiles(directory, "*");
                if (list.Length > 0)
                {
                    for (int i = 0; i < list.Length; i++)
                        File.Delete(list[i]);
                }
            }
        }

        /*
        Example of how to call ClearFileDirectory():

            if (Device.RuntimePlatform == Device.Android)
            {
                DependencyService.Get<IMediaService>().ClearFileDirectory();
            }
            if (Device.RuntimePlatform == Device.iOS)
            {
                GMMultiImagePicker.Current.ClearFileDirectory();
            }

        */
    }
}