using Android.Content;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers;

namespace Facesofnaija.Activities.NativePost.Extra
{
    /// <summary>
    /// Media controller for managing video playback in feeds
    /// Migrated from ExoPlayer to MediaPlayer for .NET 9
    /// </summary>
    public class ExoMediaController : IDisposable
    {
        private readonly Context Context;
        private VideoPlayerController CurrentVideoController;
        private readonly Dictionary<string, VideoPlayerController> VideoCachePool;
        private const int MaxCacheSize = 3;

        public bool IsPlaying => CurrentVideoController?.IsPlaying ?? false;
        public string CurrentVideoUrl { get; private set; }

        public ExoMediaController(Context context)
        {
            Context = context;
            VideoCachePool = new Dictionary<string, VideoPlayerController>();
        }

        /// <summary>
        /// Preload video for caching (for feed scroll optimization)
        /// </summary>
        public void PrelaodVideoCach(string videoUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl) || VideoCachePool.ContainsKey(videoUrl))
                    return;

                // Limit cache size
                if (VideoCachePool.Count >= MaxCacheSize)
                {
                    // Remove oldest cached video
                    var firstKey = new List<string>(VideoCachePool.Keys)[0];
                    VideoCachePool[firstKey]?.Dispose();
                    VideoCachePool.Remove(firstKey);
                }

                // Note: Actual precaching would require downloading video data
                // For now, we'll just track URLs that should be ready to play
                // This is a simplified implementation
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        /// <summary>
        /// Initialize and play video in a surface view
        /// </summary>
        public async void PlayVideo(string videoUrl, SurfaceView surfaceView, ProgressBar progressBar = null)
        {
            try
            {
                if (string.IsNullOrEmpty(videoUrl))
                    return;

                // Stop current video if playing
                if (CurrentVideoController != null && CurrentVideoUrl != videoUrl)
                {
                    CurrentVideoController.Stop();
                }

                CurrentVideoUrl = videoUrl;

                // Reuse or create new controller
                if (!VideoCachePool.TryGetValue(videoUrl, out CurrentVideoController))
                {
                    CurrentVideoController = new VideoPlayerController(Context);
                    CurrentVideoController.InitializePlayer(surfaceView, progressBar);
                    VideoCachePool[videoUrl] = CurrentVideoController;
                }

                await CurrentVideoController.LoadVideoAsync(videoUrl);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Play()
        {
            try
            {
                CurrentVideoController?.Play();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Pause()
        {
            try
            {
                CurrentVideoController?.Pause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Stop()
        {
            try
            {
                CurrentVideoController?.Stop();
                CurrentVideoUrl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SeekTo(int positionMs)
        {
            try
            {
                CurrentVideoController?.SeekTo(positionMs);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetVolume(float volume)
        {
            try
            {
                CurrentVideoController?.SetVolume(volume);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SetMuted(bool muted)
        {
            try
            {
                CurrentVideoController?.SetMuted(muted);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Release()
        {
            try
            {
                foreach (var controller in VideoCachePool.Values)
                {
                    controller?.Dispose();
                }
                VideoCachePool.Clear();
                CurrentVideoController = null;
                CurrentVideoUrl = null;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Dispose()
        {
            Release();
        }
    }
}
