using Android.Views;
using Saplin.CPDT.UICore;
using Xamarin.Forms;

[assembly: Dependency(typeof(Saplin.CPDT.Droid.KeepScreenOn))]
namespace Saplin.CPDT.Droid
{
    public class KeepScreenOn : IKeepScreenOn
    {
        public void Disable()
        {
            var activity = MainActivity.Instance;
            var window = activity.Window;

            window.ClearFlags(WindowManagerFlags.KeepScreenOn);
            activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.User;
        }

        public void Enable()
        {
            var activity = MainActivity.Instance;
            var window = activity.Window;

            window.AddFlags(WindowManagerFlags.KeepScreenOn);
            activity.RequestedOrientation = Android.Content.PM.ScreenOrientation.Locked;
        }
    }
}