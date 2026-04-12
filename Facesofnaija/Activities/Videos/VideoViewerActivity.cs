using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using System;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Utils;

// TODO: Implement MediaPlayer-based video viewer
// Original ExoPlayer implementation saved as VideoViewerActivity.cs.bak

namespace Facesofnaija.Activities.Videos
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class VideoViewerActivity : BaseActivity
    {
        private static VideoViewerActivity Instance;
        public object MAdapter { get; set; } // TODO: Add proper adapter type when implementing video features

        public static VideoViewerActivity GetInstance()
        {
            return Instance;
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Instance = this;
                
                // TODO: Implement video viewer UI
                // SetContentView(Resource.Layout.VideoViewerLayout);
                
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
