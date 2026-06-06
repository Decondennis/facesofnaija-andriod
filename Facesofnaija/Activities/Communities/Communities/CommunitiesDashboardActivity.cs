using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Communities.Adapters;
using Facesofnaija.Activities.Search;
using Facesofnaija.Activities.Suggested.Groups;
using Facesofnaija.CustomApi.Classes.Community;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CommunitiesDashboardActivity : BaseActivity
    {
        private static CommunitiesDashboardActivity Instance;
        private CommunitySectionAdapter DashboardAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        private TextView CreateButton;
        private AdView MAdView;

        private ObservableCollection<CommunityDataObject> JoinedCommunityList = new ObservableCollection<CommunityDataObject>();
        private ObservableCollection<CommunityDataObject> RequestedCommunityList = new ObservableCollection<CommunityDataObject>();
        private ObservableCollection<CommunityDataObject> SuggestedCommunityList = new ObservableCollection<CommunityDataObject>();
        private ObservableCollection<CommunityDataObject> AllCommunityList = new ObservableCollection<CommunityDataObject>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                Instance = this;

                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                Task.Run(LoadDashboardAsync);
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

        private void InitComponent()
        {
            try
            {
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);

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
                if (toolBar == null)
                    return;

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
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SetRecyclerViewAdapters()
        {
            try
            {
                DashboardAdapter = new CommunitySectionAdapter(this);
                DashboardAdapter.ItemClick += DashboardAdapterOnItemClick;
                DashboardAdapter.ActionClick += DashboardAdapterOnActionClick;
                DashboardAdapter.MoreClick += DashboardAdapterOnMoreClick;

                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(DashboardAdapter);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(18);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadDashboardAsync()
        {
            try
            {
                Console.WriteLine("FON_COMM LoadDashboardAsync: starting 3 API calls");
                var joinedTask = CustomRequests.Community.GetJoinedCommunitiesAsync();
                var allTask = CustomRequests.Community.GetAllCommunitiesAsync();
                var suggestedTask = CustomRequests.Community.GetSuggestedCommunitiesAsync();

                await Task.WhenAll(joinedTask, allTask, suggestedTask);
                Console.WriteLine("FON_COMM LoadDashboardAsync: all 3 API calls done");

                var joinedResult = joinedTask.Result;
                var allResult = allTask.Result;
                var suggestedResult = suggestedTask.Result;

                Console.WriteLine("FON_COMM LoadDashboardAsync: joined status=" + joinedResult.Item1 + " type=" + joinedResult.Item2?.GetType()?.Name);
                Console.WriteLine("FON_COMM LoadDashboardAsync: all status=" + allResult.Item1 + " type=" + allResult.Item2?.GetType()?.Name);
                Console.WriteLine("FON_COMM LoadDashboardAsync: suggested status=" + suggestedResult.Item1 + " type=" + suggestedResult.Item2?.GetType()?.Name);

                JoinedCommunityList = ExtractCommunityList(joinedResult);
                AllCommunityList = ExtractCommunityList(allResult);
                SuggestedCommunityList = ExtractCommunityList(suggestedResult);

                Console.WriteLine("FON_COMM LoadDashboardAsync: joined count=" + JoinedCommunityList?.Count);
                Console.WriteLine("FON_COMM LoadDashboardAsync: all count=" + AllCommunityList?.Count);
                Console.WriteLine("FON_COMM LoadDashboardAsync: suggested count=" + SuggestedCommunityList?.Count);

                RunOnUiThread(BuildDashboardSections);
                Console.WriteLine("FON_COMM LoadDashboardAsync: BuildDashboardSections queued");
            }
            catch (Exception exception)
            {
                Console.WriteLine("FON_COMM LoadDashboardAsync EXCEPTION: " + exception.Message);
                Methods.DisplayReportResultTrack(exception);
                RunOnUiThread(() => InflateEmptyState(EmptyStateInflater.Type.NoCommunity, SearchButtonOnClick));
            }
        }

        private ObservableCollection<CommunityDataObject> ExtractCommunityList((long?, dynamic) apiResult)
        {
            if (apiResult.Item1 != 200 || apiResult.Item2 is not ListCommunitiesObject result || result.Data == null)
                return new ObservableCollection<CommunityDataObject>();

            return new ObservableCollection<CommunityDataObject>(result.Data.Where(item => item != null));
        }

        private void BuildDashboardSections()
        {
            try
            {
                Console.WriteLine("FON_COMM BuildDashboardSections: starting, sections count=" + DashboardAdapter.Sections?.Count);

                DashboardAdapter.Sections = new ObservableCollection<CommunityDashboardSectionModel>
                {
                    new CommunityDashboardSectionModel
                    {
                        Title = "Joined Communities",
                        Type = CommunityDashboardSectionType.Joined,
                        Communities = JoinedCommunityList
                    },
                    new CommunityDashboardSectionModel
                    {
                        Title = "All Communities",
                        Type = CommunityDashboardSectionType.Suggested,
                        Communities = AllCommunityList
                    },
                    new CommunityDashboardSectionModel
                    {
                        Title = "Suggested Communities",
                        Type = CommunityDashboardSectionType.Suggested,
                        Communities = SuggestedCommunityList
                    },
                    new CommunityDashboardSectionModel
                    {
                        Title = "Requested Communities",
                        Type = CommunityDashboardSectionType.Requested,
                        Communities = new ObservableCollection<CommunityDataObject>()
                    }
                };

                Console.WriteLine("FON_COMM BuildDashboardSections: sections set, count=" + DashboardAdapter.Sections?.Count);

                DashboardAdapter.NotifyDataSetChanged();
                Console.WriteLine("FON_COMM BuildDashboardSections: NotifyDataSetChanged done");

                SwipeRefreshLayout.Refreshing = false;
                Console.WriteLine("FON_COMM BuildDashboardSections: SwipeRefresh false");

                if (DashboardAdapter.ItemCount > 0)
                {
                    MRecycler.Visibility = ViewStates.Visible;
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                    Console.WriteLine("FON_COMM BuildDashboardSections: showing recycler");
                }
                else
                {
                    InflateEmptyState(EmptyStateInflater.Type.NoCommunity, SearchButtonOnClick);
                    Console.WriteLine("FON_COMM BuildDashboardSections: empty state");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("FON_COMM BuildDashboardSections EXCEPTION: " + e.Message);
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InflateEmptyState(EmptyStateInflater.Type type, EventHandler buttonHandler)
        {
            try
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var emptyState = new EmptyStateInflater();
                emptyState.InflateLayout(Inflated, type);

                if (!emptyState.EmptyStateButton.HasOnClickListeners)
                {
                    emptyState.EmptyStateButton.Click += buttonHandler;
                }

                EmptyStateLayout.Visibility = ViewStates.Visible;
                MRecycler.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.Refreshing = false;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DashboardAdapterOnItemClick(object sender, CommunityItemAdapterClickEventArgs e)
        {
            try
            {
                if (e.Item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(CommunityProfileActivity), e.Item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void DashboardAdapterOnActionClick(object sender, CommunityItemAdapterClickEventArgs e)
        {
            try
            {
                if (e.Item == null)
                    return;

                var previousState = WoWonderTools.IsJoinedCommunity(e.Item);
                ApplyJoinState(e.Item, GetNextState(e.Item), e.Button);
                e.Button.Enabled = false;

                var (apiStatus, respond) = await CustomRequests.Community.JoinCommunityAsync(e.Item.CommunityId);
                if (apiStatus == 200 && respond is JoinCommunityObject result)
                {
                    ApplyJoinStateFromResult(e.Item, result.JoinStatus, e.Button);
                    RefreshDashboardFromState();
                }
                else
                {
                    ApplyJoinState(e.Item, previousState, e.Button);
                    Methods.DisplayReportResult(this, respond);
                }

                e.Button.Enabled = true;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void DashboardAdapterOnMoreClick(object sender, CommunitySectionAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position < 0 || e.Position >= DashboardAdapter.Sections.Count)
                    return;

                var section = DashboardAdapter.Sections[e.Position];
                if (section == null)
                    return;

                switch (section.Type)
                {
                    case CommunityDashboardSectionType.Suggested:
                        var allIntent = new Intent(this, typeof(JoinedCommunitiesActivity));
                        allIntent.PutExtra("fetch", "random_communities");
                        allIntent.PutExtra("title", section.Title);
                        StartActivity(allIntent);
                        break;
                    case CommunityDashboardSectionType.Joined:
                        StartActivity(new Intent(this, typeof(JoinedCommunitiesActivity)));
                        break;
                    case CommunityDashboardSectionType.Requested:
                        var reqIntent = new Intent(this, typeof(RequestedCommunitiesActivity));
                        StartActivity(reqIntent);
                        break;
                    default:
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_something_went_wrong), ToastLength.Short);
                        break;
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string GetNextState(CommunityDataObject item)
        {
            var state = WoWonderTools.IsJoinedCommunity(item);
            if (state == "1" || state == "2")
                return "0";

            return item.JoinPrivacy == "2" || item.Privacy == "2" ? "2" : "1";
        }

        private void ApplyJoinStateFromResult(CommunityDataObject item, string joinStatus, AppCompatButton button)
        {
            var state = joinStatus switch
            {
                "joined" => "1",
                "requested" => "2",
                _ => "0"
            };

            ApplyJoinState(item, state, button);
        }

        private void ApplyJoinState(CommunityDataObject item, string state, AppCompatButton button)
        {
            item.IsCommunityJoined = state switch
            {
                "1" => 1,
                "2" => 2,
                _ => 0
            };

            item.IsJoined = new IsJoined
            {
                Bool = state == "1",
                String = state switch
                {
                    "1" => "yes",
                    "2" => "requested",
                    _ => "no"
                }
            };

            if (button == null)
                return;

            switch (state)
            {
                case "1":
                    button.Text = GetText(Resource.String.Btn_Joined);
                    button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                    break;
                case "2":
                    button.Text = GetText(Resource.String.Lbl_Request);
                    button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends);
                    button.SetTextColor(Color.ParseColor("#444444"));
                    break;
                default:
                    button.Text = GetText(Resource.String.Btn_Join_Community);
                    button.SetBackgroundResource(Resource.Drawable.follow_button_profile_friends_pressed);
                    button.SetTextColor(Color.White);
                    break;
            }
        }

        private void RefreshDashboardFromState()
        {
            RunOnUiThread(BuildDashboardSections);
        }

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                if (addEvent)
                {
                    CreateButton.Click += CreateButtonOnClick;
                    SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
                }
                else
                {
                    CreateButton.Click -= CreateButtonOnClick;
                    SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
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
                DashboardAdapter = null!;
                SwipeRefreshLayout = null!;
                MRecycler = null!;
                EmptyStateLayout = null!;
                Inflated = null!;
                Instance = null!;
                CreateButton = null!;
                MAdView = null!;
                JoinedCommunityList = null!;
                RequestedCommunityList = null!;
                SuggestedCommunityList = null!;
                AllCommunityList = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public static CommunitiesDashboardActivity GetInstance()
        {
            return Instance;
        }

        public void RemoveCommunityFromLists(string communityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(communityId))
                    return;

                JoinedCommunityList = new ObservableCollection<CommunityDataObject>(JoinedCommunityList.Where(item => item?.CommunityId != communityId));
                RequestedCommunityList = new ObservableCollection<CommunityDataObject>(RequestedCommunityList.Where(item => item?.CommunityId != communityId));
                SuggestedCommunityList = new ObservableCollection<CommunityDataObject>(SuggestedCommunityList.Where(item => item?.CommunityId != communityId));

                RunOnUiThread(BuildDashboardSections);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void UpdateCommunityAvatar(string communityId, string avatarPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(communityId) || string.IsNullOrWhiteSpace(avatarPath))
                    return;

                foreach (var item in JoinedCommunityList.Where(item => item?.CommunityId == communityId))
                {
                    item.Avatar = avatarPath;
                }

                foreach (var item in RequestedCommunityList.Where(item => item?.CommunityId == communityId))
                {
                    item.Avatar = avatarPath;
                }

                foreach (var item in SuggestedCommunityList.Where(item => item?.CommunityId == communityId))
                {
                    item.Avatar = avatarPath;
                }

                RunOnUiThread(BuildDashboardSections);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                Task.Run(LoadDashboardAsync);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

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

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                Task.Run(LoadDashboardAsync);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 200 && resultCode == Result.Ok)
                {
                    Task.Run(LoadDashboardAsync);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}