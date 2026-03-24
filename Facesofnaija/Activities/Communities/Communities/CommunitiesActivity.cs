using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Communities.Adapters;
using Facesofnaija.Activities.Search;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient.Classes.Global;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.CustomApi.Classes.Community;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.Activities.Search.Adapters;
using Facesofnaija.Activities.Suggested.Adapters;
using Facesofnaija.CustomApi.Classes.Search;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CommunitiesActivity : BaseActivity
    {
        #region Variables Basic

        public SocialAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout, RandomCommunityViewStub;
        private View Inflated, RandomCommunityInflated;
        private TextView CreateButton;
        private AdView MAdView;
        private static CommunitiesActivity Instance;

        //private SuggestedCommunityAdapter MSAdapter;
        //private SearchCommunityAdapter RandomAdapter;

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
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(StartApiService);
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
                MAdView?.Resume();
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
                MAdView?.Pause();
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
                DestroyBasic();
                base.OnDestroy();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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
                MRecycler = (RecyclerView)FindViewById(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                RandomCommunityViewStub = FindViewById<ViewStub>(Resource.Id.viewStubRandomGroup);

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;

                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#282828") : Color.White);
                MRecycler.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#282828") : Color.White);

                CreateButton = FindViewById<TextView>(Resource.Id.toolbar_title);
                CreateButton.Text = GetString(Resource.String.Lbl_Request_Community);
                CreateButton.Visibility = ViewStates.Visible;
                 
                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Communities);
                    toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
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
                MAdapter = new SocialAdapter(this);
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(this);

                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<SocialModelsClass>(this, MAdapter, sizeProvider, 8);
                MRecycler.AddOnScrollListener(preLoader);

                //RandomAdapter = new SearchCommunityAdapter(this) { CommunityList = new ObservableCollection<CommunityDataObject>() };
                //RandomAdapter.ItemClick += RandomAdapterOnItemClick;
                //RandomAdapter.JoinButtonItemClick += MAdapterRandomOnJoinButtonItemClick;

                //RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                //MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                //MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                //MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                //MainScrollEvent.IsLoading = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void MAdapterOnItemClick(object sender, SocialAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item?.TypeView == SocialModelType.JoinedCommunities)
                    {
                        MainApplication.GetInstance()?.NavigateTo(this, typeof(CommunityProfileActivity), item.Community);
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile Random Community
        /*private void RandomAdapterOnItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(CommunityProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }*/

        /*private async void MAdapterRandomOnJoinButtonItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
        {
            try
            {

                var item = RandomAdapter.GetItem(e.Position);
                switch (item)
                {
                    case null:
                        return;
                }

                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var (apiStatus, respond) = await CustomRequests.Community.JoinCommunityAsync(item.CommunityId);
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case JoinCommunityObject result when result.JoinStatus == "requested":
                                    e.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                    e.Button.Text = Application.Context.GetText(Resource.String.Lbl_Request);
                                    e.Button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                    break;
                                case JoinCommunityObject result:
                                    {
                                        var isJoined = result.JoinStatus == "left" ? "false" : "true";
                                        e.Button.Text = GetText(isJoined == "yes" || isJoined == "true" ? Resource.String.Btn_Joined : Resource.String.Btn_Join_Community);

                                        switch (isJoined)
                                        {
                                            case "yes":
                                            case "true":
                                                e.Button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                                e.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                                break;
                                            default:
                                                e.Button.SetBackgroundResource(Resource.Drawable.round_button_pressed);
                                                e.Button.SetTextColor(Color.White);
                                                break;
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, respond);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }*/

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        CreateButton.Click += CreateButtonOnClick;
                        SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                        break;
                    default:
                        CreateButton.Click -= CreateButtonOnClick;
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
                MAdView?.Destroy();

                MAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                //RandomCommunityInflated = null!;
                Instance = null!; 
                CreateButton = null!;
                MAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static CommunitiesActivity GetInstance()
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

        #endregion

        #region Events
         
        //Scroll
        //private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Code get last id where LoadMore >>
        //        var item = MAdapter.SocialList.LastOrDefault();
        //        if (item != null && !string.IsNullOrEmpty(item?.GroupData?.GroupId) && !MainScrollEvent.IsLoading)
        //            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => GetJoinedGroups(item?.GroupData?.GroupId), () => GetSuggestedGroup(item?.GroupData?.GroupId) });
        //    }
        //    catch (Exception exception)
        //    {
        //        Methods.DisplayReportResultTrack(exception);
        //    }
        //}

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.SocialList.Clear();
                MAdapter.NotifyDataSetChanged();

                //MainScrollEvent.IsLoading = false;

                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //Event Request Community
        private void CreateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(RequestCommunityActivity));
                StartActivityForResult(intent, 200);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Permissions && Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    case 200 when resultCode == Result.Ok:
                        {
                            string result = data?.GetStringExtra("communityItem") ?? "";
                            switch (string.IsNullOrEmpty(result))
                            {
                                case false:
                                    {
                                        var item = JsonConvert.DeserializeObject<CommunityDataObject>(result);
                                        switch (item)
                                        {
                                            case null:
                                                return;
                                        }

                                        if (MAdapter.SocialList.Count > 0)
                                        {
                                            var check = MAdapter.SocialList.FirstOrDefault(a => a.TypeView == SocialModelType.ManagedCommunities);
                                            if (check != null)
                                            {
                                                MAdapter.SocialList?.Insert(1, new SocialModelsClass
                                                {
                                                    Id = Convert.ToInt32(item.CommunityId),
                                                    Community = item,
                                                    TypeView = SocialModelType.ManagedCommunities
                                                });
                                                MAdapter?.NotifyDataSetChanged();
                                            }
                                            else
                                            {
                                                MAdapter.SocialList?.Insert(0, new SocialModelsClass
                                                {
                                                    Id = 0001111111,
                                                    TitleHead = GetString(Resource.String.Lbl_Manage_Groups),
                                                    TypeView = SocialModelType.Section
                                                });

                                                MAdapter.SocialList?.Insert(1, new SocialModelsClass
                                                {
                                                    Id = Convert.ToInt32(item.CommunityId),
                                                    Community = item,
                                                    TypeView = SocialModelType.MangedGroups
                                                });

                                                MAdapter.SocialList?.Insert(2, new SocialModelsClass
                                                {
                                                    TypeView = SocialModelType.Divider
                                                });

                                                MAdapter?.NotifyDataSetChanged();
                                            }
                                        }
                                        else
                                        {
                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                Id = 0001111111, //revisit
                                                TitleHead = GetString(Resource.String.Lbl_Manage_Groups),
                                                TypeView = SocialModelType.Section
                                            });

                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                Id = Convert.ToInt32(item.CommunityId),
                                                Community = item,
                                                TypeView = SocialModelType.MangedGroups
                                            });

                                            MAdapter.SocialList?.Add(new SocialModelsClass
                                            {
                                                TypeView = SocialModelType.Divider
                                            });

                                            MAdapter.NotifyDataSetChanged();
                                        }

                                        RunOnUiThread(ShowEmptyPage);
                                        break;
                                    }
                            }

                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Get Community

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetJoinedCommunities, GetRandomCommunities });//() => LoadRandomCommunities("0")
        }

        private async Task GetJoinedCommunities()
        {
            //switch (MainScrollEvent.IsLoading)
            //{
            //    case true:
            //        return;
            //}

            if (Methods.CheckConnectivity())
            {
                //MainScrollEvent.IsLoading = true;

                var (apiStatus, respond) = await CustomRequests.Community.GetJoinedCommunitiesAsync();
                if (apiStatus != 200 || respond is not ListCommunitiesObject result || result.Data == null)
                {
                    //MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        var checkList = MAdapter.SocialList.FirstOrDefault(q => q.TypeView == SocialModelType.JoinedCommunities);
                        if (checkList == null)
                        {
                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                Id = 0001111111,
                                TitleHead = GetString(Resource.String.Lbl_Joined_Communities),
                                TypeView = SocialModelType.Section
                            });

                            foreach (var item in from item in result.Data let check = MAdapter.SocialList.FirstOrDefault(a => a.Community?.CommunityId == item.CommunityId) where check == null select item)
                            {
                                item.IsCommunityJoined = 1;

                                item.IsJoined = new IsJoined
                                {
                                    Bool = true,
                                    String = "yes"
                                };

                                MAdapter.SocialList.Add(new SocialModelsClass
                                {
                                    Id = Convert.ToInt32(item.CommunityId),
                                    Community = item,
                                    TypeView = SocialModelType.JoinedCommunities,
                                });

                                switch (ListUtils.MyCommunityList.FirstOrDefault(a => a.CommunityId == item.CommunityId))
                                {
                                    case null:
                                        ListUtils.MyCommunityList.Add(item);
                                        break;
                                }
                            }

                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                TypeView = SocialModelType.Divider
                            });
                        }
                    }
                    else
                    {
                        switch (MAdapter.SocialList.Count)
                        {
                            case > 10 when !MRecycler.CanScrollVertically(1):
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreCommunity), ToastLength.Short);
                                break;
                        }
                    }
                }

                //MainScrollEvent.IsLoading = false;
                //GetRandomCommunities();
                //GetSuggestedCommunities();
            }
            else
            {
                RunOnUiThread(() =>
                {
                    Inflated ??= EmptyStateLayout.Inflate();
                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click += null!;
                            x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                            break;
                    }

                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                });
                //MainScrollEvent.IsLoading = false;
            }
        }

        /*private async Task LoadRandomCommunities(string offset)
        {
            if (Methods.CheckConnectivity())
            {
                var countList = RandomAdapter.CommunityList.Count;

                var dictionary = new Dictionary<string, string>
                {
                    {"limit", "30"},
                    {"community_offset", offset},
                    {"search_key", "a"},
                    {"user_id", UserDetails.UserId},
                };
                var (respondCode, respondString) = await CustomRequests.GetCutomSearchAsync(dictionary);
                switch (respondCode)
                {
                    case 200:
                        {
                            switch (respondString)
                            {
                                case GetCustomSearchObject result:
                                    {
                                        var respondList = result.Communities.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Communities let check = RandomAdapter.CommunityList.FirstOrDefault(a => a.CommunityId == item.CommunityId) where check == null select item)
                                                    {
                                                        RandomAdapter.CommunityList.Add(item);
                                                    }

                                                    RunOnUiThread(() => { RandomAdapter.NotifyItemRangeInserted(countList, RandomAdapter.CommunityList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                RandomAdapter.CommunityList = new ObservableCollection<CommunityDataObject>(result.Communities);

                                                RunOnUiThread(() =>
                                                {
                                                    RandomCommunityInflated = RandomCommunityInflated switch
                                                    {
                                                        null => RandomCommunityViewStub.Inflate(),
                                                        _ => RandomCommunityInflated
                                                    };

                                                    //RecyclerInflaterRandomCommunity = new TemplateRecyclerInflater();
                                                    //RecyclerInflaterRandomCommunity.InflateLayout<CommunityDataObject>(this, RandomCommunityInflated, RandomAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, GetString(Resource.String.Lbl_RandomGroups));
                                                });
                                                break;
                                            default:
                                                {
                                                    //if (RecyclerInflaterRandomCommunity?.Recyler != null && RandomAdapter.CommunityList.Count > 10 && !RecyclerInflaterRandomCommunity.Recyler.CanScrollVertically(1))
                                                        //ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreCommunity), ToastLength.Short);
                                                    break;
                                                }
                                        }

                                        break;
                                    }
                            }

                            break;
                        }
                    default:
                        Methods.DisplayReportResult(this, respondString);
                        break;
                }

                RunOnUiThread(() =>
                {
                    //Devider2.Visibility = ViewStates.Visible;
                    ShowEmptyPage();
                });
            }
            else
            {
                Inflated = EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                switch (x.EmptyStateButton.HasOnClickListeners)
                {
                    case false:
                        x.EmptyStateButton.Click += null!;
                        x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                        break;
                }

                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
        }*/
        private async Task GetRandomCommunities()
        {
            //switch (MainScrollEvent.IsLoading)
            //{
            //    case true:
            //        return;
            //}

            if (Methods.CheckConnectivity())
            {
                //MainScrollEvent.IsLoading = true;

                var (apiStatus, respond) = await CustomRequests.Community.GetRandomCommunitiesAsync();
                if (apiStatus != 200 || respond is not ListCommunitiesObject result || result.Data == null)
                {
                    //MainScrollEvent.IsLoading = false;
                    Methods.DisplayReportResult(this, respond);
                }
                else
                {
                    var respondList = result.Data.Count;
                    if (respondList > 0)
                    {
                        MAdapter.SocialList.Add(new SocialModelsClass
                        {
                            Id = 0001111111,
                            TitleHead = GetString(Resource.String.Lbl_Communities),
                            TypeView = SocialModelType.Section
                        });

                        foreach (var item in from item in result.Data let check = MAdapter.SocialList.FirstOrDefault(a => a.Community?.CommunityId == item.CommunityId) where check == null select item)
                        {
                            item.IsCommunityJoined = 0;

                            item.IsJoined = new IsJoined
                            {
                                Bool = false,
                                String = "no"
                            };

                            MAdapter.SocialList.Add(new SocialModelsClass
                            {
                                Id = Convert.ToInt32(item.CommunityId),
                                Community = item,
                                TypeView = SocialModelType.JoinedCommunities,
                            });

                            switch (ListUtils.MyCommunityList.FirstOrDefault(a => a.CommunityId == item.CommunityId))
                            {
                                case null:
                                    ListUtils.MyCommunityList.Add(item);
                                    break;
                            }
                        }

                        MAdapter.SocialList.Add(new SocialModelsClass
                        {
                            TypeView = SocialModelType.Divider
                        });
                    }
                    else
                    {
                        switch (MAdapter.SocialList.Count)
                        {
                            case > 10 when !MRecycler.CanScrollVertically(1):
                                ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreCommunity), ToastLength.Short);
                                break;
                        }
                    }
                }

                //MainScrollEvent.IsLoading = false;
                GetSuggestedCommunities();
            }
            else
            {
                RunOnUiThread(() =>
                {
                    Inflated ??= EmptyStateLayout.Inflate();
                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click += null!;
                            x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                            break;
                    }

                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                });
                //MainScrollEvent.IsLoading = false;
            }
        }

        private void GetSuggestedCommunities()
        {
            try
            {
                /*if (ListUtils.SuggestedCommunityList.Count == 0)
                {
                    MAdapter.SocialList.Add(new SocialModelsClass
                    {
                        Id = 000001010101,
                        SuggestedCommunityList = new List<CommunityDataObject>(ListUtils.SuggestedCommunityList),
                        TitleHead = GetString(Resource.String.Lbl_Discover),
                        TypeView = SocialModelType.SuggestedCommunities
                    }); 
                }*/

                RunOnUiThread(ShowEmptyPage);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                //MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter.SocialList.Count)
                {
                    case > 0:
                        MAdapter.NotifyDataSetChanged();

                        MRecycler.Visibility = ViewStates.Visible;
                        EmptyStateLayout.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoCommunity);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    x.EmptyStateButton.Click += SearchButtonOnClick;
                                    break;
                            }
                            EmptyStateLayout.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                //MainScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //Event Open Search And Get Group Random
        private void SearchButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(SearchTabbedActivity));
                intent.PutExtra("Key", "Random_Communities");
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Factory.StartNew(StartApiService);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
         
    }
}