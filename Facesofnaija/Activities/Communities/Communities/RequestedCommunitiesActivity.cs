using System;
using System.Collections.ObjectModel;
using System.Linq;
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
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RequestedCommunitiesActivity : BaseActivity
    {
        private RequestedCommunityAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private View Inflated;

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

                Task.Run(LoadDataAsync);
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

                var createButton = FindViewById<TextView>(Resource.Id.toolbar_title);
                createButton.Visibility = ViewStates.Gone;
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
                    toolBar.Title = "Requested Communities";
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
                MAdapter = new RequestedCommunityAdapter(this);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.SetAdapter(MAdapter);
                MRecycler.HasFixedSize = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var (apiStatus, respond) = await CustomRequests.Community.GetRequestedCommunitiesAsync();
                if (apiStatus == 200 && respond is ListCommunityRequestsObject result && result.Data != null)
                {
                    RunOnUiThread(() =>
                    {
                        MAdapter.RequestList = new ObservableCollection<CommunityRequestDataObject>(result.Data.Where(x => x != null));
                        MAdapter.NotifyDataSetChanged();
                        SwipeRefreshLayout.Refreshing = false;

                        if (MAdapter.ItemCount > 0)
                        {
                            MRecycler.Visibility = ViewStates.Visible;
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
            try
            {
                Inflated ??= EmptyStateLayout.Inflate();
                var emptyState = new EmptyStateInflater();
                emptyState.InflateLayout(Inflated, type);
                if (!emptyState.EmptyStateButton.HasOnClickListeners)
                    emptyState.EmptyStateButton.Click += buttonHandler;
                EmptyStateLayout.Visibility = ViewStates.Visible;
                MRecycler.Visibility = ViewStates.Gone;
                SwipeRefreshLayout.Refreshing = false;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            Task.Run(LoadDataAsync);
        }
    }

    public class RequestedCommunityAdapter : RecyclerView.Adapter
    {
        public ObservableCollection<CommunityRequestDataObject> RequestList = new ObservableCollection<CommunityRequestDataObject>();
        private readonly Activity Context;

        public RequestedCommunityAdapter(Activity context)
        {
            Context = context;
            HasStableIds = true;
        }

        public override int ItemCount => RequestList?.Count ?? 0;

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var itemView = LayoutInflater.From(parent.Context)?.Inflate(Resource.Layout.Style_RequestedCommunityView, parent, false);
            return new RequestedCommunityViewHolder(itemView);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            try
            {
                if (viewHolder is RequestedCommunityViewHolder holder)
                {
                    var item = RequestList[position];
                    if (item == null) return;

                    holder.NameText.Text = !string.IsNullOrEmpty(item.CommunityTitle) ? item.CommunityTitle : item.CommunityName;
                    holder.DescriptionText.Text = item.About ?? string.Empty;
                    holder.CategoryText.Text = string.IsNullOrEmpty(item.Category) ? "Other" : item.Category;

                    var status = item.RequestStatus ?? "pending";
                    switch (status.ToLower())
                    {
                        case "approved":
                            holder.StatusText.Text = "Approved";
                            holder.StatusText.SetTextColor(Color.ParseColor("#4CAF50"));
                            break;
                        case "rejected":
                            holder.StatusText.Text = "Rejected";
                            holder.StatusText.SetTextColor(Color.ParseColor("#F44336"));
                            break;
                        default:
                            holder.StatusText.Text = "Pending";
                            holder.StatusText.SetTextColor(Color.ParseColor("#FF9800"));
                            break;
                    }

                    if (!string.IsNullOrEmpty(item.Time))
                    {
                        if (long.TryParse(item.Time, out var unixTime))
                        {
                            var date = DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
                            holder.DateText.Text = date.ToString("MMM dd, yyyy");
                        }
                        else
                        {
                            holder.DateText.Text = item.Time;
                        }
                    }
                    else
                    {
                        holder.DateText.Text = string.Empty;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }

    public class RequestedCommunityViewHolder : RecyclerView.ViewHolder
    {
        public TextView NameText { get; }
        public TextView DescriptionText { get; }
        public TextView CategoryText { get; }
        public TextView StatusText { get; }
        public TextView DateText { get; }

        public RequestedCommunityViewHolder(View itemView) : base(itemView)
        {
            NameText = itemView.FindViewById<TextView>(Resource.Id.request_name);
            DescriptionText = itemView.FindViewById<TextView>(Resource.Id.request_description);
            CategoryText = itemView.FindViewById<TextView>(Resource.Id.request_category);
            StatusText = itemView.FindViewById<TextView>(Resource.Id.request_status);
            DateText = itemView.FindViewById<TextView>(Resource.Id.request_date);
        }
    }
}
