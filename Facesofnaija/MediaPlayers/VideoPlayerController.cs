using Android.App;
using Android.Content;
using Android.Media;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using Facesofnaija.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace Facesofnaija.MediaPlayers
{
    /// <summary>
    /// MediaPlayer-based video controller for .NET 9
    /// Replaces ExoPlayer functionality with native Android MediaPlayer
    /// </summary>
    public class VideoPlayerController : IDisposable
    {
        private readonly Context Context;
        private MediaPlayer MediaPlayer;
        private SurfaceView SurfaceView;
        private ProgressBar LoadingProgressBar;
        private bool IsInitialized;
        private bool IsDisposed;

        public event EventHandler<VideoStateChangedEventArgs> StateChanged;
        public event EventHandler<VideoErrorEventArgs> Error;
        public event EventHandler VideoCompleted;
        public event EventHandler<int> ProgressChanged;

        public bool IsPlaying => MediaPlayer?.IsPlaying ?? false;
        public int CurrentPosition => MediaPlayer?.CurrentPosition ?? 0;
        public int Duration => MediaPlayer?.Duration ?? 0;
        public string CurrentVideoUrl { get; private set; }

        public VideoPlayerController(Context context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Initialize the video player with a surface view
        /// </summary>
        public void InitializePlayer(SurfaceView surfaceView, ProgressBar progressBar = null)
        {
            try
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(nameof(VideoPlayerController));
                }

                SurfaceView = surfaceView;
                LoadingProgressBar = progressBar;

                if (MediaPlayer == null)
                {
                    MediaPlayer = new MediaPlayer();
                    SetupMediaPlayerEvents();
                }

                if (surfaceView != null)
                {
                    var holder = surfaceView.Holder;
                    holder.SetType(SurfaceType.PushBuffers);
                    MediaPlayer.SetDisplay(holder);
                }

                IsInitialized = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new VideoErrorEventArgs(VideoErrorType.Initialization, e.Message));
            }
        }

        /// <summary>
        /// Load and prepare video from URL
        /// </summary>
        public async Task<bool> LoadVideoAsync(string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                {
                    OnError(new VideoErrorEventArgs(VideoErrorType.InvalidUrl, "Video URL is null or empty"));
                    return false;
                }

                if (!IsInitialized)
                {
                    OnError(new VideoErrorEventArgs(VideoErrorType.NotInitialized, "Player not initialized"));
                    return false;
                }

                ShowLoading(true);
                CurrentVideoUrl = videoUrl;

                await Task.Run(() =>
                {
                    MediaPlayer?.Reset();
                    MediaPlayer?.SetDataSource(Context, Uri.Parse(videoUrl));
                    MediaPlayer?.SetAudioAttributes(new AudioAttributes.Builder()
                        .SetUsage(AudioUsageKind.Media)
                        .SetContentType(AudioContentType.Movie)
                        .Build());
                    MediaPlayer?.PrepareAsync();
                });

                OnStateChanged(VideoState.Loading);
                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new VideoErrorEventArgs(VideoErrorType.LoadFailed, e.Message));
                ShowLoading(false);
                return false;
            }
        }

        /// <summary>
        /// Play the current video
        /// </summary>
        public void Play()
        {
            try
            {
                if (MediaPlayer != null && !MediaPlayer.IsPlaying)
                {
                    MediaPlayer.Start();
                    OnStateChanged(VideoState.Playing);
                    StartProgressUpdates();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new VideoErrorEventArgs(VideoErrorType.PlaybackFailed, e.Message));
            }
        }

        /// <summary>
        /// Pause the current video
        /// </summary>
        public void Pause()
        {
            try
            {
                if (MediaPlayer?.IsPlaying == true)
                {
                    MediaPlayer.Pause();
                    OnStateChanged(VideoState.Paused);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Stop the current video
        /// </summary>
        public void Stop()
        {
            try
            {
                if (MediaPlayer != null)
                {
                    MediaPlayer.Stop();
                    MediaPlayer.Reset();
                    OnStateChanged(VideoState.Stopped);
                    CurrentVideoUrl = null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Seek to a specific position in milliseconds
        /// </summary>
        public void SeekTo(int positionMs)
        {
            try
            {
                MediaPlayer?.SeekTo(positionMs);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Set volume (0.0f to 1.0f)
        /// </summary>
        public void SetVolume(float volume)
        {
            try
            {
                var clampedVolume = Math.Max(0f, Math.Min(1f, volume));
                MediaPlayer?.SetVolume(clampedVolume, clampedVolume);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Mute or unmute the video
        /// </summary>
        public void SetMuted(bool muted)
        {
            SetVolume(muted ? 0f : 1f);
        }

        private void SetupMediaPlayerEvents()
        {
            try
            {
                if (MediaPlayer == null) return;

                MediaPlayer.Prepared += OnMediaPlayerPrepared;
                MediaPlayer.Completion += OnMediaPlayerCompleted;
                MediaPlayer.Error += OnMediaPlayerError;
                MediaPlayer.BufferingUpdate += OnMediaPlayerBuffering;
                MediaPlayer.Info += OnMediaPlayerInfo;
                MediaPlayer.VideoSizeChanged += OnMediaPlayerVideoSizeChanged;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnMediaPlayerPrepared(object sender, EventArgs e)
        {
            try
            {
                ShowLoading(false);
                OnStateChanged(VideoState.Ready);
                
                // Auto-play after preparation (optional)
                Play();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnMediaPlayerCompleted(object sender, EventArgs e)
        {
            try
            {
                OnStateChanged(VideoState.Completed);
                VideoCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnMediaPlayerError(object sender, MediaPlayer.ErrorEventArgs e)
        {
            try
            {
                ShowLoading(false);
                var errorMsg = $"MediaPlayer Error: {e.What}, Extra: {e.Extra}";
                OnError(new VideoErrorEventArgs(VideoErrorType.PlaybackFailed, errorMsg));
                OnStateChanged(VideoState.Error);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnMediaPlayerBuffering(object sender, MediaPlayer.BufferingUpdateEventArgs e)
        {
            try
            {
                // e.Percent contains buffering percentage (0-100)
                if (e.Percent < 100)
                {
                    OnStateChanged(VideoState.Buffering);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnMediaPlayerInfo(object sender, MediaPlayer.InfoEventArgs e)
        {
            try
            {
                switch (e.What)
                {
                    case MediaInfo.BufferingStart:
                        ShowLoading(true);
                        OnStateChanged(VideoState.Buffering);
                        break;
                    case MediaInfo.BufferingEnd:
                        ShowLoading(false);
                        OnStateChanged(VideoState.Playing);
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnMediaPlayerVideoSizeChanged(object sender, MediaPlayer.VideoSizeChangedEventArgs e)
        {
            try
            {
                // Adjust surface view size if needed
                if (SurfaceView != null && e.Width > 0 && e.Height > 0)
                {
                    var layoutParams = SurfaceView.LayoutParameters;
                    var screenWidth = SurfaceView.Width;
                    var screenHeight = SurfaceView.Height;

                    // Calculate aspect ratio
                    float videoAspect = (float)e.Width / e.Height;
                    float screenAspect = (float)screenWidth / screenHeight;

                    if (videoAspect > screenAspect)
                    {
                        layoutParams.Width = screenWidth;
                        layoutParams.Height = (int)(screenWidth / videoAspect);
                    }
                    else
                    {
                        layoutParams.Height = screenHeight;
                        layoutParams.Width = (int)(screenHeight * videoAspect);
                    }

                    SurfaceView.LayoutParameters = layoutParams;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private async void StartProgressUpdates()
        {
            try
            {
                while (MediaPlayer?.IsPlaying == true)
                {
                    ProgressChanged?.Invoke(this, CurrentPosition);
                    await Task.Delay(500);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowLoading(bool show)
        {
            try
            {
                if (LoadingProgressBar != null)
                {
                    (Context as Activity)?.RunOnUiThread(() =>
                    {
                        LoadingProgressBar.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void OnStateChanged(VideoState state)
        {
            StateChanged?.Invoke(this, new VideoStateChangedEventArgs(state));
        }

        private void OnError(VideoErrorEventArgs args)
        {
            Error?.Invoke(this, args);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed) return;

            if (disposing)
            {
                try
                {
                    if (MediaPlayer != null)
                    {
                        MediaPlayer.Prepared -= OnMediaPlayerPrepared;
                        MediaPlayer.Completion -= OnMediaPlayerCompleted;
                        MediaPlayer.Error -= OnMediaPlayerError;
                        MediaPlayer.BufferingUpdate -= OnMediaPlayerBuffering;
                        MediaPlayer.Info -= OnMediaPlayerInfo;
                        MediaPlayer.VideoSizeChanged -= OnMediaPlayerVideoSizeChanged;

                        if (MediaPlayer.IsPlaying)
                        {
                            MediaPlayer.Stop();
                        }

                        MediaPlayer.Release();
                        MediaPlayer.Dispose();
                        MediaPlayer = null;
                    }

                    SurfaceView = null;
                    LoadingProgressBar = null;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            IsDisposed = true;
        }

        ~VideoPlayerController()
        {
            Dispose(false);
        }
    }

    #region Event Args and Enums

    public enum VideoState
    {
        Idle,
        Loading,
        Buffering,
        Ready,
        Playing,
        Paused,
        Stopped,
        Completed,
        Error
    }

    public enum VideoErrorType
    {
        Initialization,
        InvalidUrl,
        NotInitialized,
        LoadFailed,
        PlaybackFailed,
        Unknown
    }

    public class VideoStateChangedEventArgs : EventArgs
    {
        public VideoState State { get; }
        public VideoStateChangedEventArgs(VideoState state)
        {
            State = state;
        }
    }

    public class VideoErrorEventArgs : EventArgs
    {
        public VideoErrorType ErrorType { get; }
        public string Message { get; }
        public VideoErrorEventArgs(VideoErrorType errorType, string message)
        {
            ErrorType = errorType;
            Message = message;
        }
    }

    #endregion
}
