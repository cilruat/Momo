using System;
using System.IO;
using Android.Graphics;
using Momo.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(ImageHelpers))]
namespace Momo.Droid
{
    public class ImageHelpers : ICompressImages
    {
        public static readonly string collectionName = "TmpPictures";

        public string SaveFile(byte[] imageByte, string fileName)
        {
            var fileDir = new Java.IO.File(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), collectionName);
            if (!fileDir.Exists())
                fileDir.Mkdirs();

            var file = new Java.IO.File(fileDir, fileName);
            File.WriteAllBytes(file.Path, imageByte);

            return file.Path;
        }

        public string CompressImage(string path)
        {
            byte[] imageBytes;

            var originalImage = BitmapFactory.DecodeFile(path);

            var imageSize = .86;
            var imageCompression = 67;

            var width = (originalImage.Width * imageSize);
            var height = (originalImage.Height * imageSize);
            var scaledImage = Bitmap.CreateScaledBitmap(originalImage, (int)width, (int)height, true);

            using (MemoryStream ms = new MemoryStream())
            {
                scaledImage.Compress(Bitmap.CompressFormat.Jpeg, imageCompression, ms);
                imageBytes = ms.ToArray();
            }

            originalImage.Recycle();
            originalImage.Dispose();
            GC.Collect();

            return SaveFile(imageBytes, Guid.NewGuid().ToString());
        }
    }
}