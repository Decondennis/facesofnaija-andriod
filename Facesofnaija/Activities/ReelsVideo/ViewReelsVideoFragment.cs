using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers.Exo;
using WoWonderClient.Classes.Posts;

namespace Facesofnaija.Activities.ReelsVideo
{
    public class ViewReelsVideoFragment : AndroidX.Fragment.App.Fragment
    {
        private static ViewReelsVideoFragment Instance;

        private ReelsVideoDetailsActivity GlobalContext;
        private PostDataObject DataVideos;
        private PostModelType PostFeedType;

        private SurfaceView ReelsVideoSurface;
        private ProgressBar ReelsVideoProgress;
        private ImageView BtnBack;
        public LinearLayout LikeLayout;
        public ImageView ImgLike;
        public TextView TxtLikeCount;
        private LinearLayout CommentLayout;
        private ImageView ImgComment;
        private TextView TxtCommentCount;
        private TextView TxtUsername;
        private TextView TxtCaption;

        private ExoController ExoController;
        private bool MIsVisibleToUser;
        private string PendingVideoUrl;

        public TubePlayerView TubePlayerView { get; set; }

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                GlobalContext = (ReelsVideoDetailsActivity)Activity;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                return inflater.Inflate(Resource.Layout.ReelsVideoSwipeLayout, container, false);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                Instance = this;
                InitComponent(view);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();
                DataVideos = JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("DataItem") ?? "");
                if (DataVideos != null)
                    LoadData(DataVideos);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnPause()
        {
            try
            {
                base.OnPause();
                StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();
                if (MIsVisibleToUser)
                    StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroyView()
        {
            try
            {
                ReleaseVideo();
                base.OnDestroyView();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                ReleaseVideo();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static ViewReelsVideoFragment GetInstance()
        {
            try
            {
                return Instance;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        private void InitComponent(View view)
        {
            try
            {
                ReelsVideoSurface = view.FindViewById<SurfaceView>(Resource.Id.reels_video_surface);
                ReelsVideoProgress = view.FindViewById<ProgressBar>(Resource.Id.reels_video_progress);
                BtnBack = view.FindViewById<ImageView>(Resource.Id.btn_back_reels);
                LikeLayout = view.FindViewById<LinearLayout>(Resource.Id.like_layout);
                ImgLike = view.FindViewById<ImageView>(Resource.Id.img_like);
                TxtLikeCount = view.FindViewById<TextView>(Resource.Id.txt_like_count);
                CommentLayout = view.FindViewById<LinearLayout>(Resource.Id.comment_layout);
                ImgComment = view.FindViewById<ImageView>(Resource.Id.img_comment);
                TxtCommentCount = view.FindViewById<TextView>(Resource.Id.txt_comment_count);
                TxtUsername = view.FindViewById<TextView>(Resource.Id.reels_user_name);
                TxtCaption = view.FindViewById<TextView>(Resource.Id.reels_caption);

                ExoController = new ExoController(Activity);
                if (ReelsVideoSurface != null)
                {
                    ReelsVideoSurface.Holder.AddCallback(new ReelsSurfaceCallback(this));
                }

                BtnBack.Click += BtnBackOnClick;
                LikeLayout.Click += LikeLayoutOnClick;
                CommentLayout.Click += CommentLayoutOnClick;
                TxtUsername.Click += TxtUsernameOnClick;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class ReelsSurfaceCallback : Java.Lang.Object, ISurfaceHolderCallback
        {
            private readonly ViewReelsVideoFragment Fragment;

            public ReelsSurfaceCallback(ViewReelsVideoFragment fragment)
            {
                Fragment = fragment;
            }

            public void SurfaceCreated(ISurfaceHolder holder)
            {
                try
                {
                    Fragment.ExoController?.Initialize(Fragment.ReelsVideoSurface, Fragment.ReelsVideoProgress);
                    if (!string.IsNullOrEmpty(Fragment.PendingVideoUrl))
                    {
                        var url = Fragment.PendingVideoUrl;
                        Fragment.PendingVideoUrl = null;
                        Fragment.ExoController?.PlayVideo(url);
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void SurfaceChanged(ISurfaceHolder holder, Android.Graphics.Format format, int width, int height)
            {
            }

            public void SurfaceDestroyed(ISurfaceHolder holder)
            {
                try
                {
                    Fragment.ExoController?.StopVideo();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public void StopVideo()
        {
            try
            {
                ExoController?.StopVideo();
                TabbedMainActivity.GetInstance()?.SetOffWakeLock();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void ReleaseVideo()
        {
            try
            {
                ExoController?.ReleaseVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void BtnBackOnClick(object sender, EventArgs e)
        {
            try
            {
                GlobalContext?.Finish();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void TxtUsernameOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataVideos != null)
                    WoWonderTools.OpenProfile(Activity, DataVideos.UserId, DataVideos.Publisher);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LikeLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataVideos == null)
                    return;

                if (LikeLayout.Tag?.ToString() == "Liked")
                {
                    DataVideos.Reaction ??= new Reaction();
                    if (DataVideos.Reaction.Count > 0)
                        DataVideos.Reaction.Count--;
                    else
                        DataVideos.Reaction.Count = 0;

                    DataVideos.Reaction.Type = "";
                    DataVideos.Reaction.IsReacted = false;

                    TxtLikeCount.Text = Methods.FunString.FormatPriceValue(DataVideos.Reaction.Count);

                    ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                    ImgLike.SetColorFilter(Color.White);
                    LikeLayout.Tag = "Like";
                }
                else
                {
                    new ReactionReelsVideo(Activity)?.ClickDialog(new GlobalClickEventArgs
                    {
                        NewsFeedClass = DataVideos
                    });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void CommentLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (DataVideos == null)
                    return;

                var clickListener = new PostClickListener(GlobalContext, NativeFeedType.Global);
                clickListener.CommentPostClick(new GlobalClickEventArgs
                {
                    NewsFeedClass = DataVideos
                });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void LoadData(PostDataObject dataObject)
        {
            try
            {
                ListUtils.VideoReelsViewsList ??= new ObservableCollection<PostDataObject>();
                if (ListUtils.VideoReelsViewsList.All(a => a.Id != dataObject.Id))
                    ListUtils.VideoReelsViewsList.Add(dataObject);

                PostFeedType = PostFunctions.GetAdapterType(dataObject);

                var url = dataObject.PostFileFull;
                if (string.IsNullOrEmpty(url))
                    url = dataObject.PostFile;
                if (!string.IsNullOrEmpty(url) && !url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    url = AppSettings.SiteUrl.TrimEnd('/') + "/" + url.TrimStart('/');

                if (!string.IsNullOrEmpty(url))
                {
                    PendingVideoUrl = url;
                    if (ExoController?.IsReady == true)
                    {
                        PendingVideoUrl = null;
                        ExoController.PlayVideo(url);
                    }
                }

                if (dataObject.PostPrivacy == "4")
                    TxtUsername.Text = Activity.GetText(Resource.String.Lbl_Anonymous);
                else
                    TxtUsername.Text = WoWonderTools.GetNameFinal(dataObject.Publisher);

                TxtCaption.Text = string.IsNullOrEmpty(dataObject.Orginaltext) || string.IsNullOrWhiteSpace(dataObject.Orginaltext)
                    ? ""
                    : dataObject.Orginaltext;

                TxtCommentCount.Text = dataObject.PostComments;

                dataObject.Reaction ??= new Reaction();
                TxtLikeCount.Text = Methods.FunString.FormatPriceValue(dataObject.Reaction.Count);

                if (dataObject.Reaction.IsReacted != null && dataObject.Reaction.IsReacted.Value)
                {
                    if (!string.IsNullOrEmpty(dataObject.Reaction.Type))
                    {
                        var react = ListUtils.SettingsSiteList?.PostReactionsTypes?.FirstOrDefault(a => a.Value?.Id == dataObject.Reaction.Type).Value?.Id ?? "";
                        switch (react)
                        {
                            case "1":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                break;
                            case "2":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_love);
                                break;
                            case "3":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_haha);
                                break;
                            case "4":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_wow);
                                break;
                            case "5":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_sad);
                                break;
                            case "6":
                                ImgLike.SetImageResource(Resource.Drawable.emoji_angry);
                                break;
                            default:
                                if (dataObject.Reaction.Count > 0)
                                    ImgLike.SetImageResource(Resource.Drawable.emoji_like);
                                break;
                        }
                        LikeLayout.Tag = "Liked";
                    }
                    ImgLike.ClearColorFilter();
                }
                else
                {
                    ImgLike.SetImageResource(Resource.Drawable.icon_heart_vector);
                    ImgLike.SetColorFilter(Color.White);
                    LikeLayout.Tag = "Like";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
