using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Facesofnaija.Activities.Announcements.Adapters;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Announcements
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class AnnouncementsActivity : BaseActivity
    {
        private AnnouncementAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;
        public RecyclerViewOnScrollListener MainScrollEvent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.RecyclerDefaultLayout);
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                LoadAnnouncements();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            try
            {
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recycler);
                SwipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout);
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SwipeRefreshLayout.SetColorSchemeResources(Resource.Color.primary);
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
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
                var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (toolbar != null)
                {
                    toolbar.Title = GetText(Resource.String.Lbl_BreakingNews);
                    SetSupportActionBar(toolbar);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetDisplayShowTitleEnabled(true);
                    SupportActionBar.Title = GetText(Resource.String.Lbl_BreakingNews);
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
                MAdapter = new AnnouncementAdapter(this);
                MAdapter.ItemClick += OnItemClick;
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);

                MainScrollEvent = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent.LoadMoreEvent += OnLoadMore;
                MRecycler.AddOnScrollListener(MainScrollEvent);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadAnnouncements()
        {
            try
            {
                SwipeRefreshLayout.Refreshing = true;

                var token = UserDetails.AccessToken ?? string.Empty;
                var userId = UserDetails.UserId ?? string.Empty;

                if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(userId))
                {
                    Log.Warn("FON_ANNOUNCE", "No token/userId");
                    SwipeRefreshLayout.Refreshing = false;
                    return;
                }

                var baseUrl = "http://172.236.19.52";
                var url = $"{baseUrl}/api-v2.php?type=get_announcements&access_token={token}&server_key={InitializeWoWonder.ServerKey ?? ""}";

                using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(20) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                var response = await client.GetAsync(url);
                var json = await response.Content.ReadAsStringAsync();

                Log.Warn("FON_ANNOUNCE", $"Status={response.StatusCode} Body={json}");

                if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
                {
                    var result = JsonConvert.DeserializeObject<AnnouncementsRootObject>(json);
                    if (result?.ApiStatus == "200" && result.Announcements?.Count > 0)
                    {
                        if (MAdapter != null)
                        {
                            MAdapter.AnnouncementList.Clear();
                            foreach (var ann in result.Announcements)
                                MAdapter.AnnouncementList.Add(ann);
                            MAdapter.NotifyDataSetChanged();
                        }
                        else
                        {
                            Log.Warn("FON_ANNOUNCE", "MAdapter is null");
                        }

                        if (MRecycler != null)
                            MRecycler.Visibility = ViewStates.Visible;
                        try
                        {
                            if (Inflated == null && EmptyStateLayout != null)
                                Inflated = EmptyStateLayout.Inflate();
                        }
                        catch (Exception inflateEx)
                        {
                            Log.Warn("FON_ANNOUNCE", $"Inflate exception: {inflateEx.Message}");
                        }
                        if (Inflated != null)
                            Inflated.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        Log.Warn("FON_ANNOUNCE", $"ApiStatus={result?.ApiStatus} Count={result?.Announcements?.Count}");
                        ShowEmptyState();
                    }
                }
                else
                {
                    Log.Warn("FON_ANNOUNCE", $"HTTP {response.StatusCode}");
                    ShowEmptyState();
                }
            }
            catch (Exception e)
            {
                Log.Warn("FON_ANNOUNCE", $"Exception: {e.Message}");
                Log.Warn("FON_ANNOUNCE", $"StackTrace: {e.StackTrace}");
                Methods.DisplayReportResultTrack(e);
                if (e.InnerException != null)
                    Log.Warn("FON_ANNOUNCE", $"Inner: {e.InnerException.Message}");
            }
            finally
            {
                SwipeRefreshLayout.Refreshing = false;
            }
        }

        private void ShowEmptyState()
        {
            try
            {
                if (Inflated == null)
                    Inflated = EmptyStateLayout?.Inflate();
                MRecycler.Visibility = ViewStates.Gone;
                if (Inflated != null)
                    Inflated.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            LoadAnnouncements();
        }

        private void OnLoadMore(object sender, EventArgs e)
        {
            // No pagination needed - all announcements loaded at once
        }

        private void OnItemClick(object sender, AnnouncementAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position >= 0 && e.Position < MAdapter.AnnouncementList.Count)
                {
                    var item = MAdapter.AnnouncementList[e.Position];
                    var intent = new Intent(this, typeof(AnnouncementDetailsActivity));
                    intent.PutExtra("AnnouncementId", item.Id);
                    intent.PutExtra("AnnouncementText", item.Text);
                    intent.PutExtra("AnnouncementTextDecode", item.TextDecode);
                    intent.PutExtra("AnnouncementTime", item.Time);
                    intent.PutExtra("AnnouncementTimeText", item.TimeText);
                    StartActivity(intent);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
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
    }
}
