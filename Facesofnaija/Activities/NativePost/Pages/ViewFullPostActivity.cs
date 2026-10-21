using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Util;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Comment.Adapters;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Comments;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using WoWonderClient;
using Facesofnaija.CustomApi.Requests;
using Xamarin.Facebook.Ads;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.NativePost.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class ViewFullPostActivity : BaseActivity
    {
        #region Variables Basic

        private WRecyclerView MainRecyclerView;
        private NativePostAdapter NativeFeedAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private string PostId;
        private PostDataObject DataObject;
        private InterstitialAd InterstitialAd;

        private CommentAdapter MAdapter;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private RecyclerViewOnScrollListener MainScrollEvent;

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.ViewFullPostLayout);

                if (string.IsNullOrWhiteSpace(Current.AccessToken) && !string.IsNullOrWhiteSpace(UserDetails.AccessToken))
                    Current.AccessToken = UserDetails.AccessToken;

                PostId = Intent?.GetStringExtra("Id") ?? Intent?.GetStringExtra("PostId") ?? Intent?.GetStringExtra("post_id") ?? string.Empty;
                if (!string.IsNullOrEmpty(PostId))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(PostId, @"^(\d+)");
                    if (match.Success)
                        PostId = match.Groups[1].Value;
                }

                //Get Value And Set Toolbar 
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                var dataItem = Intent?.GetStringExtra("DataItem");
                if (!string.IsNullOrEmpty(dataItem))
                {
                    try
                    {
                        DataObject = Newtonsoft.Json.JsonConvert.DeserializeObject<PostDataObject>(dataItem);
                        if (DataObject != null)
                        {
                            DataObject.GetPostComments ??= new List<GetCommentObject>();
                            var combine = new FeedCombiner(DataObject, NativeFeedAdapter.ListDiffer, this);
                            combine.CombineDefaultPostSections();
                            NativeFeedAdapter.NotifyDataSetChanged();
                        }
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }
                }

                Task.Factory.StartNew(StartApiService);

                if (AppSettings.ShowFbInterstitialAds)
                    InterstitialAd = AdsFacebook.InitInterstitial(this);
                else
                    AdsColony.Ad_Interstitial(this);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                AddOrRemoveEvent(true);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnPause()
        {
            try
            {
                base.OnPause();
                AddOrRemoveEvent(false);
                MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnStop()
        {
            try
            {
                base.OnStop();
                MainRecyclerView?.StopVideo();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnTrimMemory(TrimMemory level)
        {
            try
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                base.OnTrimMemory(level);
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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        protected override void OnDestroy()
        {
            try
            {
                MainRecyclerView?.ReleasePlayer();
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Menu

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        #endregion

        #region Functions

        private void InitComponent()
        {
            try
            {
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recycler_view);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitToolbar()
        {
            try
            {
                var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolBar != null)
                {
                    toolBar.Title = GetText(Resource.String.Lbl_Post);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
                    icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SupportActionBar.SetHomeAsUpIndicator(icon);

                }
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
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.Recyler);
                NativeFeedAdapter = new NativePostAdapter(this, "", MainRecyclerView, NativeFeedType.Global);
                MainRecyclerView.SetLayoutManager(new LinearLayoutManager(this));
                MainRecyclerView.NestedScrollingEnabled = false;
                MainRecyclerView.SetXAdapter(NativeFeedAdapter, SwipeRefreshLayout);

                MAdapter = new CommentAdapter(this)
                {
                    CommentList = new ObservableCollection<CommentObjectExtra>()
                };
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.NestedScrollingEnabled = false;
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<CommentObjectExtra>(this, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        break;
                    default:
                        SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        private void DestroyBasic()
        {
            try
            {
                InterstitialAd?.Destroy();

                MainRecyclerView = null!;
                SwipeRefreshLayout = null!;
                NativeFeedAdapter = null!;
                InterstitialAd = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                NativeFeedAdapter.ListDiffer.Clear();
                NativeFeedAdapter.NotifyDataSetChanged();

                MainScrollEvent.IsLoading = false;

                MainRecyclerView.MainScrollEvent.IsLoading = false;
                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Scroll
        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.CommentList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.Id) && !MainScrollEvent.IsLoading)
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadDataComment(item.Id) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region LoadPostData

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
            {
                RunOnUiThread(() =>
                {
                    if (SwipeRefreshLayout != null) SwipeRefreshLayout.Refreshing = false;
                    ShowEmptyPage();
                });
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            else
            {
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { LoadPostDataAsync });
            }
        }

        private async Task LoadPostDataAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PostId))
                {
                    RunOnUiThread(ShowEmptyPage);
                    return;
                }

                if (string.IsNullOrWhiteSpace(Current.AccessToken) && !string.IsNullOrWhiteSpace(UserDetails.AccessToken))
                    Current.AccessToken = UserDetails.AccessToken;

                var (apiStatus, respond) = await RequestsAsync.Posts.GetPostDataAsync(PostId, "post_data");
                GetPostDataObject result = respond as GetPostDataObject;

                if (apiStatus != 200 || result?.PostData == null)
                {
                    var sessionId = !string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? UserDetails.AccessToken : Current.AccessToken;
                    Android.Util.Log.Info("WoFallback", $"ViewFullPost SDK loading failed, trying web API fallback apiStatus={apiStatus}");
                    (int fbStatus, dynamic fbRespond) = await CustomRequests.Posts.FetchPostDataWebApiAsync(sessionId, PostId, "post_data");
                    if (fbStatus == 200 && fbRespond is GetPostDataObject fbResult && fbResult.PostData != null)
                    {
                        apiStatus = 200;
                        respond = fbResult;
                        result = fbResult;
                    }
                }

                if (apiStatus == 200 && result?.PostData != null)
                {
                    DataObject = result.PostData;
                    DataObject.GetPostComments ??= new List<GetCommentObject>();

                    RunOnUiThread(() =>
                    {
                        try
                        {
                            NativeFeedAdapter.ListDiffer.Clear();
                            var combine = new FeedCombiner(DataObject, NativeFeedAdapter.ListDiffer, this);
                            combine.CombineDefaultPostSections();
                            NativeFeedAdapter.NotifyDataSetChanged();
                            ShowEmptyPage();
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    });

                    await LoadDataComment();
                }
                else
                {
                    Android.Util.Log.Warn("WoFallback", $"ViewFullPost failed to load post {PostId}, apiStatus={apiStatus}");
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            finally
            {
                RunOnUiThread(ShowEmptyPage);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = false;

                var emptyStateCheck = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                if (emptyStateCheck != null)
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    if (emptyStateChecker != null && NativeFeedAdapter.ListDiffer.Count > 1)
                        MainRecyclerView.RemoveByRowIndex(emptyStateChecker);
                }
                else
                {
                    var emptyStateChecker = NativeFeedAdapter.ListDiffer.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                    switch (emptyStateChecker)
                    {
                        case null:
                            {
                                var data = new AdapterModelsClass
                                {
                                    TypeView = PostModelType.EmptyState,
                                    Id = 744747447,
                                };
                                NativeFeedAdapter.ListDiffer.Add(data);
                                NativeFeedAdapter.NotifyItemInserted(NativeFeedAdapter.ListDiffer.IndexOf(data));
                                break;
                            }
                    }
                }
            }
            catch (Exception e)
            {
                if (SwipeRefreshLayout != null) SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Load Data Comment

        private async Task LoadDataComment(string offset = "0")
        {
            switch (MainScrollEvent.IsLoading)
            {
                case true:
                    return;
            }

            if (Methods.CheckConnectivity())
            {
                if (string.IsNullOrWhiteSpace(Current.AccessToken) && !string.IsNullOrWhiteSpace(UserDetails.AccessToken))
                    Current.AccessToken = UserDetails.AccessToken;

                MainScrollEvent.IsLoading = true;
                var sessionId = !string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? UserDetails.AccessToken : Current.AccessToken;
                var (apiStatus, respond) = await RequestsAsync.Comment.GetPostCommentsAsync(PostId, "10", offset);
                CommentObject result = respond as CommentObject;
                if (apiStatus != 200 || result?.CommentList == null)
                {
                    Log.Info("WoFallback", $"ViewFullPost SDK loading failed, trying web API fallback apiStatus={apiStatus}");
                    var (fbStatus, fbRespond) = await CustomRequests.Posts.FetchCommentsWebApiAsync(sessionId, PostId, "10", offset);
                    if (fbStatus == 200 && fbRespond is CommentObject fbResult && fbResult.CommentList != null)
                    {
                        respond = fbResult;
                        result = fbResult;
                        apiStatus = 200;
                    }
                }

                if (apiStatus != 200 || result?.CommentList == null)
                {
                    MainScrollEvent.IsLoading = false;
                    try { Methods.DisplayReportResult(this, respond); } catch (Exception) { }
                }
                else
                {
                    var commentList = result.CommentList;
                    if (commentList != null && commentList.Count > 0)
                    {
                        RunOnUiThread(() =>
                        {
                            try
                            {
                                foreach (var item in commentList)
                                {
                                    CommentObjectExtra check = MAdapter.CommentList.FirstOrDefault(a => a.Id == item.Id);
                                    if (check == null)
                                    {
                                        var db = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                        if (db != null) MAdapter.CommentList.Add(db);
                                    }
                                    else
                                    {
                                        check = ClassMapper.Mapper?.Map<CommentObjectExtra>(item);
                                        if (check != null)
                                        {
                                            check.Replies = item.Replies;
                                            check.RepliesCount = item.RepliesCount;
                                        }
                                    }
                                }

                                MAdapter.NotifyDataSetChanged();
                            }
                            catch (Exception ex)
                            {
                                Methods.DisplayReportResultTrack(ex);
                            }
                        });
                    }
                    MainScrollEvent.IsLoading = false;
                }

                RunOnUiThread(ShowEmptyPage);
            }
        }

        #endregion

    }
}