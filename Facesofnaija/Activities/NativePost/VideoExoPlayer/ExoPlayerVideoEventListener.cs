using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Audio;
using Com.Google.Android.Exoplayer2.Metadata;
using Com.Google.Android.Exoplayer2.Text;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.Video;
using System;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.ExoVideoPlayer;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Equalizer;
using static Facesofnaija.Activities.NativePost.Extra.WRecyclerView;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace Facesofnaija.Activities.NativePost.VideoExoPlayer
{
    public interface IVideoPlayerEventListener
    {
        void OnPrePlay(IExoPlayer exoGlobalController);
        void OnPlayCanceled();
        void OnPlay();
        Uri GetVideoUrl();
        AdapterHolders.PostVideoSectionViewHolder GetViewHolderOfListener();
    }

    public class NewsFeedExoPlayerVideoEventListener : Object, IPlayer.IListener
    {
        private ImageButton VideoPlayButton;
        private WRecyclerView XRecyclerView;
        private ExoMediaControl ExoMediaController;
        private AdapterHolders.PostVideoSectionViewHolder MainHolder;

        private ImageView VolumeControl;
        private ImageView ExpandControl;
        private ProgressBar BufferProgressControl;
        private LinearLayout PositionLinearLayout;
        private EqualizerView Equalizer;

        public NewsFeedExoPlayerVideoEventListener(WRecyclerView recyclerView)
        {
            try
            {
                XRecyclerView = recyclerView;
                // Player = XRecyclerView.IExoVideoPlayer;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public NewsFeedExoPlayerVideoEventListener(ExoMediaControl exoMediaControl)
        {
            try
            {
                ExoMediaController = exoMediaControl;
                // Player = XRecyclerView.IExoVideoPlayer;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetFeedHolderToListenOn(AdapterHolders.PostVideoSectionViewHolder holder)
        {
            try
            {
                MainHolder = holder;
                VolumeControl = MainHolder.StyledPlayerControlViews.FindViewById<ImageView>(Resource.Id.exo_volume_icon);
                ExpandControl = MainHolder.StyledPlayerControlViews.FindViewById<ImageView>(Resource.Id.exo_expand_icon);
                BufferProgressControl = MainHolder.StyledPlayerControlViews.FindViewById<ProgressBar>(Resource.Id.exo_buffering);
                PositionLinearLayout = MainHolder.StyledPlayerControlViews.FindViewById<LinearLayout>(Resource.Id.positionLinear);
                Equalizer = MainHolder.StyledPlayerControlViews.FindViewById<EqualizerView>(Resource.Id.equalizer_view);

                MainHolder.StyledPlayerControlViews.ProgressUpdate += holder.ControlView_ProgressUpdate;

                switch (XRecyclerView.ExoMediaController.VolumeStateProvider)
                {
                    case VolumeState.Off:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                        break;
                    case VolumeState.On:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                        break;
                    default:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                        break;
                }

                if (!VolumeControl.HasOnClickListeners)
                    VolumeControl.Click += (sender, args) => { ToggleVolume(); };

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }


        private void ToggleVolume()
        {
            try
            {
                if (XRecyclerView.ExoMediaController.Exoplayer == null)
                    return;

                switch (XRecyclerView.ExoMediaController.VolumeStateProvider)
                {
                    case VolumeState.Off:
                        XRecyclerView.ExoMediaController.Exoplayer.Volume = 1f;
                        break;
                    case VolumeState.On:
                        XRecyclerView.ExoMediaController.Exoplayer.Volume = 0f;
                        break;
                    default:
                        XRecyclerView.ExoMediaController.Exoplayer.Volume = 0f;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }



        public void OnPlayerErrorChanged(PlaybackException exception)
        {

        }

        public void OnPlaybackSuppressionReasonChanged(int playbackSuppressionReason)
        {

        }

        public void OnPlayerError(PlaybackException exception)
        {

        }

        public void OnVolumeChanged(float volume)
        {

            try
            {
                if (XRecyclerView.ExoMediaController.Exoplayer == null)
                    return;

                VolumeControl?.BringToFront();

                if (XRecyclerView.ExoMediaController.Exoplayer.Volume != volume)
                    XRecyclerView.ExoMediaController.Exoplayer.Volume = volume;

                if (volume == 0f)
                    XRecyclerView.ExoMediaController.VolumeStateProvider = VolumeState.Off;
                else
                    XRecyclerView.ExoMediaController.VolumeStateProvider = VolumeState.On;


                switch (XRecyclerView.ExoMediaController.VolumeStateProvider)
                {
                    case VolumeState.Off:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                        break;
                    case VolumeState.On:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_up_grey_24dp);
                        break;
                    default:
                        VolumeControl.SetImageResource(Resource.Drawable.ic_volume_off_grey_24dp);
                        break;
                }

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnIsPlayingChanged(bool isPlaying)
        {
            try
            {
                if (isPlaying)
                    MainHolder.OnPlay();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }

        }

        public void OnLoadingChanged(bool isLoading)
        {
        }

        public void OnMaxSeekToPreviousPositionChanged(long maxSeekToPreviousPositionMs)
        {

        }

        public void OnRepeatModeChanged(int repeatMode)
        {
        }

        public void OnSeekBackIncrementChanged(long seekBackIncrementMs)
        {

        }

        public void OnSeekForwardIncrementChanged(long seekForwardIncrementMs)
        {

        }

        public void OnSeekProcessed()
        {
        }


        public void OnPlayerStateChanged(bool playWhenReady, int playbackState)
        {
            try
            {
                //if (VideoResumeButton == null || VideoPlayButton == null || LoadingProgressBar == null)
                //    return;


                switch (playbackState)
                {

                    case IPlayer.StateEnded:
                        {
                            switch (playWhenReady)
                            {
                                case false:
                                    // VideoResumeButton.Visibility = ViewStates.Visible;
                                    break;
                                default:
                                    // VideoResumeButton.Visibility = ViewStates.Gone;
                                    VideoPlayButton.Visibility = ViewStates.Visible;
                                    break;
                            }

                            BufferProgressControl.Visibility = ViewStates.Visible;


                            TabbedMainActivity.GetInstance()?.SetOffWakeLock();
                            break;
                        }
                    case IPlayer.StateReady:
                        {
                            switch (playWhenReady)
                            {
                                case false:
                                    //VideoResumeButton.Visibility = ViewStates.Gone;
                                    VideoPlayButton.Visibility = ViewStates.Visible;
                                    break;
                                default:
                                    BufferProgressControl.Visibility = ViewStates.Gone;
                                    Equalizer.Visibility = ViewStates.Visible;
                                    Equalizer.AnimateBars();

                                    break;
                            }


                            TabbedMainActivity.GetInstance()?.SetOnWakeLock();
                            break;
                        }
                    case IPlayer.StateBuffering:
                        BufferProgressControl.Visibility = ViewStates.Visible;
                        Equalizer.Visibility = ViewStates.Gone;
                        Equalizer.StopBars();
                        //VideoPlayButton.Visibility = ViewStates.Invisible;
                        //LoadingProgressBar.Visibility = ViewStates.Visible;
                        //VideoResumeButton.Visibility = ViewStates.Invisible;
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnPlaylistMetadataChanged(MediaMetadata mediaMetadata)
        {

        }

        public void OnShuffleModeEnabledChanged(bool p0)
        {
        }

        public void OnSkipSilenceEnabledChanged(bool skipSilenceEnabled)
        {

        }

        public void OnPlaybackParametersChanged(PlaybackParameters playbackParameters)
        {
        }

        public void OnPositionDiscontinuity(int reason)
        {
        }

        public void OnTrackSelectionParametersChanged(TrackSelectionParameters parameters)
        {

        }

        public void OnTracksChanged(Tracks tracks)
        {
        }

        public void OnTimelineChanged(Timeline timeline, int time)
        {
        }

        public void OnMediaItemTransition(MediaItem MediaItem, int reason)
        {
        }

        public void OnPlaybackStateChanged(int state)
        {
        }

        public void OnPlayWhenReadyChanged(bool playWhenReady, int reason)
        {
        }

        public void OnEvents(IPlayer Player, IPlayer.Events Events)
        {

        }

        public void OnIsLoadingChanged(bool changed)
        {
        }

        public void OnMediaMetadataChanged(MediaMetadata mediaMetadata)
        {
        }

        public void OnMetadata(Metadata metadata)
        {

        }

        public void OnDeviceInfoChanged(DeviceInfo deviceInfo)
        {

        }

        public void OnDeviceVolumeChanged(int Volume, bool Muted)
        {
        }

        public void OnAudioAttributesChanged(AudioAttributes audioAttributes)
        {

        }

        public void OnAudioSessionIdChanged(int audioSessionId)
        {

        }

        public void OnAvailableCommandsChanged(IPlayer.Commands availableCommands)
        {
        }

        public void OnCues(CueGroup cueGroup)
        {

        }

        public void OnSurfaceSizeChanged(int width, int hight)
        {

        }

        public void OnVideoSizeChanged(VideoSize size)
        {

        }

        public void OnRenderedFirstFrame()
        {

        }
    }
}