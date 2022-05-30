using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using Momo.Droid;
using Xamarin.Forms;
using Xamarin.Essentials;

[assembly: Dependency(typeof(NotiServiceWrapper))]
namespace Momo.Droid
{
    public class NotiServiceWrapper : INotificationService
    {
        private readonly INotificationService notificationService;

        public NotiServiceWrapper() { notificationService = NotiService.Instance; }

        public string GetDeviceToken() { return notificationService.GetDeviceToken(); }
        public NotiInfo GetInfo() { return notificationService.GetInfo(); }
    }
}