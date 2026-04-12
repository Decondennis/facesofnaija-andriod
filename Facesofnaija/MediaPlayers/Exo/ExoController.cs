using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.MediaPlayers.Exo
{
    /// <summary>
    /// Video controller wrapper - migrated from ExoPlayer to MediaPlayer
    /// </summary>
    public class ExoController : IDisposable
    {
        private readonly Context Context;
        private VideoPlayerController VideoController;

        public bool IsPlaying => VideoController?.IsPlaying ?? false;
        public int CurrentPosition => VideoController?.CurrentPosition ?? 0;
        public int Duration => VideoController?.Duration ?? 0;

        public ExoController(Context context)
        {
            Context = context;
        }

        /// <summary>
        /// Initialize with surface view for video rendering
        /// </summary>
        public void Initialize(SurfaceView surfaceView, ProgressBar progressBar = null)
        {
            try
            {
                VideoController = new VideoPlayerController(Context);
                VideoController.InitializePlayer(surfaceView, progressBar);

                VideoController.StateChanged += (s, e) =>
                {
                    // Handle state changes if needed
                };

                VideoController.Error += (s, e) =>
                {
                    Methods.DisplayReportResultTrack(new Exception($"Video Error: {e.Message}"));
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Load and play video from URL
        /// </summary>
        public async void PlayVideo(string url)
        {
            try
            {
                if (VideoController == null)
                {
                    Methods.DisplayReportResultTrack(new Exception("VideoController not initialized"));
                    return;
                }

                bool loaded = await VideoController.LoadVideoAsync(url);
                if (!loaded)
                {
                    Methods.DisplayReportResultTrack(new Exception("Failed to load video"));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Play()
        {
            VideoController?.Play();
        }

        public void PauseVideo()
        {
            VideoController?.Pause();
        }

        public void StopVideo()
        {
            VideoController?.Stop();
        }

        public void ReleaseVideo()
        {
            VideoController?.Dispose();
            VideoController = null;
        }

        public void SeekTo(int positionMs)
        {
            VideoController?.SeekTo(positionMs);
        }

        public void SetVolume(float volume)
        {
            VideoController?.SetVolume(volume);
        }

        public void SetMuted(bool muted)
        {
            VideoController?.SetMuted(muted);
        }

        public void Dispose()
        {
            ReleaseVideo();
        }
    }
}
