using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using System;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Utils;

// TODO: Implement MediaPlayer-based fullscreen video playback
// Original ExoPlayer implementation saved as VideoFullScreenActivity.cs.bak

namespace Facesofnaija.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class VideoFullScreenActivity : BaseActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                // TODO: Implement fullscreen video player UI
                // SetContentView(Resource.Layout.VideoFullScreenLayout);

                // Show temporary message
                Android.Widget.Toast.MakeText(this, "Video playback temporarily unavailable during .NET 9 migration", Android.Widget.ToastLength.Long)?.Show();
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BackPressed()
        {
            try
            {
                Finish();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
