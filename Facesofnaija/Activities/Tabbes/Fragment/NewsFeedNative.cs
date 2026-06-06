using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide;
using Bumptech.Glide.Load.Engine;
using Bumptech.Glide.Request;
using Google.Android.Material.FloatingActionButton;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.ShimmerUtils;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.MediaPlayers;
using Facesofnaija.MediaPlayers.Exo;
using Facesofnaija.SQLite;
using WoWonderClient;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;
using static Facesofnaija.Activities.NativePost.Extra.WRecyclerView;
using Exception = System.Exception;
using Uri = Android.Net.Uri;

namespace Facesofnaija.Activities.Tabbes.Fragment
{
    public class NewsFeedNative : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private FloatingActionButton PopupBubbleView;
        public WRecyclerView MainRecyclerView;
        public NativePostAdapter PostFeedAdapter;
        public SwipeRefreshLayout SwipeRefreshLayout;
        private TabbedMainActivity GlobalContext;

        private ViewStub ShimmerPageLayout;
        private View InflatedShimmer;
        private TemplateShimmerInflater ShimmerInflater;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                // Create your fragment here 
                GlobalContext = (TabbedMainActivity)Activity ?? TabbedMainActivity.GetInstance();
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
                View view = inflater.Inflate(Resource.Layout.TNewsFeedLayout, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                Log.Warn("FON_TIMELINE", "NewsFeedNative.OnViewCreated called");

                InitComponent(view);
                InitShimmer(view);
                SetRecyclerViewAdapters();

                Log.Warn("FON_TIMELINE", "NewsFeedNative calling LoadPost(true)");
                LoadPost(true);
                Log.Warn("FON_TIMELINE", "NewsFeedNative LoadPost done");

                _ = EnsureFeedTimeoutFallback();
                GlobalContext?.GetOneSignalNotification();
            }
            catch (Exception exception)
            {
                Log.Warn("FON_TIMELINE", $"ERROR in OnViewCreated: {exception.Message}");
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnStop()
        {
            try
            {
                base.OnStop();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnDestroy()
        {
            try
            {
                MainRecyclerView?.ReleasePlayer();

                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                MainRecyclerView = (WRecyclerView)view.FindViewById(Resource.Id.newsfeedRecyler);
                PopupBubbleView = (FloatingActionButton)view.FindViewById(Resource.Id.popup_bubble);

                SwipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                if (SwipeRefreshLayout != null)
                {
                    SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                    SwipeRefreshLayout.Refreshing = false;
                    SwipeRefreshLayout.Enabled = true;
                    SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitShimmer(View view)
        {
            try
            {
                ShimmerPageLayout = view.FindViewById<ViewStub>(Resource.Id.viewStubShimmer);
                InflatedShimmer ??= ShimmerPageLayout.Inflate();

                ShimmerInflater = new TemplateShimmerInflater();
                ShimmerInflater.InflateLayout(Activity, InflatedShimmer, ShimmerTemplateStyle.PostTemplate);

                ShimmerInflater.Hide();
                if (InflatedShimmer != null)
                    InflatedShimmer.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                PostFeedAdapter = new NativePostAdapter(GlobalContext, "", MainRecyclerView, NativeFeedType.Global);
                MainRecyclerView.SetXTemplateShimmer(ShimmerInflater);
                MainRecyclerView?.SetXAdapter(PostFeedAdapter, SwipeRefreshLayout);
                MainRecyclerView.ApiPostAsync = new ApiPostAsync(MainRecyclerView, PostFeedAdapter);
                MainRecyclerView.Visibility = ViewStates.Visible;

                switch (AppSettings.ShowNewPostOnNewsFeed)
                {
                    case true:
                        MainRecyclerView?.SetXPopupBubble(PopupBubbleView);
                        break;
                    default:
                        PopupBubbleView.Visibility = ViewStates.Gone;
                        break;
                }

                MainRecyclerView.MainScrollEvent = new RecyclerScrollListener(MainRecyclerView);
                MainRecyclerView.AddOnScrollListener(MainRecyclerView.MainScrollEvent);
                MainRecyclerView.MainScrollEvent.LoadMoreEvent += MainRecyclerView.MainScrollEvent_LoadMoreEvent;
                MainRecyclerView.MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Refresh

        //Refresh 
        public void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                PopupBubbleView.Visibility = ViewStates.Gone;

                ShimmerInflater?.Show();

                // Save existing story list before clearing so stories survive the refresh.
                var existingStoryList = PostFeedAdapter?.ListDiffer
                    ?.FirstOrDefault(a => a.TypeView == PostModelType.Story)
                    ?.StoryList?.Where(s => s.Type != "Your").ToList() ?? new List<StoryDataObject>();

                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                Task.Run(() =>
                {
                    try
                    {
                        MainRecyclerView?.StopVideo();
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                });

                var combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, Activity);

                combiner.AddPostBoxPostView("feed", -1);

                if (AppSettings.ShowStory)
                {
                    combiner.AddStoryPostView(existingStoryList);
                }

                //combiner.AddPostBoxPostView("feed", -1);

                combiner.AddGreetingAlertPostView();
                combiner.AddCommunitiesAlertPostView(() => Activity?.RunOnUiThread(() => PostFeedAdapter?.NotifyDataSetChanged()));
                combiner.AddAnnouncementAlertPostView(() => Activity?.RunOnUiThread(() => PostFeedAdapter?.NotifyDataSetChanged()));

                PostFeedAdapter?.NotifyDataSetChanged();
                MainRecyclerView.MainScrollEvent.IsLoading = false;

                if (AppSettings.ShowStory)
                    _ = Task.Run(LoadStory);

                Task.Factory.StartNew(() => StartApiService("0", "Refresh"));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Get Post Feed

        public void LoadPost(bool local)
        {
            try
            {
                Console.WriteLine($"DEBUG LoadPost: Starting with local={local}");
                
                var combiner = new FeedCombiner(null, PostFeedAdapter?.ListDiffer, Activity);

                //combiner.AddStoryPostView();
                combiner.AddPostBoxPostView("feed", -1);
                Console.WriteLine("DEBUG LoadPost: Added PostBoxPostView");

                SqLiteDatabase dbDatabase = new SqLiteDatabase();

                if (AppSettings.ShowStory)
                {
                    //var list = dbDatabase.GetDataStory();
                    combiner.AddStoryPostView(new List<StoryDataObject>());
                    Console.WriteLine("DEBUG LoadPost: Added StoryPostView");

                    _ = Task.Run(LoadStory);
                }

                combiner.AddCommunitiesAlertPostView(() => Activity?.RunOnUiThread(() => PostFeedAdapter?.NotifyDataSetChanged()));
                Console.WriteLine("DEBUG LoadPost: Added CommunitiesAlertPostView");

                combiner.AddAnnouncementAlertPostView(() => Activity?.RunOnUiThread(() => PostFeedAdapter?.NotifyDataSetChanged()));
                Console.WriteLine("DEBUG LoadPost: Added AnnouncementAlertPostView");

                combiner.AddGreetingAlertPostView();
                Console.WriteLine("DEBUG LoadPost: Added GreetingAlertPostView");

                var adapterCount = PostFeedAdapter?.ListDiffer?.Count ?? 0;
                Console.WriteLine($"DEBUG LoadPost: Current adapter count = {adapterCount}");

                switch (adapterCount)
                {
                    case <= 5:
                        Console.WriteLine("DEBUG LoadPost: Adapter count <= 5, calling StartApiService()");
                        Task.Factory.StartNew(() => StartApiService());
                        break;
                    default:
                        {
                            Console.WriteLine($"DEBUG LoadPost: Adapter count > 5, loading more");
                            var item = PostFeedAdapter?.ListDiffer?.LastOrDefault();

                            var lastItem = PostFeedAdapter.ListDiffer.IndexOf(item);

                            item = PostFeedAdapter?.ListDiffer[lastItem];

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
                                    item = PostFeedAdapter?.ListDiffer?.LastOrDefault(a => a.TypeView != PostModelType.Divider && a.TypeView != PostModelType.ViewProgress && a.TypeView != PostModelType.AdMob1 && a.TypeView != PostModelType.AdMob2 && a.TypeView != PostModelType.AdMob3 && a.TypeView != PostModelType.FbAdNative && a.TypeView != PostModelType.AdsPost && a.TypeView != PostModelType.SuggestedGroupsBox && a.TypeView != PostModelType.SuggestedUsersBox && a.TypeView != PostModelType.CommentSection && a.TypeView != PostModelType.AddCommentSection);
                                    offset = item?.PostData?.PostId ?? "0";
                                    Console.WriteLine($"DEBUG LoadPost: Using offset {offset}");
                                    break;
                                default:
                                    offset = item.PostData?.PostId ?? "0";
                                    Console.WriteLine($"DEBUG LoadPost: Default offset {offset}");
                                    break;
                            }

                            StartApiService(offset, "Insert");
                            break;
                        }
                }

                Console.WriteLine("DEBUG LoadPost: Completed successfully");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR in LoadPost: {e}");
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void StartApiService(string offset = "0", string typeRun = "Add")
        {
            Log.Warn("FON_TIMELINE", $"StartApiService: offset={offset} typeRun={typeRun}");

            if (!Methods.CheckConnectivity())
            {
                Log.Warn("FON_TIMELINE", "StartApiService: No connectivity");
                ToastUtils.ShowToast(Activity, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            else
            {
                Log.Warn("FON_TIMELINE", "StartApiService: Queuing FetchNewsFeedApiPosts");
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => MainRecyclerView.ApiPostAsync.FetchNewsFeedApiPosts(offset, typeRun) });
            }
        }

        #endregion

        #region Get Story

        public async Task LoadStory()
        {
            if (!AppSettings.ShowStory) return;

            if (Methods.CheckConnectivity())
            {
                var checkSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                Android.Util.Log.Warn("FON_TIMELINE", $"LoadStory: checkSection={(checkSection == null ? "NULL" : "found")} listCount={PostFeedAdapter?.ListDiffer?.Count ?? -1}");
                if (checkSection != null)
                {
                    checkSection.StoryList ??= new ObservableCollection<StoryDataObject>();

                    var (apiStatus, respond) = await StoryApiService.GetUserStoriesAsync();
                    Android.Util.Log.Warn("FON_TIMELINE", $"LoadStory: apiStatus={apiStatus} respond={respond?.GetType()?.Name ?? "null"}");
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserStoriesObject result)
                        {
                            Android.Util.Log.Warn("FON_STORY_FLOW", $"LoadStory got result StoriesCount={result.Stories?.Count ?? 0}");
                            if (result.Stories?.Count > 0)
                            {
                                var first = result.Stories[0];
                                Android.Util.Log.Warn("FON_STORY_FLOW", $"First story group: userId={first.UserId} avatar={(first.Avatar?.Length > 60 ? first.Avatar.Substring(0, 60) + "..." : first.Avatar)} storiesCount={first.Stories?.Count}");
                            }
                            await Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    var storiesToAdd = new System.Collections.Generic.List<StoryDataObject>();

                                    string NormalizeStoryUrl(string url)
                                    {
                                        if (string.IsNullOrWhiteSpace(url))
                                            return string.Empty;

                                        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                            return url;

                                        var baseUrl = InitializeWoWonder.WebsiteUrl?.Trim().TrimEnd('/');
                                        if (string.IsNullOrWhiteSpace(baseUrl))
                                            return url;

                                        return $"{baseUrl}/{url.TrimStart('/')}";
                                    }

                                    void PreloadImage(string mediaFile)
                                    {
                                        try
                                        {
                                            var ctx = Context ?? Activity?.ApplicationContext;
                                            if (ctx != null)
                                                Glide.With(ctx).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                        }
                                        catch (Exception) { /* Glide preload is best-effort */ }
                                    }

                                    foreach (var item in result.Stories)
                                    {
                                        if (string.IsNullOrWhiteSpace(item?.UserId)) continue;

                                        item.Avatar = NormalizeStoryUrl(item.Avatar);
                                        item.DurationsList = new List<long>();

                                        var innerStories = item.Stories ?? new List<StoryDataObject.Story>();
                                        var expandedStories = new List<StoryDataObject.Story>();
                                        foreach (var story in innerStories)
                                        {
                                            story.Thumbnail = NormalizeStoryUrl(story.Thumbnail);

                                            var hasMultipleImages = story.Images?.Count > 1;
                                            var hasMultipleVideos = story.Videos?.Count > 1;

                                            if (hasMultipleImages && story.Videos?.Count == 0)
                                            {
                                                var firstImg = NormalizeStoryUrl(story.Thumbnail ?? string.Empty);
                                                if (!string.IsNullOrWhiteSpace(firstImg))
                                                {
                                                    expandedStories.Add(new StoryDataObject.Story
                                                    {
                                                        Id = story.Id,
                                                        UserId = story.UserId,
                                                        Title = story.Title,
                                                        Description = story.Description,
                                                        Posted = story.Posted,
                                                        Expire = story.Expire,
                                                        Thumbnail = firstImg,
                                                        TypeView = "Image"
                                                    });
                                                }
                                                foreach (var img in story.Images)
                                                {
                                                    var imgUrl = NormalizeStoryUrl(img.Filename ?? string.Empty);
                                                    if (string.IsNullOrWhiteSpace(imgUrl)) continue;
                                                    expandedStories.Add(new StoryDataObject.Story
                                                    {
                                                        Id = story.Id,
                                                        UserId = story.UserId,
                                                        Title = story.Title,
                                                        Description = story.Description,
                                                        Posted = story.Posted,
                                                        Expire = story.Expire,
                                                        Thumbnail = imgUrl,
                                                        TypeView = "Image"
                                                    });
                                                }
                                            }
                                            else if (hasMultipleVideos)
                                            {
                                                foreach (var vid in story.Videos)
                                                {
                                                    var vidUrl = NormalizeStoryUrl(vid.Filename ?? string.Empty);
                                                    if (string.IsNullOrWhiteSpace(vidUrl)) continue;
                                                    expandedStories.Add(new StoryDataObject.Story
                                                    {
                                                        Id = story.Id,
                                                        UserId = story.UserId,
                                                        Title = story.Title,
                                                        Description = story.Description,
                                                        Posted = story.Posted,
                                                        Expire = story.Expire,
                                                        Thumbnail = story.Thumbnail,
                                                        Videos = new List<StoryDataObject.Video> { new StoryDataObject.Video { Filename = vidUrl } },
                                                        TypeView = "Video"
                                                    });
                                                }
                                            }
                                            else
                                            {
                                                if (story.Videos?.Count > 0)
                                                    story.Videos[0].Filename = NormalizeStoryUrl(story.Videos[0].Filename);
                                                expandedStories.Add(story);
                                            }
                                        }

                                        foreach (var story in expandedStories)
                                        {
                                            var thumbnail = story.Thumbnail ?? string.Empty;
                                            var mediaFile = !thumbnail.Contains("avatar") && story.Videos?.Count == 0 ? thumbnail : story.Videos?.FirstOrDefault()?.Filename ?? "";

                                            if (string.IsNullOrEmpty(mediaFile))
                                            {
                                                item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                                story.TypeView = "Image";
                                                continue;
                                            }

                                            var fileType = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                            if (fileType != "Video")
                                            {
                                                story.TypeView = "Image";
                                                PreloadImage(mediaFile);
                                                item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                            }
                                            else
                                            {
                                                story.TypeView = "Video";
                                                var fileName = mediaFile.Split('/').Last();
                                                mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                if (PlayerSettings.EnableOfflineMode)
                                                {
                                                    try { new PreCachingExoPlayerVideo(Context ?? Activity?.ApplicationContext).CacheVideosFiles(Uri.Parse(mediaFile)); }
                                                    catch (Exception) { }
                                                }

                                                item.DurationsList.Add(AppSettings.ShowFullVideo
                                                    ? Long.ParseLong(WoWonderTools.GetDuration(mediaFile))
                                                    : AppSettings.StoryVideoDuration * 1000);
                                            }
                                        }

                                        Android.Util.Log.Warn("FON_STORY_FLOW", $"Expanded userId={item.UserId} from={innerStories.Count} to={expandedStories.Count} images0={(innerStories.FirstOrDefault()?.Images?.Count ?? 0)}");
                                        item.Stories = expandedStories;

                                        storiesToAdd.Add(item);
                                    }

                                    Android.Util.Log.Warn("FON_TIMELINE", $"LoadStory: storiesToAdd={storiesToAdd.Count}");
                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            // Re-fetch the story section — checkSection may be stale if the feed
                                            // rebuilt its ListDiffer (NotifyDataSetChanged) while the API fetch ran.
                                            var freshSection = PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                            if (freshSection == null)
                                            {
                                                Android.Util.Log.Warn("FON_TIMELINE", "LoadStory: freshSection is NULL — aborting UI update");
                                                return;
                                            }

                                            freshSection.StoryList ??= new ObservableCollection<StoryDataObject>();

                                            bool added = false;
                                            foreach (var storyItem in storiesToAdd)
                                            {
                                                var existing = freshSection.StoryList.FirstOrDefault(s => s.UserId == storyItem.UserId);
                                                if (existing != null)
                                                {
                                                    if (existing.Stories?.Count != storyItem.Stories?.Count)
                                                    {
                                                        existing.Stories = storyItem.Stories;
                                                        existing.Avatar = storyItem.Avatar;
                                                        added = true;
                                                    }
                                                }
                                                else
                                                {
                                                    freshSection.StoryList.Add(storyItem);
                                                    added = true;
                                                }
                                            }

                                            if (!added) return;

                                            if (PostFeedAdapter?.HolderStory?.AboutMore != null)
                                                PostFeedAdapter.HolderStory.AboutMore.Visibility = freshSection.StoryList.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;

                                            var storyIdx = PostFeedAdapter.ListDiffer.IndexOf(freshSection);
                                            Android.Util.Log.Warn("FON_TIMELINE", $"LoadStory: NotifyItemChanged idx={storyIdx} storyListCount={freshSection.StoryList.Count}");
                                            PostFeedAdapter?.NotifyItemChanged(storyIdx);

                                            //if (checkSection.StoryList.Count > 0)
                                            //{
                                            //    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            //    dbDatabase.InsertOrUpdateStory(JsonConvert.SerializeObject(checkSection.StoryList));
                                            //}
                                            //else
                                            //{
                                            //    SqLiteDatabase dbDatabase = new SqLiteDatabase();
                                            //    dbDatabase.DeleteStory();
                                            //}
                                        }
                                        catch (Exception e)
                                        {
                                            Methods.DisplayReportResultTrack(e);
                                        }
                                    });
                                }
                                catch (Exception e)
                                {
                                    Methods.DisplayReportResultTrack(e);
                                }
                            }).ConfigureAwait(false);
                        }
                    }
                    else
                        Methods.DisplayReportResult(Activity, respond);
                }
            }
            else
            {
                ToastUtils.ShowToast(Activity, Activity.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }

        #endregion

        private async Task EnsureFeedTimeoutFallback()
        {
            try
            {
                await Task.Delay(15000).ConfigureAwait(false);

                Activity?.RunOnUiThread(() =>
                {
                    try
                    {
                        if (MainRecyclerView == null || PostFeedAdapter == null)
                            return;

                        if (SwipeRefreshLayout != null && SwipeRefreshLayout.Refreshing)
                            SwipeRefreshLayout.Refreshing = false;

                        MainRecyclerView.MainScrollEvent.IsLoading = false;
                        MainRecyclerView.Visibility = ViewStates.Visible;
                        ShimmerInflater?.Hide();
                        if (InflatedShimmer != null)
                            InflatedShimmer.Visibility = ViewStates.Gone;
                        PostFeedAdapter.SetLoaded();

                        var hasRealPost = PostFeedAdapter.ListDiffer?.Any(a => a?.PostData != null) == true;
                        if (!hasRealPost)
                            PostFeedAdapter.NotifyDataSetChanged();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                });
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}