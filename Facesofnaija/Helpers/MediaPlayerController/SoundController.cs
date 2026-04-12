using Android.App;
using Android.Content;
using Android.Widget;
using System;
using System.Collections.Generic;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers;
using Facesofnaija.Helpers.Model;
using WoWonderClient.Classes.Posts;

namespace Facesofnaija.Helpers.MediaPlayerController
{
    /// <summary>
    /// Audio controller for sound posts - migrated from ExoPlayer to MediaPlayer
    /// </summary>
    public class SoundController : IDisposable
    {
        private readonly Activity ActivityContext;
        private readonly Context GlobalContext;
        private AudioPlayerController AudioController;
        private List<PostDataObject> PlaylistQueue;
        private int CurrentPlaylistIndex = -1;

        // UI Controls
        private ImageView BtnPlay;
        private ImageView BtnNext;
        private ImageView BtnPrevious;
        private SeekBar ProgressSeekBar;
        private TextView TxtCurrentTime;
        private TextView TxtTotalDuration;

        public string PostId { get; set; }
        public bool AutoPlayNext { get; set; } = false;

        public SoundController(Activity activityContext)
        {
            ActivityContext = activityContext;
            GlobalContext = Application.Context;
            AudioController = new AudioPlayerController(GlobalContext);
            PlaylistQueue = new List<PostDataObject>();

            SetupAudioEvents();
        }

        private void SetupAudioEvents()
        {
            try
            {
                AudioController.StateChanged += (s, e) =>
                {
                    ActivityContext?.RunOnUiThread(() =>
                    {
                        UpdatePlayPauseButton(e.State == AudioState.Playing);
                    });
                };

                AudioController.ProgressChanged += (s, e) =>
                {
                    ActivityContext?.RunOnUiThread(() =>
                    {
                        UpdateProgress(e.Position, e.Duration);
                    });
                };

                AudioController.AudioCompleted += (s, e) =>
                {
                    if (AutoPlayNext)
                    {
                        PlayNext();
                    }
                };

                AudioController.Error += (s, e) =>
                {
                    ActivityContext?.RunOnUiThread(() =>
                    {
                        Toast.MakeText(GlobalContext, $"Audio Error: {e.Message}", ToastLength.Short)?.Show();
                    });
                };
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void InitializeUi(ImageView btnPlay = null, ImageView btnNext = null, ImageView btnPrevious = null, 
            SeekBar seekBar = null, TextView txtCurrentTime = null, TextView txtTotalDuration = null)
        {
            try
            {
                BtnPlay = btnPlay;
                BtnNext = btnNext;
                BtnPrevious = btnPrevious;
                ProgressSeekBar = seekBar;
                TxtCurrentTime = txtCurrentTime;
                TxtTotalDuration = txtTotalDuration;

                if (BtnPlay != null)
                {
                    BtnPlay.Click += (s, e) => StartOrPausePlayer();
                }

                if (BtnNext != null)
                {
                    BtnNext.Click += (s, e) => BtnForwardOnClick();
                }

                if (BtnPrevious != null)
                {
                    BtnPrevious.Click += (s, e) => BtnBackwardOnClick();
                }

                if (ProgressSeekBar != null)
                {
                    ProgressSeekBar.ProgressChanged += (s, e) =>
                    {
                        if (e.FromUser)
                        {
                            SeekTo(e.Progress);
                        }
                    };
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public async void StartPlaySound(object args, bool autoNext = false)
        {
            try
            {
                AutoPlayNext = autoNext;

                if (args is PostDataObject postData)
                {
                    PostId = postData.PostId;

                    string audioUrl = postData.PostFileFull ?? postData.PostRecord;

                    if (string.IsNullOrEmpty(audioUrl))
                    {
                        Toast.MakeText(GlobalContext, "Audio URL not found", ToastLength.Short)?.Show();
                        return;
                    }

                    await AudioController.LoadAudioAsync(audioUrl, postData.PostId);
                }
                else if (args is string audioUrl)
                {
                    await AudioController.LoadAudioAsync(audioUrl);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void StartOrPausePlayer()
        {
            try
            {
                AudioController?.TogglePlayPause();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BtnForwardOnClick()
        {
            try
            {
                PlayNext();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void BtnBackwardOnClick()
        {
            try
            {
                PlayPrevious();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayNext()
        {
            try
            {
                if (PlaylistQueue != null && PlaylistQueue.Count > 0)
                {
                    CurrentPlaylistIndex++;
                    if (CurrentPlaylistIndex >= PlaylistQueue.Count)
                    {
                        CurrentPlaylistIndex = 0; // Loop to start
                    }

                    StartPlaySound(PlaylistQueue[CurrentPlaylistIndex], AutoPlayNext);
                }
                else
                {
                    // Skip forward 10 seconds if no playlist
                    AudioController?.SkipForward(10000);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void PlayPrevious()
        {
            try
            {
                if (PlaylistQueue != null && PlaylistQueue.Count > 0)
                {
                    CurrentPlaylistIndex--;
                    if (CurrentPlaylistIndex < 0)
                    {
                        CurrentPlaylistIndex = PlaylistQueue.Count - 1; // Loop to end
                    }

                    StartPlaySound(PlaylistQueue[CurrentPlaylistIndex], AutoPlayNext);
                }
                else
                {
                    // Skip backward 10 seconds if no playlist
                    AudioController?.SkipBackward(10000);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void SeekTo(int position)
        {
            try
            {
                AudioController?.SeekTo(position);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateProgress(int currentPosition, int duration)
        {
            try
            {
                if (ProgressSeekBar != null)
                {
                    ProgressSeekBar.Max = duration;
                    ProgressSeekBar.Progress = currentPosition;
                }

                if (TxtCurrentTime != null)
                {
                    TxtCurrentTime.Text = FormatTime(currentPosition);
                }

                if (TxtTotalDuration != null)
                {
                    TxtTotalDuration.Text = FormatTime(duration);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void UpdatePlayPauseButton(bool isPlaying)
        {
            try
            {
                if (BtnPlay != null)
                {
                    int iconRes = isPlaying ? Resource.Drawable.icon_pause_vector : Resource.Drawable.icon_play_vector;
                    BtnPlay.SetImageResource(iconRes);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private string FormatTime(int milliseconds)
        {
            int seconds = milliseconds / 1000;
            int minutes = seconds / 60;
            seconds = seconds % 60;
            return $"{minutes:D2}:{seconds:D2}";
        }

        public void SetPlaylist(List<PostDataObject> playlist, int startIndex = 0)
        {
            PlaylistQueue = playlist;
            CurrentPlaylistIndex = startIndex;
        }

        public void StopSound()
        {
            try
            {
                AudioController?.Stop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void Dispose()
        {
            try
            {
                AudioController?.Dispose();
                AudioController = null;
                PlaylistQueue?.Clear();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
