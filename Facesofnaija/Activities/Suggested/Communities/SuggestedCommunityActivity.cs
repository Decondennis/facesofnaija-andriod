using System;
using System.Collections.Generic;
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
using AndroidX.SwipeRefreshLayout.Widget;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Search.Adapters;
using Facesofnaija.Activities.Suggested.Adapters;
using Facesofnaija.Adapters;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Group;
using Facesofnaija.CustomApi.Classes.Search;
using WoWonderClient.Requests;
using Facesofnaija.CustomApi.Classes.Global;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Activities.Communities.Communities;
using Facesofnaija.CustomApi.Classes.Community;

namespace Facesofnaija.Activities.Suggested.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SuggestedCommunityActivity : BaseActivity
    {
        #region Variables Basic

        private SuggestedCommunityAdapter MAdapter;
        private SearchCommunityAdapter RandomAdapter;
        private CategoriesImageAdapter CategoriesAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;

        private ViewStub EmptyStateLayout, SuggestedCommunityViewStub, CatCommunityViewStub, RandomCommunityViewStub;
        private View Inflated, SuggestedCommunityInflated, CatGroupInflated, RandomCommunityInflated;
        private TemplateRecyclerInflater RecyclerInflaterSuggestedCommunity,RecyclerInflaterCatCommunity,RecyclerInflaterRandomCommunity;
        private RecyclerViewOnScrollListener SuggestedCommunityScrollEvent;

        private LinearLayout Devider1, Devider2;

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
                SetContentView(Resource.Layout.SuggestedGroupLayout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                Task.Factory.StartNew(StartApiService);
                AdsGoogle.Ad_Interstitial(this);
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
                ListUtils.SuggestedCommunityList = MAdapter.CommunityList.Count switch
                {
                    > 0 => MAdapter.CommunityList,
                    _ => ListUtils.SuggestedCommunityList
                };

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
                EmptyStateLayout = FindViewById<ViewStub>(Resource.Id.viewStub);

                SuggestedCommunityViewStub = FindViewById<ViewStub>(Resource.Id.viewStubSuggestedGroup);
                CatCommunityViewStub = FindViewById<ViewStub>(Resource.Id.viewStubCatGroup);
                RandomCommunityViewStub = FindViewById<ViewStub>(Resource.Id.viewStubRandomGroup);

                Devider1 = FindViewById<LinearLayout>(Resource.Id.linearLayout1);
                Devider2 = FindViewById<LinearLayout>(Resource.Id.linearLayout2);

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
                    toolBar.Title = GetString(Resource.String.Lbl_Discover);
                    toolBar.SetTitleTextColor(Color.ParseColor(AppSettings.MainColor));
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                    SupportActionBar.SetHomeAsUpIndicator(AppCompatResources.GetDrawable(this, AppSettings.FlowDirectionRightToLeft ? Resource.Drawable.ic_action_right_arrow_color : Resource.Drawable.ic_action_left_arrow_color));

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
                MAdapter = new SuggestedCommunityAdapter(this) { CommunityList = new ObservableCollection<CommunityDataObject>()  };
                MAdapter.ItemClick += MAdapterOnItemClick;
                MAdapter.JoinButtonItemClick += MAdapterOnJoinButtonItemClick;

                RandomAdapter = new SearchCommunityAdapter(this) { CommunityList = new ObservableCollection<CommunityDataObject>() };
                RandomAdapter.ItemClick += RandomAdapterOnItemClick;
                RandomAdapter.JoinButtonItemClick += MAdapterRandomOnJoinButtonItemClick;

                switch (CategoriesController.ListCategoriesGroup.Count)
                {
                    case > 0:
                    {
                        CategoriesAdapter = new CategoriesImageAdapter(this) { CategoriesList = CategoriesController.ListCategoriesGroup };
                        CategoriesAdapter.ItemClick += CategoriesAdapterOnItemClick;

                        CatGroupInflated = CatGroupInflated switch
                        {
                            null => CatCommunityViewStub.Inflate(),
                            _ => CatGroupInflated
                        };

                        RecyclerInflaterCatCommunity = new TemplateRecyclerInflater();
                        RecyclerInflaterCatCommunity.InflateLayout<Classes.Categories>(this, CatGroupInflated, CategoriesAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_Categories), GetString(Resource.String.Lbl_FindGroupByCategories));

                        RecyclerInflaterCatCommunity.Recyler.Visibility = ViewStates.Visible;

                        CategoriesAdapter.NotifyDataSetChanged();
                        break;
                    }
                    default:
                        Methods.DisplayReportResult(this, "Not have List Categories Group");
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
                MAdapter = null!;
                RandomAdapter = null!;
                CategoriesAdapter = null!;
                SwipeRefreshLayout = null!;
                EmptyStateLayout = null!;
                SuggestedCommunityViewStub = null!;
                CatCommunityViewStub = null!;
                RandomCommunityViewStub = null!;
                Inflated = null!;
                SuggestedCommunityInflated = null!;
                CatGroupInflated = null!;
                RandomCommunityInflated = null!;
                RecyclerInflaterSuggestedCommunity = null!;
                RecyclerInflaterCatCommunity = null!;
                RecyclerInflaterRandomCommunity = null!;
                SuggestedCommunityScrollEvent = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        //Get Group By Categories
        private void CategoriesAdapterOnItemClick(object sender, CategoriesImageAdapterClickEventArgs e)
        {
            try
            {
                var item = CategoriesAdapter.GetItem(e.Position);
                if (item != null)
                {
                    var intent = new Intent(this, typeof(GroupByCategoriesActivity));
                    intent.PutExtra("CategoryId", item.CategoriesId);
                    intent.PutExtra("CategoryName", Methods.FunString.DecodeString(item.CategoriesName));
                    StartActivity(intent);
                } 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
         
        //See all SuggestedGroup
        private void MainLinearSuggestedGroupOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(this, typeof(AllSuggestedGroupActivity));
                StartActivity(intent); 
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Offset Suggested Group
        private void SuggestedGroupScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                var item = MAdapter.CommunityList.LastOrDefault();
                if (item != null && !string.IsNullOrEmpty(item.CommunityId))
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadCommunity(item.CommunityId) });   
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Open Profile Suggested Group
        private void MAdapterOnItemClick(object sender, SuggestedCommunityAdapterClickEventArgs e)
        {
            try
            {
                var item = MAdapter.GetItem(e.Position);
                if (item != null)
                {
                    MainApplication.GetInstance()?.NavigateTo(this, typeof(GroupProfileActivity), item);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void MAdapterRandomOnJoinButtonItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
        {
            try
            { 

                var item = MAdapter.GetItem(e.Position);
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
                            case JoinGroupObject result when result.JoinStatus == "requested":
                                e.Button.SetTextColor(Color.ParseColor(AppSettings.MainColor));
                                e.Button.Text = Application.Context.GetText(Resource.String.Lbl_Request);
                                e.Button.SetBackgroundResource(Resource.Drawable.round_button_normal);
                                break;
                            case JoinGroupObject result:
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
        }

        private void MAdapterOnJoinButtonItemClick(object sender, SuggestedCommunityAdapterClickEventArgs e)
        {
            try
            {
                var item = RandomAdapter.GetItem(e.Position);
                if (item != null)
                {
                    WoWonderTools.SetJoinCommunity(this, item.CommunityId, e.JoinButton);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }


        //Open Profile Random Group
        private void RandomAdapterOnItemClick(object sender, SearchCommunityAdapterClickEventArgs e)
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
        }

        #endregion

        #region Load Data

        private void StartApiService()
        {
            if (!Methods.CheckConnectivity())
                ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadCommunity("0") , () => LoadRandomCommunity("0") });
        }

        private async Task LoadCommunity(string offset)
        {
            if (SuggestedCommunityScrollEvent != null && SuggestedCommunityScrollEvent.IsLoading)
                return;

            if (Methods.CheckConnectivity())
            {
                if (SuggestedCommunityScrollEvent != null) SuggestedCommunityScrollEvent.IsLoading = true;
                var countList = MAdapter.CommunityList.Count;

                var (respondCode, respondString) = await RequestsAsync.Group.GetRecommendedGroupsAsync("10", offset);
                switch (respondCode)
                {
                    case 200:
                        {
                            switch (respondString)
                            {
                                case ListCommunitiesObject result:
                                    {
                                        var respondList = result.Data.Count;
                                        switch (respondList)
                                        {
                                            case > 0 when countList > 0:
                                                {
                                                    foreach (var item in from item in result.Data let check = MAdapter.CommunityList.FirstOrDefault(a => a.CommunityId == item.CommunityId) where check == null select item)
                                                    {
                                                        MAdapter.CommunityList.Add(item);
                                                    }

                                                    RunOnUiThread(() => { MAdapter.NotifyItemRangeInserted(countList, MAdapter.CommunityList.Count - countList); });
                                                    break;
                                                }
                                            case > 0:
                                                MAdapter.CommunityList = new ObservableCollection<CommunityDataObject>(result.Data);

                                                RunOnUiThread(() =>
                                                {
                                                    SuggestedCommunityInflated ??= SuggestedCommunityViewStub.Inflate();

                                                    RecyclerInflaterSuggestedCommunity = new TemplateRecyclerInflater();
                                                    RecyclerInflaterSuggestedCommunity.InflateLayout<CommunityDataObject>(this, SuggestedCommunityInflated, MAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerHorizontal, 0, true, GetString(Resource.String.Lbl_SuggestedForYou), "", true);

                                                    RecyclerInflaterSuggestedCommunity.MainLinear.Click += MainLinearSuggestedGroupOnClick;

                                                    switch (SuggestedCommunityScrollEvent)
                                                    {
                                                        case null:
                                                            {
                                                                RecyclerViewOnScrollListener playlistRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(RecyclerInflaterSuggestedCommunity.LayoutManager);
                                                                SuggestedCommunityScrollEvent = playlistRecyclerViewOnScrollListener;
                                                                SuggestedCommunityScrollEvent.LoadMoreEvent += SuggestedGroupScrollEventOnLoadMoreEvent;
                                                                RecyclerInflaterSuggestedCommunity.Recyler.AddOnScrollListener(playlistRecyclerViewOnScrollListener);
                                                                SuggestedCommunityScrollEvent.IsLoading = false;
                                                                break;
                                                            }
                                                    }
                                                });
                                                break;
                                            default:
                                                {
                                                    if (RecyclerInflaterSuggestedCommunity?.Recyler != null && MAdapter.CommunityList.Count > 10 && !RecyclerInflaterSuggestedCommunity.Recyler.CanScrollVertically(1))
                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
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
                    Devider1.Visibility = ViewStates.Visible;
                    RunOnUiThread(ShowEmptyPage);
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
                if (SuggestedCommunityScrollEvent != null) SuggestedCommunityScrollEvent.IsLoading = false;
            }
        }

        private async Task LoadRandomCommunity(string offset)
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

                                            RecyclerInflaterRandomCommunity = new TemplateRecyclerInflater();
                                            //RecyclerInflaterRandomCommunity.InflateLayout<GroupDataObject>(this, RandomGroupInflated, RandomAdapter, TemplateRecyclerInflater.TypeLayoutManager.LinearLayoutManagerVertical, 0, true, GetString(Resource.String.Lbl_RandomCommunities));
                                        });
                                        break;
                                    default:
                                    {
                                        if (RecyclerInflaterRandomCommunity?.Recyler != null && RandomAdapter.CommunityList.Count > 10 && !RecyclerInflaterRandomCommunity.Recyler.CanScrollVertically(1))
                                            ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_NoMoreGroup), ToastLength.Short);
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
                    Devider2.Visibility = ViewStates.Visible;
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
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (SuggestedCommunityScrollEvent != null) SuggestedCommunityScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;

                if (MAdapter.CommunityList.Count > 0)
                {
                    if (RecyclerInflaterSuggestedCommunity?.Recyler != null)
                        RecyclerInflaterSuggestedCommunity.Recyler.Visibility = ViewStates.Visible;
                     
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (RandomAdapter.CommunityList.Count > 0)
                {
                    if (RecyclerInflaterRandomCommunity?.Recyler != null)
                        RecyclerInflaterRandomCommunity.Recyler.Visibility = ViewStates.Visible;
                     
                    EmptyStateLayout.Visibility = ViewStates.Gone;
                }

                if (MAdapter.CommunityList.Count == 0 && RandomAdapter.CommunityList.Count == 0)
                {
                    if (RecyclerInflaterSuggestedCommunity?.Recyler != null)
                        RecyclerInflaterSuggestedCommunity.Recyler.Visibility = ViewStates.Gone;

                    Inflated ??= EmptyStateLayout.Inflate();

                    EmptyStateInflater x = new EmptyStateInflater();
                    x.InflateLayout(Inflated, EmptyStateInflater.Type.NoGroup);
                    switch (x.EmptyStateButton.HasOnClickListeners)
                    {
                        case false:
                            x.EmptyStateButton.Click += null!;
                            break;
                    }

                    EmptyStateLayout.Visibility = ViewStates.Visible;
                }
            }
            catch (Exception e)
            {
                if (SuggestedCommunityScrollEvent != null) SuggestedCommunityScrollEvent.IsLoading = false;
                SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
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