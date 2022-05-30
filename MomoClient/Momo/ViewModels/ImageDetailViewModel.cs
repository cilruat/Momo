using System;
using System.Net;
using System.IO;
using System.Text;

using Xamarin.Forms;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class ImageDetailViewModel : BaseViewModel
    {
        public Command DownloadCommand { get; }

        public ImageDetailViewModel(string url)
        {
            ImageUrl = url;
            DownloadCommand = new Command(OnDownload);
        }

        private string imageUrl;

        public string ImageUrl
        {
            get => imageUrl;
            set => SetProperty(ref imageUrl, value);
        }

        private async void OnDownload()
        {
            string[] actions = { "저장하기"};
            string action = await UserDialogs.Instance.ActionSheetAsync("", "", "", null, actions[0]);
            if (action == actions[0])
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    UserDialogs.Instance.Toast(new ToastConfig("다운로드중..."));

                    try
                    {
                        WebClient webClient = new WebClient();
                        webClient.DownloadDataCompleted += OnDownloadCompleted;

                        Uri url = new Uri(imageUrl);
                        webClient.DownloadDataAsync(url);
                    }
                    catch
                    {
                        await UserDialogs.Instance.AlertAsync("다운로드에 실패했습니다", okText: "확인");
                        return;
                    }
                }
                else
                    await UserDialogs.Instance.AlertAsync("다운로드에 실패했습니다", okText: "확인");
            }
        }

        private async void OnDownloadCompleted(object s, DownloadDataCompletedEventArgs e)
        {
            try
            {
                var bytes = e.Result;

                DateTime time = DateTime.Now;
                string localFileName = "MomoImage_" +
                    time.Year.ToString() + "_" +
                    time.Month.ToString() + "_" +
                    time.Day.ToString() + "_" +
                    time.Hour.ToString() + "_" +
                    time.Minute.ToString() + "_" +
                    time.Second.ToString() + ".jpg";

                string documentsPath = DependencyService.Get<IPathInfo>().GetPath();
                string localPath = Path.Combine(documentsPath, localFileName);

                File.WriteAllBytes(localPath, bytes);

                UserDialogs.Instance.Toast(new ToastConfig("저장했습니다"));
            }
            catch
            {
                await UserDialogs.Instance.AlertAsync("다운로드에 실패했습니다", okText: "확인");
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
