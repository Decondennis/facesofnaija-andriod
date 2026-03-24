using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Upstream.Cache;
using Google.Android.Material.FloatingActionButton;
using Java.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Activities.NativePost.VideoExoPlayer;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.ExoVideoPlayer;
using Facesofnaija.Helpers.ShimmerUtils;
using Facesofnaija.Helpers.Utils;
using Console = System.Console;
using Exception = System.Exception;
using LayoutDirection = Android.Views.LayoutDirection;
using Object = Java.Lang.Object;
using Uri = Android.Net.Uri;

namespace Facesofnaija.Activities.NativePost.Extra
{
    public class WRecyclerView : RecyclerView, IExoPlayer.IListener
    {
        private static WRecyclerView Instance;
        public enum VolumeState { On, Off }

        public FrameLayout MediaContainerLayout;
        public ImageView PlayControl;

        private int VideoSurfaceDefaultHeight;
        private int ScreenDefaultHeight;
        public Context MainContext;
        public bool IsVideoViewAdded;
        public Uri VideoUrl;
        public string Hash;
        public RecyclerScrollListener MainScrollEvent;
        public NativePostAdapter NativeFeedAdapter;
        public SwipeRefreshLayout SwipeRefreshLayoutView;
        public FloatingActionButton PopupBubbleView;
        public TemplateShimmerInflater ShimmerInflater;

        public CacheDataSource.Factory CacheDataSourceFactory;
        public static SimpleCache Cache;
        private static DefaultBandwidthMeter BandwidthMeter;

        public static string Filter { set; get; }
        public static string PostType { set; get; }

        public ApiPostAsync ApiPostAsync;

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

                LayoutDirection = AppSettings.FlowDirectionRightToLeft switch
                {
                    true => LayoutDirection.Rtl,
                    _ => LayoutDirection
                };

                HasFixedSize = false;
                SetItemViewCacheSize(150);
                //SetItemAnimator(new DefaultItemAnimator());
                GetRecycledViewPool().SetMaxRecycledViews(1, 20);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.HeaderPost, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.BottomPostPart, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.ImagePost, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.TextSectionPostPart, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.CommentSection, 20);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AddCommentSection, 20);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.VideoPost, 9);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.ColorPost, 15);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage2, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage3, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImage4, 10);
                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.MultiImages, 6);

                GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PrevBottomPostPart, 15);

                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PromotePost, 10);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SharedHeaderPost, 50);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.Story, 1);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SuggestedPagesBox, 1);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SuggestedGroupsBox, 1);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.SuggestedUsersBox, 1);

                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.PollPost, 50);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AlertBox, 2);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.YoutubePost, 50);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.VideoPost, 50);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdMob1, 10);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdMob2, 10);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdMob3, 10);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AddPostBox, 1);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.AdsPost, 6);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.DeepSoundPost, 15);
                //GetRecycledViewPool().SetMaxRecycledViews((int)PostModelType.Divider, 250);

                ClearAnimation();
                var f = GetItemAnimator();
                ((SimpleItemAnimator)f).SupportsChangeAnimations = false;
                f.ChangeDuration = 0;

                GetItemAnimator().ChangeDuration = 0;
                this.SetItemAnimator(null);

                OverScrollMode = OverScrollMode.Never;



                //DividerItemDecoration itemDecorator = new DividerItemDecoration(MainContext, DividerItemDecoration.Vertical);
                //itemDecorator.SetDrawable(ContextCompat.GetDrawable(MainContext, Resource.Drawable.Post_Devider_Shape));
                //AddItemDecoration(itemDecorator);

                var point = Methods.App.OverrideGetSize(MainContext);
                if (point != null)
                {
                    VideoSurfaceDefaultHeight = point.X;
                    ScreenDefaultHeight = point.Y;
                }

                var screenSize = GetScreenSize(MainContext);
                videoItemHeight = screenSize.X;
                screenHeight = screenSize.Y;


                ////===================== Exo Player ========================
                SetPlayer();
                AddOnChildAttachStateChangeListener(new NewAttachStateChangeListener(this));
                ////=============================================

                //MainScrollEvent = new RecyclerScrollListener(this);
                //AddOnScrollListener(MainScrollEvent);
                //AddOnChildAttachStateChangeListener(new ChildAttachStateChangeListener(this));
                //MainScrollEvent.LoadMoreEvent += MainScrollEvent_LoadMoreEvent;
                //MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static WRecyclerView GetInstance()
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

        public void SetXAdapter(NativePostAdapter adapter, SwipeRefreshLayout swipeRefreshLayout)
        {
            try
            {
                NativeFeedAdapter = adapter;
                SwipeRefreshLayoutView = swipeRefreshLayout;
                ApiPostAsync = new ApiPostAsync(this, adapter);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string GetFilter()
        {
            return string.IsNullOrEmpty(Filter) ? "0" : Filter;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="postType">'photos','video','music','files','maps','text'</param>
        /// <param name="filter">0(All),1(People I Follow)</param>
        public void SetPostAndFilterType(int postType, string filter)
        {
            try
            {
                if (postType == 0) //All
                {
                    PostType = "";
                }
                else if (postType == 1) //text
                {
                    PostType = "text";
                }
                else if (postType == 2) //image
                {
                    PostType = "photos";
                }
                else if (postType == 3) //video
                {
                    PostType = "video";
                }
                else if (postType == 4) //Music
                {
                    PostType = "music";
                }
                else if (postType == 5) //File
                {
                    PostType = "files";
                }
                else if (postType == 6) //Map
                {
                    PostType = "maps";
                }

                Filter = filter;

                var tab = TabbedMainActivity.GetInstance()?.NewsFeedTab;
                if (tab != null)
                {
                    tab.SwipeRefreshLayout.Refreshing = true;
                    tab.SwipeRefreshLayoutOnRefresh(this, EventArgs.Empty);

                    tab.PostFeedAdapter?.ListDiffer?.Clear();
                    tab.PostFeedAdapter?.NotifyDataSetChanged();

                    tab.LoadPost(false);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public string GetPostType()
        {
            return PostType ?? "";
        }

        //TemplateShimmer 
        public void SetXTemplateShimmer(TemplateShimmerInflater shimmerInflater)
        {
            try
            {
                if (shimmerInflater != null)
                {
                    ShimmerInflater = shimmerInflater;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        //PopupBubble 
        public void SetXPopupBubble(FloatingActionButton popupBubble)
        {
            try
            {
                if (popupBubble != null)
                {
                    PopupBubbleView = popupBubble;
                    PopupBubbleView.Visibility = ViewStates.Gone;
                    PopupBubbleView.Click += PopupBubbleViewOnClick;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PopupBubbleViewOnClick(object sender, EventArgs e)
        {
            try
            {
                ApiPostAsync.LoadTopDataApi(ListUtils.NewPostList);

                PopupBubbleView.Visibility = ViewStates.Gone;
                ScrollToPosition(0);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        /// <summary>
        /// //////
        /// </summary>
        public int videoItemHeight = 0;
        public int screenHeight = 0;
        public IVideoPlayerEventListener CurrentVideoHolder;
        public NewsFeedExoPlayerVideoEventListener PlayerVideoEventListener;



        public ExoMediaControl ExoMediaController;

        private void SetPlayer()
        {
            try
            {

                if (MainContext == null)
                    return;


                ExoMediaController = new ExoMediaControl().Build(MainContext);
                ExoMediaController.ConfigureDataSources();
                ExoMediaController.BuildPlayer();

                PlayerVideoEventListener = new NewsFeedExoPlayerVideoEventListener(this);
                ExoMediaController.Exoplayer.AddListener(PlayerVideoEventListener);

                //MediaItem mediaItem = new MediaItem.Builder().SetUri(videoUri).SetMediaId(mediaId).SetTag(metadata).Build();

                //BandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(MainContext);

                //videoTrackSelectionFactory = new AdaptiveTrackSelection.Factory();
                //trackSelector = new DefaultTrackSelector(MainContext, videoTrackSelectionFactory);
                //IRenderersFactory renderersFactory = new DefaultRenderersFactory(MainContext);

                //SimpleVideoPlayer = ExoPlayer.Builder(this).Build();


                //var trackSelectorParameters = (DefaultTrackSelector.Parameters)MainContext.GetParcelable("track_selector_parameters");
                //// trackSelector.SetParameters(new DefaultTrackSelector.ParametersBuilder(MainContext));

                //trackSelector.SetParameters(trackSelector.BuildUponParameters().SetMaxVideoSizeSd().SetMaxVideoFrameRate(1080));

                //var loadControl = new DefaultLoadControl.Builder().SetBufferDurationsMs(2000, 10 * 1000, 2000, 2000).CreateDefaultLoadControl();

                //SimpleVideoPlayer = new IExoPlayer.Builder(MainContext).SetTrackSelector(trackSelector).SetLoadControl(loadControl).Build();

                //DefaultDataSourceFac = new DefaultDataSourceFactory(MainContext, Util.GetUserAgent(MainContext, AppSettings.ApplicationName), BandwidthMeter);


                //if (!PlayerSettings.EnableOfflineMode)
                //    return;



                //CacheDataSourceFactory ??= new CacheDataSourceFactory(Cache, DefaultDataSourceFac);


                //PlayerVideoEventListener.MFullScreenButton.SetOnClickListener(new VideoOnFullScreenClick(PlayerVideoEventListener.MFullScreenButton, this));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public int GetVisibleVideoHeight(int position, LinearLayoutManager linearLayoutManager)
        {
            View child = linearLayoutManager.FindViewByPosition(position);

            if (child == null)
                return -1;

            int[] location = new int[2];
            child.GetLocationInWindow(location);
            return location[1] < 0 ? location[1] + videoItemHeight : screenHeight - location[1];
        }

        public Point GetScreenSize(Context cont)
        {
            var width = cont.Resources.DisplayMetrics.WidthPixels;
            var height = cont.Resources.DisplayMetrics.HeightPixels;

            Point size = new Point(width, height);
            return size;
        }

        public bool IsVideoItem(NativePostAdapter nativeFeedAdapter, int position)
        {
            if (nativeFeedAdapter.GetItemViewType(position) == (int)PostModelType.VideoPost)
                return true;

            return false;
        }

        public int FindCurrentVideoPosition()
        {
            var result = -1;
            var linearLayoutManager = ((LinearLayoutManager)Objects.RequireNonNull(GetLayoutManager()));
            var firstPosition = linearLayoutManager.FindFirstVisibleItemPosition();
            var lastPosition = linearLayoutManager.FindLastVisibleItemPosition();
            var percentMax = 0;
            int position = firstPosition;
            if (firstPosition <= lastPosition)
            {
                while (true)
                {
                    if (this.IsVideoItem(NativeFeedAdapter, position))
                    {
                        int percent = this.GetVisibleVideoHeight(position, linearLayoutManager);
                        if (percentMax < percent)
                        {
                            percentMax = percent;
                            result = position;
                        }
                    }

                    if (position == lastPosition)
                    {
                        break;
                    }

                    ++position;
                }
            }

            return result;
        }

        public IVideoPlayerEventListener GetTargetVideoHolder()
        {

            try
            {
                int position = FindCurrentVideoPosition();
                if (position == -1)
                {
                    return null;
                }
                else
                {
                    ViewHolder viewHolder = FindViewHolderForAdapterPosition(position);

                    if (viewHolder is IVideoPlayerEventListener holder)
                        return holder;
                    else
                        return null;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null;
            }
        }


        public void PlayVideo(IVideoPlayerEventListener targetViewHolder, bool isclick)
        {
            try
            {
                if (isclick == false && CurrentVideoHolder != null && CurrentVideoHolder == targetViewHolder)
                    return;

                CurrentVideoHolder?.OnPlayCanceled();
                CurrentVideoHolder = targetViewHolder;

                if (BandwidthMeter == null)
                    BandwidthMeter = DefaultBandwidthMeter.GetSingletonInstance(MainContext);

                VideoUrl = targetViewHolder.GetVideoUrl();

                PlayerVideoEventListener.SetFeedHolderToListenOn(CurrentVideoHolder.GetViewHolderOfListener());
                ExoMediaController.PrepareMediaSource(VideoUrl.ToString());
                CurrentVideoHolder?.OnPrePlay(ExoMediaController.Exoplayer);

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                CurrentVideoHolder = null;
            }
        }



        public class NewAttachStateChangeListener : Object, IOnChildAttachStateChangeListener
        {
            private readonly WRecyclerView XRecyclerView;

            public NewAttachStateChangeListener(WRecyclerView recyclerView)
            {
                XRecyclerView = recyclerView;
            }

            public void OnChildViewAttachedToWindow(View view)
            {
                try
                {

                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public void OnChildViewDetachedFromWindow(View view)
            {
                try
                {
                    var holder = XRecyclerView.FindContainingViewHolder(view);
                    if (holder is IVideoPlayerEventListener viewholder)
                    {
                        viewholder.OnPlayCanceled();
                        XRecyclerView.CurrentVideoHolder = null;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        public void MainScrollEvent_LoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                switch (NativeFeedAdapter.NativePostType)
                {
                    case NativeFeedType.Memories:
                    case NativeFeedType.Share:
                        return;
                }

                var list = NativeFeedAdapter.ListDiffer;
                // var list = new List<AdapterModelsClass>(diff);

                if (list.Count <= int.Parse(AppSettings.PostApiLimitOnScroll) && !MainScrollEvent.IsLoading)
                    return;


                //NativeFeedAdapter.SetLoading();

                //if (ApiPostAsync.PostCacheList?.Count > 0 && NativeFeedAdapter.NativePostType == NativeFeedType.Global)
                //{
                //  var addedData = ApiPostAsync.LoadBottomDataApi(ApiPostAsync.PostCacheList.Take(20).ToList());
                //  if (addedData)
                //      return;
                //}

                var item = list.LastOrDefault();
                var lastItem = list.IndexOf(item);

                item = list[lastItem];

                string offset;

                switch (item.TypeView)
                {
                    case PostModelType.Divider:
                    case PostModelType.ViewProgress:
                    case PostModelType.AdMob1:
                    case PostModelType.AdMob2:
                    case PostModelType.AdMob3:
                    case PostModelType.FbAdNative:
                    case PostModelType.AdsPost:
                    case PostModelType.SuggestedGroupsBox:
                    case PostModelType.SuggestedUsersBox:
                    case PostModelType.CommentSection:
                    case PostModelType.AddCommentSection:
                        item = list.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                        offset = item?.PostData?.PostId ?? "0";
                        Console.WriteLine(offset);
                        break;
                    default:
                        offset = item.PostData?.PostId ?? "0";
                        break;
                }

                Console.WriteLine(offset);


                Trace.BeginSection("MainScrollEvent_LoadMoreEvent ApiPostAsync Simulation");
                ApiPostAsync.ExcuteDataToMainThread(offset, "Add", Hash);


                Trace.EndSection();

                return;

                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(MainContext, MainContext.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                {
                    Trace.BeginSection("MainScrollEvent_LoadMoreEvent ApiPostAsync Simulation");

                    ApiPostAsync.ExcuteDataToMainThread(offset, "Add", Hash);
                    // await ApiPostAsync.FetchNewsFeedApiPosts(offset, "Add", Hash).ConfigureAwait(false); 
                    //PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => NativeFeedAdapter.NativePostType != NativeFeedType.HashTag ? ApiPostAsync.FetchNewsFeedApiPosts(offset) : ApiPostAsync.FetchNewsFeedApiPosts(offset, "Add", Hash) });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void InsertByRowIndex(AdapterModelsClass item, string index = "")
        {
            try
            {
                var diff = NativeFeedAdapter.ListDiffer;
                var diffList = new List<AdapterModelsClass>(diff);

                int countIndex = 1;
                var model1 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                var model2 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                var model3 = diffList.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);

                if (model3 != null)
                    countIndex += diffList.IndexOf(model3);
                else if (model2 != null)
                    countIndex += diffList.IndexOf(model2);
                else if (model1 != null)
                    countIndex += diffList.IndexOf(model1);
                else
                    countIndex = 0;

                countIndex = string.IsNullOrEmpty(index) switch
                {
                    false => Convert.ToInt32(index),
                    _ => countIndex
                };

                diffList.Insert(countIndex, item);

                var emptyStateChecker = diffList.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                if (emptyStateChecker != null && diffList.Count > 1)
                {
                    diffList.Remove(emptyStateChecker);

                }

                NativeFeedAdapter.NotifyItemRangeInserted(diff.Count - 1, diffList.Count);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void RemoveByRowIndex(AdapterModelsClass item)
        {
            try
            {
                var diff = NativeFeedAdapter.ListDiffer;
                var index = diff.IndexOf(item);
                switch (index)
                {
                    case <= 0:
                        return;
                    default:
                        diff.RemoveAt(index);
                        NativeFeedAdapter.NotifyItemRemoved(index);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDetachedFromWindow()
        {
            try
            {
                base.OnDetachedFromWindow();
                if (GetAdapter() != null)
                {
                    SetAdapter(null);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public class RecyclerScrollListener : OnScrollListener
        {
            public delegate void LoadMoreEventHandler(object sender, EventArgs e);

            public event LoadMoreEventHandler LoadMoreEvent;

            public bool IsLoading { get; set; }
            public bool IsScrolling { get; set; }

            private PreCachingLayoutManager LayoutManager { get; set; }
            private readonly WRecyclerView XRecyclerView;

            public RecyclerScrollListener(WRecyclerView recyclerView)
            {
                try
                {
                    XRecyclerView = recyclerView;

                    LayoutManager ??= (PreCachingLayoutManager)recyclerView.GetLayoutManager();
                    IsLoading = false;
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnScrollStateChanged(RecyclerView recyclerView, int newState)
            {
                try
                {
                    Trace.BeginSection("OnScrollStateChanged LoadMoreEvent Simulation");
                    base.OnScrollStateChanged(recyclerView, newState);

                    switch (newState)
                    {
                        case (int)Android.Widget.ScrollState.TouchScroll:
                            {

                                //if (Glide.With(XRecyclerView.Context).IsPaused)
                                //    Glide.With(XRecyclerView.Context).ResumeRequests();
                            }
                            break;
                        case (int)Android.Widget.ScrollState.Fling:
                            IsScrolling = true;
                            //Glide.With(XRecyclerView.Context).PauseRequests();
                            break;
                        case (int)Android.Widget.ScrollState.Idle:
                            {
                                Glide.Get(XRecyclerView.Context).OnLowMemory();
                                //Glide.Get(XRecyclerView.Context).ArrayPool.ClearMemory();
                                //Glide.Get(XRecyclerView.Context).BitmapPool.TrimMemory(2);
                                //Glide.Get(XRecyclerView.Context).BitmapPool.ClearMemory();
                                GC.Collect();

                                switch (AppSettings.AutoPlayVideo)
                                {
                                    // There's a special case when the end of the list has been reached.
                                    // Need to handle that with this bit of logic
                                    case true:
                                        var target = XRecyclerView.GetTargetVideoHolder();
                                        if (target != null)
                                        {
                                            XRecyclerView.PlayVideo(target, false);
                                        }

                                        // XRecyclerView.PlayVideo(!recyclerView.CanScrollVertically(1));
                                        //XRecyclerView
                                        break;
                                }

                                // elin Doughouz XRecyclerView?.ApiPostAsync?.FetchLoadMoreNewsFeedApiPosts().ConfigureAwait(false);

                                //if (Glide.With(XRecyclerView.Context).IsPaused)
                                //    Glide.With(XRecyclerView.Context).ResumeRequests();

                                break;

                            }
                    }
                    Trace.EndSection();
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

            public override void OnScrolled(RecyclerView recyclerView, int dx, int dy)
            {
                try
                {
                    base.OnScrolled(recyclerView, dx, dy);

                    if (IsLoading)
                        return;

                    var visibleItemCount = recyclerView.ChildCount;
                    var totalItemCount = recyclerView.GetAdapter().ItemCount;

                    LayoutManager ??= (PreCachingLayoutManager)recyclerView.GetLayoutManager();

                    int findVisibleItems = LayoutManager.FindLastVisibleItemPosition();

                    if (findVisibleItems + 35 < totalItemCount)
                        return;

                    LoadMoreEvent?.Invoke(this, null);
                    Trace.EndSection();
                }
                catch (Exception e)
                {
                    Console.WriteLine("API = Started OnScrolled IsLoading Failed");
                    Methods.DisplayReportResultTrack(e);
                }
            }
        }

        #region VideoObject player

        public void StopVideo()
        {
            try
            {

                //ResetMediaPlayer();

                CurrentVideoHolder?.OnPlayCanceled();

                //GC Collect
                // GC.Collect();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void ReleasePlayer()
        {
            try
            {
                StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

    }
}