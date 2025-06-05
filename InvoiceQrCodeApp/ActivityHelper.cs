#if ANDROID
using Android.App;

namespace InvoiceQrCodeApp;

public static class ActivityHelper
{
    public static Activity GetCurrentActivity()
    {
        return Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity!;
    }
}
#endif
