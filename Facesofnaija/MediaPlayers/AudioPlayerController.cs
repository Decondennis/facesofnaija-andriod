using Android.App;
using Android.Content;
using Android.Media;
using System;
using System.Threading.Tasks;
using Facesofnaija.Helpers.Utils;
using Uri = Android.Net.Uri;

namespace Facesofnaija.MediaPlayers
{
    /// <summary>
    /// MediaPlayer-based audio controller for .NET 9
    /// Handles audio post playback (music, voice recordings, etc.)
    /// </summary>
    public class AudioPlayerController : IDisposable
    {
        private readonly Context Context;
        private MediaPlayer MediaPlayer;
        private bool IsInitialized;
        private bool IsDisposed;
        private System.Timers.Timer ProgressTimer;

        public event EventHandler<AudioStateChangedEventArgs> StateChanged;
        public event EventHandler<AudioErrorEventArgs> Error;
        public event EventHandler AudioCompleted;
        public event EventHandler<AudioProgressEventArgs> ProgressChanged;

        public bool IsPlaying => MediaPlayer?.IsPlaying ?? false;
        public int CurrentPosition => MediaPlayer?.CurrentPosition ?? 0;
        public int Duration => MediaPlayer?.Duration ?? 0;
        public string CurrentAudioUrl { get; private set; }
        public string CurrentPostId { get; set; }

        public AudioPlayerController(Context context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Initialize();
        }

        private void Initialize()
        {
            try
            {
                MediaPlayer = new MediaPlayer();
                SetupMediaPlayerEvents();

                MediaPlayer.SetAudioAttributes(new AudioAttributes.Builder()
                    .SetUsage(AudioUsageKind.Media)
                    .SetContentType(AudioContentType.Music)
                    .Build());

                ProgressTimer = new System.Timers.Timer(500); // Update every 500ms
                ProgressTimer.Elapsed += (s, e) =>
                {
                    if (IsPlaying)
                    {
                        OnProgressChanged(new AudioProgressEventArgs(CurrentPosition, Duration));
                    }
                };

                IsInitialized = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new AudioErrorEventArgs(AudioErrorType.Initialization, e.Message));
            }
        }

        /// <summary>
        /// Load and prepare audio from URL
        /// </summary>
        public async Task<bool> LoadAudioAsync(string audioUrl, string postId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(audioUrl))
                {
                    OnError(new AudioErrorEventArgs(AudioErrorType.InvalidUrl, "Audio URL is null or empty"));
                    return false;
                }

                if (!IsInitialized)
                {
                    OnError(new AudioErrorEventArgs(AudioErrorType.NotInitialized, "Player not initialized"));
                    return false;
                }

                CurrentAudioUrl = audioUrl;
                CurrentPostId = postId;

                await Task.Run(() =>
                {
                    MediaPlayer?.Reset();
                    MediaPlayer?.SetDataSource(Context, Uri.Parse(audioUrl));
                    MediaPlayer?.SetAudioAttributes(new AudioAttributes.Builder()
                        .SetUsage(AudioUsageKind.Media)
                        .SetContentType(AudioContentType.Music)
                        .Build());
                    MediaPlayer?.PrepareAsync();
                });

                OnStateChanged(AudioState.Loading);
                return true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new AudioErrorEventArgs(AudioErrorType.LoadFailed, e.Message));
                return false;
            }
        }

        /// <summary>
        /// Play the current audio
        /// </summary>
        public void Play()
        {
            try
            {
                if (MediaPlayer != null && !MediaPlayer.IsPlaying)
                {
                    MediaPlayer.Start();
                    ProgressTimer?.Start();
                    OnStateChanged(AudioState.Playing);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                OnError(new AudioErrorEventArgs(AudioErrorType.PlaybackFailed, e.Message));
            }
        }

        /// <summary>
        /// Pause the current audio
        /// </summary>
        public void Pause()
        {
            try
            {
                if (MediaPlayer?.IsPlaying == true)
                {
                    MediaPlayer.Pause();
                    ProgressTimer?.Stop();
                    OnStateChanged(AudioState.Paused);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Toggle between play and pause
        /// </summary>
        public void TogglePlayPause()
        {
            if (IsPlaying)
                Pause();
            else
                Play();
        }

        /// <summary>
        /// Stop the current audio
        /// </summary>
        public void Stop()
        {
            try
            {
                if (MediaPlayer != null)
                {
                    ProgressTimer?.Stop();
                    MediaPlayer.Stop();
                    MediaPlayer.Reset();
                    OnStateChanged(AudioState.Stopped);
                    CurrentAudioUrl = null;
                    CurrentPostId = null;
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
                OnProgressChanged(new AudioProgressEventArgs(positionMs, Duration));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Skip forward by specified milliseconds
        /// </summary>
        public void SkipForward(int milliseconds = 10000)
        {
            var newPosition = Math.Min(CurrentPosition + milliseconds, Duration);
            SeekTo(newPosition);
        }

        /// <summary>
        /// Skip backward by specified milliseconds
        /// </summary>
        public void SkipBackward(int milliseconds = 10000)
        {
            var newPosition = Math.Max(CurrentPosition - milliseconds, 0);
            SeekTo(newPosition);
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
        /// Mute or unmute the audio
        /// </summary>
        public void SetMuted(bool muted)
        {
            SetVolume(muted ? 0f : 1f);
        }

        /// <summary>
        /// Set playback speed (0.5f = half speed, 2.0f = double speed)
        /// Requires API 23+
        /// </summary>
        public void SetPlaybackSpeed(float speed)
        {
            try
            {
                if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.M)
                {
                    var clampedSpeed = Math.Max(0.5f, Math.Min(2.0f, speed));
                    var playbackParams = new PlaybackParams();
                    playbackParams.SetSpeed(clampedSpeed);
                    if (MediaPlayer != null)
                    {
                        MediaPlayer.PlaybackParams = playbackParams;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
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
                OnStateChanged(AudioState.Ready);
                // Auto-play after preparation
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
                ProgressTimer?.Stop();
                OnStateChanged(AudioState.Completed);
                AudioCompleted?.Invoke(this, EventArgs.Empty);
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
                var errorMsg = $"MediaPlayer Error: {e.What}, Extra: {e.Extra}";
                OnError(new AudioErrorEventArgs(AudioErrorType.PlaybackFailed, errorMsg));
                OnStateChanged(AudioState.Error);
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
                if (e.Percent < 100)
                {
                    OnStateChanged(AudioState.Buffering);
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
                        OnStateChanged(AudioState.Buffering);
                        break;
                    case MediaInfo.BufferingEnd:
                        OnStateChanged(AudioState.Playing);
                        break;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OnStateChanged(AudioState state)
        {
            StateChanged?.Invoke(this, new AudioStateChangedEventArgs(state));
        }

        private void OnProgressChanged(AudioProgressEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        private void OnError(AudioErrorEventArgs args)
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
                    ProgressTimer?.Stop();
                    ProgressTimer?.Dispose();
                    ProgressTimer = null;

                    if (MediaPlayer != null)
                    {
                        MediaPlayer.Prepared -= OnMediaPlayerPrepared;
                        MediaPlayer.Completion -= OnMediaPlayerCompleted;
                        MediaPlayer.Error -= OnMediaPlayerError;
                        MediaPlayer.BufferingUpdate -= OnMediaPlayerBuffering;
                        MediaPlayer.Info -= OnMediaPlayerInfo;

                        if (MediaPlayer.IsPlaying)
                        {
                            MediaPlayer.Stop();
                        }

                        MediaPlayer.Release();
                        MediaPlayer.Dispose();
                        MediaPlayer = null;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            IsDisposed = true;
        }

        ~AudioPlayerController()
        {
            Dispose(false);
        }
    }

    #region Event Args and Enums

    public enum AudioState
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

    public enum AudioErrorType
    {
        Initialization,
        InvalidUrl,
        NotInitialized,
        LoadFailed,
        PlaybackFailed,
        Unknown
    }

    public class AudioStateChangedEventArgs : EventArgs
    {
        public AudioState State { get; }
        public AudioStateChangedEventArgs(AudioState state)
        {
            State = state;
        }
    }

    public class AudioProgressEventArgs : EventArgs
    {
        public int Position { get; }
        public int Duration { get; }
        public double ProgressPercentage => Duration > 0 ? (Position * 100.0 / Duration) : 0;

        public AudioProgressEventArgs(int position, int duration)
        {
            Position = position;
            Duration = duration;
        }
    }

    public class AudioErrorEventArgs : EventArgs
    {
        public AudioErrorType ErrorType { get; }
        public string Message { get; }
        public AudioErrorEventArgs(AudioErrorType errorType, string message)
        {
            ErrorType = errorType;
            Message = message;
        }
    }

    #endregion
}
