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

                PostFeedAdapter?.ListDiffer?.Clear();
                PostFeedAdapter?.NotifyDataSetChanged();

                PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.Clear();
                PostFeedAdapter?.HolderStory?.StoryAdapter?.NotifyDataSetChanged();

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
                    combiner.AddStoryPostView(new List<StoryDataObject>());
                }

                //combiner.AddPostBoxPostView("feed", -1);

                combiner.AddGreetingAlertPostView();
                combiner.AddCommunitiesAlertPostView();
                combiner.AddAnnouncementAlertPostView();

                PostFeedAdapter?.NotifyDataSetChanged();
                MainRecyclerView.MainScrollEvent.IsLoading = false;

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
                }

                combiner.AddCommunitiesAlertPostView();
                Console.WriteLine("DEBUG LoadPost: Added CommunitiesAlertPostView");

                combiner.AddAnnouncementAlertPostView();
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
                if (checkSection != null)
                {
                    checkSection.StoryList ??= new ObservableCollection<StoryDataObject>();

                    var (apiStatus, respond) = await RequestsAsync.Story.GetUserStoriesAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is GetUserStoriesObject result)
                            await Task.Factory.StartNew(() =>
                            {
                                try
                                {
                                    bool add = false;
                                    foreach (var item in result.Stories)
                                    {
                                        var check = checkSection.StoryList.FirstOrDefault(a => a.UserId == item.UserId);
                                        if (check != null)
                                        {
                                            if (check.Stories.Count == item.Stories.Count)
                                                return;

                                            foreach (var item2 in item.Stories)
                                            {
                                                item.DurationsList ??= new List<long>();

                                                //image and video
                                                var mediaFile = !item2.Thumbnail.Contains("avatar") && item2.Videos?.Count == 0 ? item2.Thumbnail : item2.Videos?.FirstOrDefault()?.Filename ?? "";

                                                var type = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                if (type != "Video")
                                                {
                                                    Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                    item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                                    item2.TypeView = "Image";
                                                }
                                                else
                                                {
                                                    item2.TypeView = "Video";

                                                    var fileName = mediaFile.Split('/').Last();
                                                    mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                    if (PlayerSettings.EnableOfflineMode)
                                                        new PreCachingExoPlayerVideo(Context).CacheVideosFiles(Uri.Parse(mediaFile));

                                                    if (AppSettings.ShowFullVideo)
                                                    {
                                                        var duration = WoWonderTools.GetDuration(mediaFile);
                                                        item.DurationsList.Add(Long.ParseLong(duration));
                                                    }
                                                    else
                                                    {
                                                        item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                    }
                                                }
                                            }

                                            add = true;
                                            check.Stories = item.Stories;
                                        }
                                        else
                                        {
                                            foreach (var item1 in item.Stories)
                                            {
                                                item.DurationsList ??= new List<long>();

                                                //image and video
                                                string mediaFile = !item1.Thumbnail.Contains("avatar") && item1.Videos?.Count == 0 ? item1.Thumbnail : item1.Videos?.FirstOrDefault()?.Filename ?? "";

                                                var type1 = Methods.AttachmentFiles.Check_FileExtension(mediaFile);
                                                if (type1 != "Video")
                                                {
                                                    item1.TypeView = "Image";
                                                    Glide.With(Context).Load(mediaFile).Apply(new RequestOptions().SetDiskCacheStrategy(DiskCacheStrategy.All).CenterCrop()).Preload();
                                                    item.DurationsList.Add(AppSettings.StoryImageDuration * 1000);
                                                }
                                                else
                                                {
                                                    item1.TypeView = "Video";
                                                    var fileName = mediaFile.Split('/').Last();
                                                    mediaFile = WoWonderTools.GetFile(DateTime.Now.Day.ToString(), Methods.Path.FolderDiskStory, fileName, mediaFile);

                                                    if (PlayerSettings.EnableOfflineMode)
                                                        new PreCachingExoPlayerVideo(Context).CacheVideosFiles(Uri.Parse(mediaFile));

                                                    if (AppSettings.ShowFullVideo)
                                                    {
                                                        var duration = WoWonderTools.GetDuration(mediaFile);
                                                        item.DurationsList.Add(Long.ParseLong(duration));
                                                    }
                                                    else
                                                    {
                                                        item.DurationsList.Add(AppSettings.StoryVideoDuration * 1000);
                                                    }
                                                }
                                            }

                                            add = true;
                                            checkSection.StoryList.Add(item);
                                        }
                                    }

                                    Activity?.RunOnUiThread(() =>
                                    {
                                        try
                                        {
                                            if (!add)
                                                return;

                                            if (PostFeedAdapter?.HolderStory?.AboutMore != null)
                                                PostFeedAdapter.HolderStory.AboutMore.Visibility = checkSection.StoryList.Count > 4 ? ViewStates.Visible : ViewStates.Invisible;

                                            PostFeedAdapter?.NotifyItemChanged(PostFeedAdapter.ListDiffer.IndexOf(checkSection));

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