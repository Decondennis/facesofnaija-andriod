using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using System;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Helpers.ShimmerUtils;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Helpers.Model;

// TODO: Implement MediaPlayer-based video playback in RecyclerView
// Original ExoPlayer implementation saved as WRecyclerView.cs.bak

namespace Facesofnaija.Activities.NativePost.Extra
{
    public class WRecyclerView : RecyclerView
    {
        private static WRecyclerView Instance;
        public enum VolumeState { On, Off }

        public FrameLayout MediaContainerLayout;
        public ImageView PlayControl;

        private int VideoSurfaceDefaultHeight;
        private int ScreenDefaultHeight;
        public Context MainContext;
        public bool IsVideoViewAdded;
        public string VideoUrl;
        public string Hash;
        public RecyclerScrollListener MainScrollEvent;
        public NativePostAdapter NativeFeedAdapter;
        public SwipeRefreshLayout SwipeRefreshLayoutView;
        public FloatingActionButton PopupBubbleView;
        public TemplateShimmerInflater ShimmerInflater;

        public static string Filter { set; get; }
        public static string PostType { set; get; }

        public ApiPostAsync ApiPostAsync;
        public ExoMediaController ExoMediaController { get; set; }

        private VideoView ActiveVideoView;
        private AdapterHolders.PostVideoSectionViewHolder ActiveVideoHolder;
        private string ActiveVideoUrl;
        public string LastPlayedVideoUrl { get; private set; }
        public int LastPlayedPositionMs { get; private set; }
        public bool HasActiveVideo => ActiveVideoView != null;

        protected WRecyclerView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public WRecyclerView(Context context) : base(context)
        {
            Init(context);
        }

        public WRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init(context);
        }

        public WRecyclerView(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
            Init(context);
        }

        private void Init(Context context)
        {
            try
            {
                MainContext = context;
                Instance = this;
                HasFixedSize = true;

                var display = MainContext?.GetSystemService(Context.WindowService)?.JavaCast<IWindowManager>()?.DefaultDisplay;
                Point point = new Point();
                display?.GetSize(point);

                VideoSurfaceDefaultHeight = point.X;
                ScreenDefaultHeight = point.Y;

                // Initialize media controller for video playback
                ExoMediaController = new ExoMediaController(context);

                // TODO: Initialize MediaPlayer components
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static WRecyclerView GetInstance()
        {
            return Instance;
        }

        // In-feed video playback (single active player to avoid multiple videos playing while scrolling)
        public void PlayVideo(bool isEndOfList = false, string url = null, object holder = null)
        {
            try
            {
                if (holder is AdapterHolders.PostVideoSectionViewHolder vh)
                {
                    PlayVideo(vh, isEndOfList);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(url))
                {
                    ActiveVideoUrl = url;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayVideo(object holder, bool isEndOfList)
        {
            try
            {
                if (holder is not AdapterHolders.PostVideoSectionViewHolder videoHolder)
                    return;

                var videoUrl = videoHolder.VideoUrl;
                if (string.IsNullOrWhiteSpace(videoUrl) || videoHolder.MediaContainer == null)
                {
                    return;
                }

                // Toggle play/pause when tapping the currently active item.
                if (ActiveVideoHolder == videoHolder && ActiveVideoView != null)
                {
                    if (ActiveVideoView.IsPlaying)
                    {
                        ActiveVideoView.Pause();
                        if (videoHolder.PlayButton != null)
                            videoHolder.PlayButton.Visibility = ViewStates.Visible;
                    }
                    else
                    {
                        ActiveVideoView.Start();
                        if (videoHolder.PlayButton != null)
                            videoHolder.PlayButton.Visibility = ViewStates.Gone;
                    }

                    return;
                }

                // User tapped a different video — clear auto-resume so old video doesn't restart
                if (LastPlayedVideoUrl != videoUrl)
                {
                    LastPlayedVideoUrl = null;
                    LastPlayedPositionMs = 0;
                }

                RemoveVideo();

                // Fill the fixed-height (250dp) media_container completely
                var videoView = new VideoView(videoHolder.ItemView.Context)
                {
                    LayoutParameters = new FrameLayout.LayoutParams(
                        ViewGroup.LayoutParams.MatchParent,
                        ViewGroup.LayoutParams.MatchParent,
                        GravityFlags.Center)
                };
                videoView.Clickable = true;

                ActiveVideoView = videoView;
                ActiveVideoHolder = videoHolder;
                ActiveVideoUrl = videoUrl;

                if (videoHolder.VideoProgressBar != null)
                    videoHolder.VideoProgressBar.Visibility = ViewStates.Visible;
                if (videoHolder.PlayButton != null)
                    videoHolder.PlayButton.Visibility = ViewStates.Gone;
                // Keep VideoImage visible on top as backdrop while buffering
                if (videoHolder.VideoImage != null)
                    videoHolder.VideoImage.Visibility = ViewStates.Visible;

                // Tap the video surface to pause/resume
                var capturedVideoHolder = videoHolder;
                videoView.Click += (s, e) =>
                {
                    WRecyclerView.GetInstance()?.PlayVideo(capturedVideoHolder, false);
                };

                // Add VideoView at index 0 (behind thumbnail ImageView) so it has a Surface to decode on.
                // The thumbnail stays visible on top until OnPrepared hides it — no black flash.
                videoHolder.MediaContainer.AddView(videoView, 0);

                var preparedListener = new MediaPreparedListener(videoHolder, videoView, LastPlayedVideoUrl == videoUrl ? LastPlayedPositionMs : 0);
                var completionListener = new MediaCompletionListener(videoHolder);
                var errorListener = new MediaErrorListener(videoHolder);
                
                videoView.SetOnPreparedListener(preparedListener);
                videoView.SetOnCompletionListener(completionListener);
                videoView.SetOnErrorListener(errorListener);
                
                try
                {
                    var uri = Android.Net.Uri.Parse(videoUrl);
                    videoView.SetVideoURI(uri);
                }
                catch (Exception)
                {
                    videoView.SetVideoPath(videoUrl);
                }
                
                videoView.Start();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                RemoveVideo();
            }
        }

        public void RemoveVideo()
        {
            try
            {
                if (ActiveVideoView != null)
                {
                    if (!string.IsNullOrWhiteSpace(ActiveVideoUrl))
                    {
                        try
                        {
                            LastPlayedPositionMs = Math.Max(ActiveVideoView.CurrentPosition, 0);
                        }
                        catch
                        {
                            LastPlayedPositionMs = 0;
                        }
                    }

                    try
                    {
                        ActiveVideoView.StopPlayback();
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    if (ActiveVideoView.Parent is ViewGroup parent)
                        parent.RemoveView(ActiveVideoView);
                }

                if (ActiveVideoHolder != null)
                {
                    if (ActiveVideoHolder.VideoProgressBar != null)
                        ActiveVideoHolder.VideoProgressBar.Visibility = ViewStates.Gone;

                    if (ActiveVideoHolder.PlayButton != null)
                        ActiveVideoHolder.PlayButton.Visibility = ViewStates.Visible;

                    if (ActiveVideoHolder.VideoImage != null)
                        ActiveVideoHolder.VideoImage.Visibility = ViewStates.Visible;
                }

                ActiveVideoView = null;
                ActiveVideoHolder = null;
                ActiveVideoUrl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveVideoForHolder(AdapterHolders.PostVideoSectionViewHolder holder)
        {
            try
            {
                if (holder == null)
                    return;

                if (ReferenceEquals(holder, ActiveVideoHolder))
                {
                    // Scrolled away — remember the URL so we can auto-resume when it comes back
                    LastPlayedVideoUrl = ActiveVideoUrl;

                    try
                    {
                        LastPlayedPositionMs = ActiveVideoView?.CurrentPosition ?? 0;
                    }
                    catch
                    {
                        LastPlayedPositionMs = 0;
                    }

                    RemoveVideo();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ReleasePlayer()
        {
            RemoveVideo();
        }

        public void StopVideo()
        {
            LastPlayedVideoUrl = null;
            LastPlayedPositionMs = 0;
            RemoveVideo();
        }

        public void RestartPlayAfterShrinkScreen()
        {
            try
            {
                if (ActiveVideoHolder != null && !string.IsNullOrWhiteSpace(ActiveVideoUrl))
                    PlayVideo(ActiveVideoHolder, false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public bool ShouldAutoResume(string videoUrl)
        {
            var result = !string.IsNullOrWhiteSpace(videoUrl) && videoUrl == LastPlayedVideoUrl;
            return result;
        }

        public void SetXAdapter(NativePostAdapter adapter, SwipeRefreshLayout swipeRefresh)
        {
            NativeFeedAdapter = adapter;
            SwipeRefreshLayoutView = swipeRefresh;
            SetAdapter(adapter);
        }

        public void SetXTemplateShimmer(TemplateShimmerInflater inflater)
        {
            ShimmerInflater = inflater;
        }

        public void RemoveByRowIndex(object item)
        {
            try
            {
                if (NativeFeedAdapter?.ListDiffer == null || item == null)
                    return;

                var removed = false;
                if (item is AdapterModelsClass model)
                {
                    var index = NativeFeedAdapter.ListDiffer.IndexOf(model);
                    if (index >= 0)
                    {
                        NativeFeedAdapter.ListDiffer.RemoveAt(index);
                        NativeFeedAdapter.NotifyItemRemoved(index);
                        removed = true;
                    }
                }

                if (!removed)
                {
                    for (var i = 0; i < NativeFeedAdapter.ListDiffer.Count; i++)
                    {
                        if (ReferenceEquals(NativeFeedAdapter.ListDiffer[i], item))
                        {
                            NativeFeedAdapter.ListDiffer.RemoveAt(i);
                            NativeFeedAdapter.NotifyItemRemoved(i);
                            removed = true;
                            break;
                        }
                    }
                }

                if (!removed)
                    NativeFeedAdapter.NotifyDataSetChanged();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static string GetFilter() 
        {
            return Filter ?? "";
        }

        public static string GetPostType()
        {
            return PostType ?? "";
        }

        public void SetPostAndFilterType(string postType, string filter)
        {
            try
            {
                PostType = postType;
                Filter = filter;
                // TODO: Refresh adapter with new filter
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetPostAndFilterType(int postType, string filter)
        {
            try
            {
                var mappedType = postType switch
                {
                    1 => "text",
                    2 => "photos",
                    3 => "video",
                    4 => "music",
                    5 => "files",
                    6 => "maps",
                    _ => ""
                };

                SetPostAndFilterType(mappedType, filter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetXPopupBubble(FloatingActionButton popupBubble)
        {
            try
            {
                PopupBubbleView = popupBubble;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                // Load more posts when scrolling to bottom
                ApiPostAsync?.FetchNewsFeedApiPosts();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private class MediaPreparedListener : Java.Lang.Object, MediaPlayer.IOnPreparedListener
        {
            private readonly AdapterHolders.PostVideoSectionViewHolder Holder;
            private readonly VideoView VideoView;
            private readonly int ResumePositionMs;

            public MediaPreparedListener(AdapterHolders.PostVideoSectionViewHolder holder, VideoView videoView, int resumePositionMs)
            {
                Holder = holder;
                VideoView = videoView;
                ResumePositionMs = Math.Max(resumePositionMs, 0);
            }

            public void OnPrepared(MediaPlayer mp)
            {
                try
                {
                    if (mp != null)
                    {
                        mp.Looping = true;
                        mp.SetVideoScalingMode(VideoScalingMode.ScaleToFitWithCropping);
                    }

                    if (ResumePositionMs > 0)
                        VideoView.SeekTo(ResumePositionMs);

                    // Hide spinner and thumbnail — first frame rendered, video surface is visible underneath
                    if (Holder.VideoProgressBar != null)
                        Holder.VideoProgressBar.Visibility = ViewStates.Gone;

                    if (Holder.VideoImage != null)
                        Holder.VideoImage.Visibility = ViewStates.Gone;

                    if (Holder.PlayButton != null)
                        Holder.PlayButton.Visibility = ViewStates.Gone;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MediaCompletionListener : Java.Lang.Object, MediaPlayer.IOnCompletionListener
        {
            private readonly AdapterHolders.PostVideoSectionViewHolder Holder;

            public MediaCompletionListener(AdapterHolders.PostVideoSectionViewHolder holder)
            {
                Holder = holder;
            }

            public void OnCompletion(MediaPlayer mp)
            {
                try
                {
                    Console.WriteLine($"DEBUG MEDIA COMPLETION: video finished");
                    if (Holder.VideoImage != null)
                        Holder.VideoImage.Visibility = ViewStates.Visible;

                    if (Holder.PlayButton != null)
                        Holder.PlayButton.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        private class MediaErrorListener : Java.Lang.Object, MediaPlayer.IOnErrorListener
        {
            private readonly AdapterHolders.PostVideoSectionViewHolder Holder;

            public MediaErrorListener(AdapterHolders.PostVideoSectionViewHolder holder)
            {
                Holder = holder;
            }

            public bool OnError(MediaPlayer mp, [GeneratedEnum] MediaError what, int extra)
            {
                try
                {
                    Console.WriteLine($"DEBUG MEDIA ERROR: what={what}, extra={extra}");
                    if (Holder.VideoProgressBar != null)
                        Holder.VideoProgressBar.Visibility = ViewStates.Gone;

                    if (Holder.VideoImage != null)
                        Holder.VideoImage.Visibility = ViewStates.Visible;

                    if (Holder.PlayButton != null)
                        Holder.PlayButton.Visibility = ViewStates.Visible;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }

                return true;
            }
        }
    }
}
