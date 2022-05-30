using Momo.Droid;
using Xamarin.Forms;

[assembly: Dependency(typeof(CloseAppService))]
namespace Momo.Droid
{
    public class CloseAppService : ICloseAppService
    {
        public void CloseApplication()
        {
            MainActivity.Instance.OnFinishActivity();
        }
    }
}