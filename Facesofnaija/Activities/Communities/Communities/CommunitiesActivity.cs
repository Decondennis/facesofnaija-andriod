using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Facesofnaija.Activities.Base;

namespace Facesofnaija.Activities.Communities.Communities
{
    // DEPRECATED: Use CommunitiesDashboardActivity instead
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CommunitiesActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                var intent = new Intent(this, typeof(CommunitiesDashboardActivity));
                StartActivity(intent);
                Finish();
            }
            catch
            {
                Finish();
            }
        }

        public static CommunitiesActivity GetInstance() => null;
    }
}
