using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Momo.Models;
using Momo.Views;

using Xamarin.Forms;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Acr.UserDialogs;

namespace Momo.ViewModels
{
    public class GroupImagesViewModel : BaseViewModel
    {
        private bool isEmptyList;
        private bool isImageList;

        public bool IsEmptyList
        {
            get => isEmptyList;
            set => SetProperty(ref isEmptyList, value);
        }

        public bool IsImageList
        {
            get => isImageList;
            set => SetProperty(ref isImageList, value);
        }

        public ObservableCollection<MediaFile> Images { get; }
        public Command LoadImagesCommand { get; }
        public Command OptionCommand { get; }
        public Command<MediaFile> ImageTapped { get; }

        private bool isReload = true;

        public GroupImagesViewModel()
        {
            IsEmptyList = false;
            IsImageList = false;

            Images = new ObservableCollection<MediaFile>();
            LoadImagesCommand = new Command(ExecuteLoadImagesCommand);
            OptionCommand = new Command(OnOption);
            ImageTapped = new Command<MediaFile>(OnImageTapped);
        }

        async void ExecuteLoadImagesCommand()
        {
            try
            {
                IsBusy = true;

                if (isReload == false)
                {
                    var images = await DataMediaFile.GetItemsAsync();
                    if (images != null && DataMediaFile.GetCount() > 0)
                    {
                        Images.Clear();
                        foreach (MediaFile m in images)
                        {
                            if (m.GroupId == Common.ViewGroupID)
                                Images.Add(m);
                        }
                    }
                    else
                    {
                        IsEmptyList = true;
                        IsImageList = false;
                    }

                    IsBusy = false;
                }
                else
                {
                    HttpClient client = new HttpClient();
                    Uri uri = new Uri(Common.UrlServerPHP + "GetImageList.php");

                    var param = new Dictionary<string, string> { { "group_id", Common.ViewGroupID } };
                    var content = new FormUrlEncodedContent(param);

                    Task<HttpResponseMessage> task_response = client.PostAsync(uri, content);
                    HttpResponseMessage response = task_response.Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Images.Clear();

                        Task<string> task_jsonResponse = response.Content.ReadAsStringAsync();
                        string jsonResponse = task_jsonResponse.Result;
                        if (jsonResponse.StartsWith("null"))
                        {
                            IsBusy = false;
                            IsEmptyList = true;
                            IsImageList = false;
                            return;
                        }

                        JArray jArray = JArray.Parse(jsonResponse);
                        foreach (JObject e in jArray)
                        {
                            Dictionary<string, string> dicRes = JsonConvert.DeserializeObject<Dictionary<string, string>>(e.ToString());

                            string path = dicRes["url"];
                            if (path.StartsWith(".."))
                                path = Common.UrlServer + dicRes["url"].Substring(3);

                            MediaFile image = new MediaFile
                            {
                                Id = dicRes["id"],
                                GroupId = dicRes["group_id"],
                                NoticeId = dicRes["notice_id"],
                                Path = path,
                                Type = (MediaFileType)Enum.Parse(typeof(MediaFileType), dicRes["type"])
                            };

                            _ = DataMediaFile.UpdateItemAsync(image);
                            Images.Add(image);
                        }

                        _ = DataMediaFile.SortItemAsync();

                        isReload = false;
                        Device.StartTimer(TimeSpan.FromSeconds(5), () =>
                        {
                            isReload = true;
                            return false;
                        });

                        IsEmptyList = Images.Count == 0;
                        IsImageList = Images.Count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                IsBusy = false;
                await Common.ErrorAlertWithMoveLogin(ex);
                return;
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void OnOption()
        {
            string[] actions = { "선택", "모든 사진 저장" };
            string action = await UserDialogs.Instance.ActionSheetAsync("", "", "", null, actions);
            if (action == actions[0])
            {
                await UserDialogs.Instance.AlertAsync("준비중인 기능입니다", okText: "확인");
            }
            else if (action == actions[1])
            {
                bool isAccept = await UserDialogs.Instance.ConfirmAsync("Wi-Fi에 연결되지 않은 경우 데이터 이용료가 발생할 수 있습니다.", "사진을 저장하시겠습니까?", "휴대폰에 바로 저장", "취소");
                if (isAccept)
                {
                    if (Device.RuntimePlatform == Device.Android)
                    {
                        UserDialogs.Instance.ShowLoading("다운로드중...");

                        try
                        {
                            WebClient webClient = new WebClient();

                            IEnumerator<MediaFile> list = Images.GetEnumerator();
                            while(list.MoveNext())
                            {
                                MediaFile m = list.Current;

                                Uri url = new Uri(m.Path);
                                byte[] bytes = await webClient.DownloadDataTaskAsync(url);

                                DateTime time = DateTime.Now;
                                string localFileName = "MomoImage_" +
                                    time.Year.ToString() + "_" +
                                    time.Month.ToString() + "_" +
                                    time.Day.ToString() + "_" +
                                    time.Hour.ToString() + "_" +
                                    time.Minute.ToString() + "_" +
                                    time.Second.ToString() + "_" +
                                    time.Millisecond.ToString() + ".jpg";

                                string documentsPath = DependencyService.Get<IPathInfo>().GetPath();
                                string localPath = Path.Combine(documentsPath, localFileName);

                                //File.WriteAllBytes(localPath, bytes);

                                using(FileStream stream = File.Open(localPath, FileMode.OpenOrCreate))
                                {
                                    stream.Seek(0, SeekOrigin.End);
                                    await stream.WriteAsync(bytes, 0, bytes.Length);
                                }
                            }

                            UserDialogs.Instance.HideLoading();
                            await UserDialogs.Instance.AlertAsync("모든 사진을 저장했습니다", okText: "확인");
                        }
                        catch
                        {
                            UserDialogs.Instance.HideLoading();
                            await UserDialogs.Instance.AlertAsync("사진을 다운로드 받던 중 실패했습니다", okText: "확인");
                            return;
                        }
                    }
                    else
                        await UserDialogs.Instance.AlertAsync("다운로드에 실패했습니다", okText: "확인");
                }
            }
        }

        private async void OnImageTapped(MediaFile image)
        {
            if (image == null)
                return;

            if (Common.IsClickActioning)
                return;

            Common.IsClickActioning = true;

            await Shell.Current.Navigation.PushModalAsync(new ImageDetailPage(image.Path));
            await Task.Delay(100);

            Common.IsClickActioning = false;
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}
