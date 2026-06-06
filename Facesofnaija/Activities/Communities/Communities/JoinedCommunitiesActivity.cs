using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Search.Adapters;
using Facesofnaija.CustomApi.Classes.Community;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class JoinedCommunitiesActivity : BaseActivity
    {
        private SearchCommunityAdapter Adapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView Recycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;

        private string FetchType = "joined_communities";
        private string PageTitle = "Joined Communities";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                FetchType = Intent?.GetStringExtra("fetch") ?? "joined_communities";
                PageTitle = Intent?.GetStringExtra("title") ?? "Joined Communities";

                InitComponent();
                InitToolbar();
                SetRecycler();

                Task.Run(LoadJoinedCommunitiesAsync);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            try
            {
                base.OnResume();
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                SwipeRefreshLayout.Refresh -= SwipeRefreshLayoutOnRefresh;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            Recycler = FindViewById<RecyclerView>(Resource.Id.recyler);
            EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);
            SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);

            SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
            SwipeRefreshLayout.Refreshing = true;
            SwipeRefreshLayout.Enabled = true;
            SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
            SwipeRefreshLayout.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#282828") : Color.White);
            Recycler.SetBackgroundColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#282828") : Color.White);

            var createButton = FindViewById<TextView>(Resource.Id.toolbar_title);
            createButton.Visibility = ViewStates.Gone;
        }

        private void InitToolbar()
        {
            var toolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
            if (toolBar == null)
                return;

            toolBar.Title = PageTitle;
            toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
            SetSupportActionBar(toolBar);
            SupportActionBar?.SetDisplayShowCustomEnabled(true);
            SupportActionBar?.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar?.SetHomeButtonEnabled(true);
            SupportActionBar?.SetDisplayShowHomeEnabled(true);

            var icon = AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.icon_back_arrow_right : Resource.Drawable.icon_back_arrow_left);
            icon?.SetTint(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
            SupportActionBar?.SetHomeAsUpIndicator(icon);
        }

        private void SetRecycler()
        {
            Adapter = new SearchCommunityAdapter(this) { CommunityList = new ObservableCollection<CommunityDataObject>() };
            Adapter.ItemClick += AdapterOnItemClick;
            Adapter.JoinButtonItemClick += AdapterOnJoinButtonItemClick;

            LayoutManager = new LinearLayoutManager(this);
            Recycler.SetLayoutManager(LayoutManager);
            Recycler.SetAdapter(Adapter);
            Recycler.HasFixedSize = true;
            Recycler.SetItemViewCacheSize(18);
            Recycler.GetLayoutManager().ItemPrefetchEnabled = true;
        }

        private async Task LoadJoinedCommunitiesAsync()
        {
            try
            {
                var (status, respond) = await CustomRequests.Community.GetCommunitiesByFetchAsync(FetchType);
                if (status == 200 && respond is ListCommunitiesObject result && result.Data != null)
                {
                    RunOnUiThread(() =>
                    {
                        Adapter.CommunityList = new ObservableCollection<CommunityDataObject>(result.Data);
                        Adapter.NotifyDataSetChanged();
                        SwipeRefreshLayout.Refreshing = false;

                        if (Adapter.ItemCount > 0)
                        {
                            Recycler.Visibility = ViewStates.Visible;
                            EmptyStateLayout.Visibility = ViewStates.Gone;
                        }
                        else
                        {
                            InflateEmptyState(EmptyStateInflater.Type.NoCommunity, EmptyStateButtonOnClick);
                        }
                    });
                }
                else
                {
                    RunOnUiThread(() =>
                    {
                        SwipeRefreshLayout.Refreshing = false;
                        InflateEmptyState(EmptyStateInflater.Type.NoCommunity, EmptyStateButtonOnClick);
                    });
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                RunOnUiThread(() =>
                {
                    SwipeRefreshLayout.Refreshing = false;
                    InflateEmptyState(EmptyStateInflater.Type.NoCommunity, EmptyStateButtonOnClick);
                });
            }
        }

        private void InflateEmptyState(EmptyStateInflater.Type type, EventHandler buttonHandler)
        {
            Inflated ??= EmptyStateLayout.Inflate();
            var emptyState = new EmptyStateInflater();
            emptyState.InflateLayout(Inflated, type);

            if (!emptyState.EmptyStateButton.HasOnClickListeners)
                emptyState.EmptyStateButton.Click += buttonHandler;

            EmptyStateLayout.Visibility = ViewStates.Visible;
            Recycler.Visibility = ViewStates.Gone;
            SwipeRefreshLayout.Refreshing = false;
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            Task.Run(LoadJoinedCommunitiesAsync);
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            Task.Run(LoadJoinedCommunitiesAsync);
        }

        private void AdapterOnItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position < 0 || e.Position >= Adapter.CommunityList.Count)
                    return;

                var item = Adapter.GetItem(e.Position);
                if (item != null)
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(CommunityProfileActivity), item);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void AdapterOnJoinButtonItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position < 0 || e.Position >= Adapter.CommunityList.Count)
                    return;

                var item = Adapter.GetItem(e.Position);
                if (item == null)
                    return;

                var (apiStatus, respond) = await CustomRequests.Community.JoinCommunityAsync(item.CommunityId);
                if (apiStatus == 200 && respond is JoinCommunityObject result && result.JoinStatus == "left")
                {
                    Adapter.CommunityList.Remove(item);
                    Adapter.NotifyDataSetChanged();
                    if (Adapter.ItemCount == 0)
                        InflateEmptyState(EmptyStateInflater.Type.NoCommunity, EmptyStateButtonOnClick);
                }
                else
                {
                    Methods.DisplayReportResult(this, respond);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}
