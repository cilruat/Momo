using System.Collections.Generic;

using Android.Net;
using Android.Telephony;
using Android.Provider;
using Android.Database;

using Xamarin.Forms;
using Momo.Droid;
using Plugin.CurrentActivity;

[assembly:Dependency(typeof(DeviceInfoService))]
namespace Momo.Droid
{
    public class DeviceInfoService : IDeviceInfo
    {
        public string GetPhoneNumber()
        {
            TelephonyManager tMgr =
                (TelephonyManager)CrossCurrentActivity.Current.AppContext.GetSystemService(Android.Content.Context.TelephonyService);

            string number = tMgr.Line1Number;            
            number = number.Replace("-", "");
            number = number.Replace("+82", "0");

            return number;
        }
    }
}