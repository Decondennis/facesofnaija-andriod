using AndroidX.Fragment.App;
using Android.Widget;
using Android.Views;
using System;
using Facesofnaija.Helpers.Utils;

// TODO: Implement MediaPlayer-based reels video playback
// Original ExoPlayer implementation saved as ViewReelsVideoFragment.cs.bak

namespace Facesofnaija.Activities.ReelsVideo
{
    public class ViewReelsVideoFragment : Fragment
    {
        private static ViewReelsVideoFragment Instance;

        public TextView TxtLikeCount { get; set; }
        public TubePlayerView TubePlayerView { get; set; }
        public View LikeLayout { get; set; }
        public ImageView ImgLike { get; set; }

        public static ViewReelsVideoFragment GetInstance()
        {
            return Instance;
        }

        public void StopVideo()
        {
            try
            {
                // TODO: Implement video stop with MediaPlayer
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnCreate(Android.OS.Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                Instance = this;

                // TODO: Initialize reels video player
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override Android.Views.View OnCreateView(Android.Views.LayoutInflater inflater, Android.Views.ViewGroup container, Android.OS.Bundle savedInstanceState)
        {
            try
            {
                // TODO: Create reels video view
                var view = inflater.Inflate(Android.Resource.Layout.SimpleListItem1, container, false);
                TxtLikeCount = view?.FindViewById<TextView>(Android.Resource.Id.Text1);
                LikeLayout = view;
                ImgLike = view?.FindViewById<ImageView>(Android.Resource.Id.Icon);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }
    }
}
