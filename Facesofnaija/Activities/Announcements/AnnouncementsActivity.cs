using Android.App;
using static Facesofnaija.AppSettings;
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
using Facesofnaija.Helpers.Fonts;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using WoWonderClient;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using AndroidX.AppCompat.Widget;
using Com.Airbnb.Lottie;

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
                MRecycler = FindViewById<RecyclerView>(Resource.Id.recyler);
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
                var baseUrl = AppSettings.SiteUrl?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = "https://www.facesofnaija.ng";

                var url = $"{baseUrl}/api-v2.php?type=get_announcements&access_token={token}&server_key={InitializeWoWonder.ServerKey ?? ""}";

                bool loadedFromServer = false;
                try
                {
                    using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(15) };
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

                            loadedFromServer = true;
                        }
                    }
                }
                catch (Exception netEx)
                {
                    Log.Warn("FON_ANNOUNCE", $"Network fetch warning: {netEx.Message}");
                }

                // Fallback to local / direct RSS repository if server call returned no announcements
                if (!loadedFromServer)
                {
                    var headlines = await Facesofnaija.Helpers.News.RssNewsRepository.GetHeadlinesAsync();
                    if (headlines != null && headlines.Count > 0)
                    {
                        if (MAdapter != null)
                        {
                            MAdapter.AnnouncementList.Clear();
                            foreach (var h in headlines)
                            {
                                MAdapter.AnnouncementList.Add(new AnnouncementDataObject
                                {
                                    Id = h.Id?.ToString() ?? "rss",
                                    Text = $"[{h.Source}] {h.Title}",
                                    TextDecode = h.Title,
                                    Source = h.Source,
                                    Url = h.Link,
                                    Link = h.Link,
                                    Time = h.Time.ToString(),
                                    TimeText = !string.IsNullOrWhiteSpace(h.TimeText) ? h.TimeText : "Recent",
                                    Active = "1"
                                });
                            }
                            MAdapter.NotifyDataSetChanged();
                        }

                        if (MRecycler != null)
                            MRecycler.Visibility = ViewStates.Visible;
                        if (Inflated != null)
                            Inflated.Visibility = ViewStates.Gone;
                    }
                    else
                    {
                        ShowEmptyState(false);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Warn("FON_ANNOUNCE", $"Exception: {e.Message}");
                Methods.DisplayReportResultTrack(e);
                ShowEmptyState(true);
            }
            finally
            {
                SwipeRefreshLayout.Refreshing = false;
            }
        }

        private void ShowEmptyState(bool isError = false)
        {
            try
            {
                if (Inflated == null && EmptyStateLayout != null)
                    Inflated = EmptyStateLayout.Inflate();

                if (Inflated != null)
                {
                    var headText = Inflated.FindViewById<TextView>(Resource.Id.headText);
                    var secondaryText = Inflated.FindViewById<TextView>(Resource.Id.seconderyText);
                    var button = Inflated.FindViewById<AppCompatButton>(Resource.Id.button);
                    var animationView = Inflated.FindViewById<LottieAnimationView>(Resource.Id.animation_view);
                    var emptyIcon = Inflated.FindViewById<TextView>(Resource.Id.emtyicon);
                    var emptyImage = Inflated.FindViewById<ImageView>(Resource.Id.iv_empty);

                    if (isError)
                    {
                        var infl = new EmptyStateInflater();
                        infl.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                        if (infl.EmptyStateButton != null)
                        {
                            infl.EmptyStateButton.Visibility = ViewStates.Visible;
                            if (!infl.EmptyStateButton.HasOnClickListeners)
                            {
                                infl.EmptyStateButton.Click += (s, e) => LoadAnnouncements();
                            }
                        }
                    }
                    else
                    {
                        if (animationView != null) animationView.Visibility = ViewStates.Gone;
                        if (emptyImage != null) emptyImage.Visibility = ViewStates.Gone;
                        if (emptyIcon != null)
                        {
                            FontUtils.SetTextViewIcon(FontsIconFrameWork.FontAwesomeLight, emptyIcon, FontAwesomeIcon.Newspaper);
                            emptyIcon.Visibility = ViewStates.Visible;
                        }

                        if (headText != null)
                            headText.Text = GetText(Resource.String.Lbl_BreakingNews);
                        if (secondaryText != null)
                            secondaryText.Text = "There are no breaking news announcements at this time.";
                        if (button != null)
                            button.Visibility = ViewStates.Gone;
                    }

                    Inflated.Visibility = ViewStates.Visible;
                }

                if (MRecycler != null)
                    MRecycler.Visibility = ViewStates.Gone;
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
                    var targetUrl = !string.IsNullOrWhiteSpace(item.Url) ? item.Url : item.Link;

                    if (!string.IsNullOrWhiteSpace(targetUrl) && (targetUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || targetUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)))
                    {
                        new IntentController(this).OpenBrowserFromApp(targetUrl);
                        return;
                    }

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
