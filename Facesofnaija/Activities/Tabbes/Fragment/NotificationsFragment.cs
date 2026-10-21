using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Bumptech.Glide.Util;
using Google.Android.Material.Tabs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Facesofnaija.Activities.Communities.Communities;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Events;
using Facesofnaija.Activities.FriendRequest;
using Facesofnaija.Activities.Memories;
using Facesofnaija.Activities.NativePost.Pages;
using Facesofnaija.Activities.Story;
using Facesofnaija.Activities.Tabbes.Adapters;
using Facesofnaija.Activities.UserProfile;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.IntegrationRecyclerView;
using Facesofnaija.Library.Anjo.SuperTextLibrary;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Page;
using WoWonderClient.Classes.Story;
using WoWonderClient.Classes.User;
using WoWonderClient.Requests;
using WoWonderClient;

namespace Facesofnaija.Activities.Tabbes.Fragment
{
    public class NotificationsFragment : AndroidX.Fragment.App.Fragment
    {
        #region Variables Basic

        private NotificationsAdapter MAdapter;
        private TabbedMainActivity GlobalContext;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout, AnnouncementViewStub, FriendRequestViewStub, CommunitiesRequestViewStub;
        private View Inflated, AnnouncementInflated, FriendRequestInflated, CommunitiesRequestInflated;
        private TabLayout Tabs;
        private static string NameType = "All", TextAnnouncement;

        private RecyclerViewOnScrollListener MainScrollEvent;
        private bool MIsVisibleToUser;

        #endregion

        #region General

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
            GlobalContext = TabbedMainActivity.GetInstance();
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TNotificationsLayout, container, false);
                return view;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);
                InitComponent(view);
                SetRecyclerViewAdapters();

                Task.Factory.StartNew(() => StartApiService(false));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnTabVisible()
        {
            try
            {
                if (MAdapter == null || MAdapter.NotificationsList == null || MAdapter.NotificationsList.Count == 0)
                {
                    Task.Factory.StartNew(() => StartApiService(false));
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void SetMenuVisibility(bool menuVisible)
        {
            try
            {
                base.SetMenuVisibility(menuVisible);
                MIsVisibleToUser = menuVisible;
                if (menuVisible)
                {
                    OnTabVisible();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public override void OnResume()
        {
            try
            {
                base.OnResume();

                if (MAdapter == null || MAdapter.NotificationsList == null || MAdapter.NotificationsList.Count == 0)
                {
                    Task.Factory.StartNew(() => StartApiService(false));
                }
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                SwipeRefreshLayout = (SwipeRefreshLayout)view.FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = true;
                SwipeRefreshLayout.Enabled = true;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));
                SwipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;

                AnnouncementViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubAnnouncement);
                FriendRequestViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubFriendRequest);
                FriendRequestViewStub.LayoutResource = Resource.Layout.ViewModel_Request;
                CommunitiesRequestViewStub = (ViewStub)view.FindViewById(Resource.Id.viewStubCommunitiesRequest);

                Tabs = view.FindViewById<TabLayout>(Resource.Id.tab_home);
                Tabs.TabSelected += TabsOnTabSelected;

                var tabLastActivities = Tabs.GetTabAt(1).View;
                var tabFriendsBirthday = Tabs.GetTabAt(2).View;

                if (!AppSettings.ShowLastActivities)
                    tabLastActivities.Visibility = ViewStates.Gone;

                if (!AppSettings.ShowFriendsBirthday)
                    tabFriendsBirthday.Visibility = ViewStates.Gone;

                MRecycler = (RecyclerView)view.FindViewById(Resource.Id.recyler);
                EmptyStateLayout = view.FindViewById<ViewStub>(Resource.Id.viewStub);
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
                MAdapter = new NotificationsAdapter(Activity) { NotificationsList = new ObservableCollection<Classes.NotificationsClass>() };
                MAdapter.ItemClick += MAdapterOnItemClick;
                LayoutManager = new LinearLayoutManager(Activity);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;

                RecyclerViewOnScrollListener xamarinRecyclerViewOnScrollListener = new RecyclerViewOnScrollListener(LayoutManager);
                MainScrollEvent = xamarinRecyclerViewOnScrollListener;
                MainScrollEvent.LoadMoreEvent += MainScrollEventOnLoadMoreEvent;
                MRecycler.AddOnScrollListener(xamarinRecyclerViewOnScrollListener);
                MainScrollEvent.IsLoading = false;

                var sizeProvider = new FixedPreloadSizeProvider(10, 10);
                var preLoader = new RecyclerViewPreloader<Classes.NotificationsClass>(Activity, MAdapter, sizeProvider, 10);
                MRecycler.AddOnScrollListener(preLoader);
                MRecycler.SetAdapter(MAdapter);

                MRecycler.NestedScrollingEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Open user profile
        private void MAdapterOnItemClick(object sender, NotificationsAdapterClickEventArgs e)
        {
            try
            {
                if (e.Position > -1)
                {
                    var item = MAdapter.GetItem(e.Position);
                    if (item != null)
                    {
                        if (NameType == "All")
                        {
                            EventClickNotification(Activity, item.Notification);
                        }
                        else if (NameType == "LastActivities")
                        {
                            if (item.LastActivities.ActivityType == "following" || item.LastActivities.ActivityType == "friend")
                            {
                                WoWonderTools.OpenProfile(Activity, item.LastActivities.UserId, item.LastActivities.Activator);
                            }
                            else
                            {
                                var intent = new Intent(Activity, typeof(ViewFullPostActivity));
                                intent.PutExtra("Id", item.LastActivities.PostId);
                                //intent.PutExtra("DataItem", JsonConvert.SerializeObject(item.PostData));
                                Activity.StartActivity(intent);
                            }
                        }
                        else if (NameType == "FriendsBirthday")
                        {
                            WoWonderTools.OpenProfile(Activity, item.User.UserId, item.User);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //Refresh
        private void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
        {
            try
            {
                MAdapter.NotificationsList.Clear();
                MAdapter.NotifyDataSetChanged();

                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;

                MRecycler.Visibility = ViewStates.Visible;
                EmptyStateLayout.Visibility = ViewStates.Gone;
                if (Inflated != null) Inflated.Visibility = ViewStates.Gone;

                if (NameType == "All")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
                }
                else if (NameType == "LastActivities")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync() });
                }
                else if (NameType == "FriendsBirthday")
                {
                    if (!Methods.CheckConnectivity())
                        ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    else
                        PollyController.RunRetryPolicyFunction(new List<Func<Task>> { GetFriendsBirthdayApi });
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MainScrollEventOnLoadMoreEvent(object sender, EventArgs e)
        {
            try
            {
                //Code get last id where LoadMore >>
                var item = MAdapter.NotificationsList.LastOrDefault();

                if (NameType == "All")
                {
                    if (item != null && !string.IsNullOrEmpty(item.Notification.NotifierId) && !MainScrollEvent.IsLoading)
                    {
                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(false, item.Notification.NotifierId) });
                    }
                }
                else if (NameType == "LastActivities")
                {
                    if (item != null && !string.IsNullOrEmpty(item.LastActivities?.Id) && !MainScrollEvent.IsLoading)
                    {
                        if (!Methods.CheckConnectivity())
                            ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                        else
                            PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadActivitiesAsync(item.LastActivities.Id) });
                    }
                }
                else if (NameType == "FriendsBirthday")
                {
                    //Nothing 
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void TabsOnTabSelected(object sender, TabLayout.TabSelectedEventArgs e)
        {
            try
            {
                switch (e.Tab.Position)
                {
                    //All Notifications
                    case 0:
                        {
                            NameType = "All";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await LoadGeneralData(false);
                            break;
                        }
                    //Last Activities
                    case 1:
                        {
                            NameType = "LastActivities";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await LoadActivitiesAsync();
                            break;
                        }
                    //Friends Birthday
                    case 2:
                        {
                            NameType = "FriendsBirthday";

                            MAdapter.NotificationsList.Clear();
                            MAdapter.NotifyDataSetChanged();

                            await GetFriendsBirthdayApi();
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Load Notification 

        private void StartApiService(bool seenNotifications)
        {
            if (!Methods.CheckConnectivity())
            {
                Activity?.RunOnUiThread(ShowEmptyPage);
                ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
            }
            else
                PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(seenNotifications), LoadReviewsAsync });
        }

        //Get General Data Using Api >> notifications , pro_users , promoted_pages , trending_hashTag
        public async Task<(string, string, string)> LoadGeneralData(bool seenNotifications, string offset = "0")
        {
            if (MainScrollEvent != null && MainScrollEvent.IsLoading) return ("", "", "");

            if (Methods.CheckConnectivity())
            {
                if (MainScrollEvent != null)
                    MainScrollEvent.IsLoading = true;

                string fetch = "notifications,friend_requests";

                if (AppSettings.ShowAnnouncement)
                    fetch += ",announcement";

                if (AppSettings.ShowProUsersMembers)
                    fetch += ",pro_users";

                if (AppSettings.ShowPromotedPages)
                    fetch += ",promoted_pages";

                if (AppSettings.ShowTrendingHashTags)
                    fetch += ",trending_hashtag";

                if (AppSettings.MessengerIntegration)
                    fetch += ",count_new_messages";

                var (apiStatus, respond) = await CustomRequests.GetGeneralDataDirectAsync(seenNotifications, UserDetails.OnlineUsers, UserDetails.DeviceId, offset, fetch);
                if (apiStatus == 200)
                {
                    if (respond is GetGeneralDataObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                // Notifications
                                var countList = MAdapter.NotificationsList.Count;
                                var respondList = result.Notifications?.Count ?? 0;
                                Android.Util.Log.Warn("FON_NOTIF", $"LoadGeneralData: success, notifications={respondList}");
                                if (respondList > 0 && result.Notifications != null)
                                {
                                    foreach (var item in from item in result.Notifications let check = MAdapter.NotificationsList.FirstOrDefault(a => a.Notification?.Id == item.Id) where check == null select item)
                                    {
                                        MAdapter.NotificationsList.Add(new Classes.NotificationsClass()
                                        {
                                            Type = Classes.ItemType.Notifications,
                                            Notification = item
                                        });
                                    }

                                    var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.Notifications).ToList();
                                    if (list.Count > 0)
                                        foreach (var item in list)
                                            MAdapter.NotificationsList.Remove(item);

                                    if (countList > 0)
                                    {
                                        MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList);
                                    }
                                    else
                                    {
                                        MAdapter.NotifyDataSetChanged();
                                    }
                                }
                                else
                                {
                                    if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                        ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreNotifications), ToastLength.Short);
                                }

                                // FriendRequests
                                var respondListFriendRequests = result?.FriendRequests?.Count;
                                if (respondListFriendRequests > 0)
                                {
                                    ListUtils.FriendRequestsList = new ObservableCollection<UserDataObject>(result.FriendRequests);

                                    if (FriendRequestInflated == null)
                                    {
                                        FriendRequestInflated = FriendRequestViewStub.Inflate();

                                        var friendRequestImage = (ImageView)FriendRequestInflated.FindViewById(Resource.Id.imageFirstUser);
                                        var txTFriendRequest = (TextView)FriendRequestInflated.FindViewById(Resource.Id.tv_Friends_connection);
                                        var txtAllFriendRequest = (TextView)FriendRequestInflated.FindViewById(Resource.Id.tv_Friends);
                                        var friendRequestCount = (TextView)FriendRequestInflated.FindViewById(Resource.Id.count_view);

                                        FriendRequestInflated.Click += LayoutFriendRequestOnClick;

                                        txTFriendRequest.Text = Activity.GetText(AppSettings.ConnectivitySystem == 1 ? Resource.String.Lbl_FollowRequest : Resource.String.Lbl_FriendRequest);

                                        var user = ListUtils.FriendRequestsList.FirstOrDefault();
                                        if (user != null)
                                        {
                                            GlideImageLoader.LoadImage(Activity, user.Avatar, friendRequestImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                            if (respondListFriendRequests > 2)
                                                txtAllFriendRequest.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(user), 18) + " " + GetText(Resource.String.Lbl_And) + " " + ListUtils.FriendRequestsList.Count + " " + GetText(Resource.String.Lbl_OtherPeople);
                                            else
                                                txtAllFriendRequest.Text = Methods.FunString.SubStringCutOf(WoWonderTools.GetNameFinal(user), 18) + " " + GetText(Resource.String.Lbl_And) + " " + GetText(Resource.String.Lbl_OtherPeople);
                                        }

                                        friendRequestCount.Text = ListUtils.FriendRequestsList.Count.ToString();
                                        friendRequestCount.Visibility = ViewStates.Visible;
                                    }
                                }
                                else
                                {
                                    if (FriendRequestViewStub != null)
                                        FriendRequestViewStub.Visibility = ViewStates.Gone;
                                }

                                if (AppSettings.ShowTrendingPage && GlobalContext.TrendingTab != null)
                                {
                                    // TrendingHashtag
                                    var respondListHashTag = result?.TrendingHashtag?.Count;
                                    if (respondListHashTag > 0 && AppSettings.ShowTrendingHashTags)
                                    {
                                        ListUtils.HashTagList = new ObservableCollection<TrendingHashtag>(result.TrendingHashtag);

                                        var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.HashTag);
                                        if (checkList == null)
                                        {
                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                            {
                                                Id = 900,
                                                Title = Activity.GetText(Resource.String.Lbl_TrendingHashTags),
                                                SectionType = Classes.ItemType.HashTag,
                                                Type = Classes.ItemType.Section,
                                            });

                                            var list = result.TrendingHashtag.Take(5).ToList();

                                            foreach (var item in from item in list let check = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(a => a.HashTags?.Id == item.Id && a.Type == Classes.ItemType.HashTag) where check == null select item)
                                            {
                                                GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                                {
                                                    Id = long.Parse(item.Id),
                                                    HashTags = item,
                                                    Type = Classes.ItemType.HashTag
                                                });
                                            }

                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Add(new Classes.TrendingClass
                                            {
                                                Type = Classes.ItemType.Divider
                                            });
                                        }
                                    }

                                    // PromotedPages
                                    var respondListPromotedPages = result.PromotedPages?.Count;
                                    if (respondListPromotedPages > 0 && AppSettings.ShowPromotedPages)
                                    {
                                        var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ProPage);
                                        if (checkList == null)
                                        {
                                            var proPage = new Classes.TrendingClass
                                            {
                                                Id = 200,
                                                PageList = new List<PageDataObject>(),
                                                Type = Classes.ItemType.ProPage
                                            };

                                            foreach (var item in from item in result.PromotedPages let check = proPage.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                            {
                                                proPage.PageList.Add(item);
                                            }

                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(0, proPage);
                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(1, new Classes.TrendingClass
                                            {
                                                Type = Classes.ItemType.Divider
                                            });
                                        }
                                        else
                                        {
                                            foreach (var item in from item in result.PromotedPages let check = checkList.PageList.FirstOrDefault(a => a.PageId == item.PageId) where check == null select item)
                                            {
                                                checkList.PageList.Add(item);
                                            }
                                        }
                                    }

                                    // ProUsers
                                    var respondListProUsers = result?.ProUsers?.Count;
                                    if (respondListProUsers > 0 && AppSettings.ShowProUsersMembers)
                                    {
                                        var checkList = GlobalContext.TrendingTab.MAdapter.TrendingList.FirstOrDefault(q => q.Type == Classes.ItemType.ProUser);
                                        if (checkList == null)
                                        {
                                            var proUser = new Classes.TrendingClass
                                            {
                                                Id = 100,
                                                UserList = new List<UserDataObject>(),
                                                Type = Classes.ItemType.ProUser
                                            };

                                            var data = ListUtils.MyProfileList?.FirstOrDefault();
                                            if (data?.IsPro != "1" && AppSettings.ShowGoPro)
                                            {
                                                var dataOwner = proUser.UserList.FirstOrDefault(a => a.Type == "Your");
                                                if (dataOwner == null && data != null)
                                                {
                                                    data.Type = "Your";
                                                    data.Username = Context.GetText(Resource.String.Lbl_AddMe);
                                                    proUser.UserList.Insert(0, data);
                                                }
                                            }

                                            foreach (var item in from item in result.ProUsers let check = proUser.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                            {
                                                proUser.UserList.Add(item);
                                            }

                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(0, proUser);
                                            GlobalContext.TrendingTab.MAdapter.TrendingList.Insert(1, new Classes.TrendingClass
                                            {
                                                Type = Classes.ItemType.Divider
                                            });
                                        }
                                        else
                                        {
                                            foreach (var item in from item in result.ProUsers let check = checkList.UserList.FirstOrDefault(a => a.UserId == item.UserId) where check == null select item)
                                            {
                                                checkList.UserList.Add(item);
                                            }
                                        }
                                    }

                                    GlobalContext.TrendingTab.MAdapter.NotifyDataSetChanged();
                                }

                                if (!string.IsNullOrEmpty(result.Announcement?.AnnouncementClass?.TextDecode))
                                {
                                    if (AnnouncementInflated == null)
                                    {
                                        AnnouncementInflated = AnnouncementViewStub.Inflate();

                                        var AnnouncementImage = (AppCompatButton)AnnouncementInflated.FindViewById(Resource.Id.announcementButton);
                                        AnnouncementImage.Click += AnnouncementImageOnClick;
                                    }

                                    TextAnnouncement = result.Announcement?.AnnouncementClass?.TextDecode;
                                }

                                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                                ShowEmptyPage();
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                        return (result.NewNotificationsCount, result.NewFriendRequestsCount, result.CountNewMessages);
                    }
                }
                else
                {
                    if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                    Android.Util.Log.Warn("FON_NOTIF", $"LoadGeneralData failed apiStatus={apiStatus}");
                }

                Activity?.RunOnUiThread(ShowEmptyPage);

                return ("", "", "");
            }
            else
            {
                Inflated ??= EmptyStateLayout.Inflate();
                EmptyStateInflater x = new EmptyStateInflater();
                x.InflateLayout(Inflated, EmptyStateInflater.Type.NoConnection);
                if (!x.EmptyStateButton.HasOnClickListeners)
                {
                    x.EmptyStateButton.Click += null!;
                    x.EmptyStateButton.Click += EmptyStateButtonOnClick;
                }

                ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
            }

            return ("", "", "");
        }

        private async Task LoadActivitiesAsync(string offset = "0")
        {
            if (MainScrollEvent != null && MainScrollEvent.IsLoading) return;

            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowLastActivities)
                {
                    if (MainScrollEvent != null)
                        MainScrollEvent.IsLoading = true;

                    var (apiStatus, respond) = await RequestsAsync.Global.GetActivitiesAsync("6", offset);
                    if (apiStatus == 200)
                    {
                        if (respond is LastActivitiesObject result)
                        {
                            // LastActivities
                            var countList = MAdapter.NotificationsList.Count;
                            var respondList = result.Activities.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Activities let check = MAdapter.NotificationsList.FirstOrDefault(a => a.LastActivities?.Id == item.Id) where check == null select item)
                                {
                                    MAdapter.NotificationsList.Add(new Classes.NotificationsClass()
                                    {
                                        Type = Classes.ItemType.LastActivities,
                                        LastActivities = item
                                    });
                                }

                                var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.LastActivities).ToList();
                                if (list.Count > 0)
                                    foreach (var item in list)
                                        MAdapter.NotificationsList.Remove(item);

                                if (countList > 0)
                                {
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList);
                                }
                                else
                                {
                                    MAdapter.NotifyDataSetChanged();
                                }
                            }
                            else
                            {
                                if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_NoMoreActivities), ToastLength.Short);
                            }
                        }
                    }
                    else
                    {
                        if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                        Android.Util.Log.Warn("FON_NOTIF", $"LoadActivitiesAsync failed status={apiStatus}");
                    }
                }

                Activity?.RunOnUiThread(ShowEmptyPage);
            }
        }

        private async Task GetFriendsBirthdayApi()
        {
            if (Methods.CheckConnectivity())
            {
                if (AppSettings.ShowFriendsBirthday)
                {
                    var (apiStatus, respond) = await RequestsAsync.Global.GetFriendsBirthdayAsync();
                    if (apiStatus == 200)
                    {
                        if (respond is ListUsersObject result)
                        {
                            MAdapter.NotificationsList.Clear();

                            // FriendsBirthday 
                            var countList = MAdapter.NotificationsList.Count;
                            var respondList = result.Data.Count;
                            if (respondList > 0)
                            {
                                foreach (var item in from item in result.Data let check = MAdapter.NotificationsList.FirstOrDefault(a => a.User?.UserId == item.UserId) where check == null select item)
                                {
                                    MAdapter.NotificationsList.Add(new Classes.NotificationsClass()
                                    {
                                        Type = Classes.ItemType.FriendsBirthday,
                                        User = item
                                    });
                                }

                                var list = MAdapter.NotificationsList.Where(a => a.Type != Classes.ItemType.FriendsBirthday).ToList();
                                if (list.Count > 0)
                                    foreach (var item in list)
                                        MAdapter.NotificationsList.Remove(item);

                                if (countList > 0)
                                {
                                    MAdapter.NotifyItemRangeInserted(countList, MAdapter.NotificationsList.Count - countList);
                                }
                                else
                                {
                                    MAdapter.NotifyDataSetChanged();
                                }
                            }
                            else
                            {
                                if (MAdapter.NotificationsList.Count > 10 && !MRecycler.CanScrollVertically(1))
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_No_more_users), ToastLength.Short);
                            }
                        }
                    }
                    else
                    {
                        Android.Util.Log.Warn("FON_NOTIF", $"GetFriendsBirthdayApi failed status={apiStatus}");
                    }

                    Activity?.RunOnUiThread(ShowEmptyPage);
                }
            }
        }

        private void LayoutFriendRequestOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(FriendRequestActivity));
                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShowEmptyPage()
        {
            try
            {
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                if (SwipeRefreshLayout != null) SwipeRefreshLayout.Refreshing = false;

                switch (MAdapter?.NotificationsList?.Count)
                {
                    case > 0:
                        if (MRecycler != null) MRecycler.Visibility = ViewStates.Visible;
                        if (EmptyStateLayout != null) EmptyStateLayout.Visibility = ViewStates.Gone;
                        if (Inflated != null) Inflated.Visibility = ViewStates.Gone;
                        break;
                    default:
                        {
                            if (MRecycler != null) MRecycler.Visibility = ViewStates.Gone;

                            Inflated ??= EmptyStateLayout.Inflate();

                            EmptyStateInflater x = new EmptyStateInflater();
                            x.InflateLayout(Inflated, EmptyStateInflater.Type.NoNotifications);
                            switch (x.EmptyStateButton.HasOnClickListeners)
                            {
                                case false:
                                    x.EmptyStateButton.Click += null!;
                                    break;
                            }
                            if (EmptyStateLayout != null) EmptyStateLayout.Visibility = ViewStates.Visible;
                            if (Inflated != null) Inflated.Visibility = ViewStates.Visible;
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                if (MainScrollEvent != null) MainScrollEvent.IsLoading = false;
                if (SwipeRefreshLayout != null) SwipeRefreshLayout.Refreshing = false;
                Methods.DisplayReportResultTrack(e);
            }
        }

        //No Internet Connection 
        private void EmptyStateButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    ToastUtils.ShowToast(Context, Context.GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                else
                    PollyController.RunRetryPolicyFunction(new List<Func<Task>> { () => LoadGeneralData(true) });
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task LoadReviewsAsync()
        {
            if (Methods.CheckConnectivity())
            {
                var (apiStatus, respond) = await RequestsAsync.Page.GetInvitesAsync("10");
                if (apiStatus == 200)
                {
                    if (respond is GetInvitesObject result)
                    {
                        Activity?.RunOnUiThread(() =>
                        {
                            try
                            {
                                var respondList = result.Data.Count;
                                if (respondList > 0)
                                {
                                    if (CommunitiesRequestInflated == null)
                                    {
                                        CommunitiesRequestInflated = CommunitiesRequestViewStub.Inflate();

                                        var CommunitiesImage = (ImageView)CommunitiesRequestInflated?.FindViewById(Resource.Id.imageFirstUser);
                                        var txTCommunities = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.tv_Friends_connection);
                                        var txtAllCommunities = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.tv_Friends);
                                        var CommunitiesCount = (TextView)CommunitiesRequestInflated?.FindViewById(Resource.Id.count_view);

                                        txtAllCommunities.Visibility = ViewStates.Gone;

                                        CommunitiesRequestInflated.Click += CommunitiesRequestInflatedOnClick;

                                        txTCommunities.Text = Activity.GetText(Resource.String.Lbl_InvitationToPage);

                                        var user = result.Data.FirstOrDefault();
                                        if (user != null)
                                            GlideImageLoader.LoadImage(Activity, user.Avatar, CommunitiesImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                                        CommunitiesCount.Text = result.Data.Count.ToString();
                                        CommunitiesCount.Visibility = ViewStates.Visible;
                                    }
                                }
                                else
                                {
                                    if (CommunitiesRequestViewStub != null)
                                        CommunitiesRequestViewStub.Visibility = ViewStates.Gone;
                                }
                            }
                            catch (Exception e)
                            {
                                Methods.DisplayReportResultTrack(e);
                            }
                        });
                    }
                }
                else
                    Methods.DisplayReportResult(Activity, respond);
            }
        }

        private void CommunitiesRequestInflatedOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Activity, typeof(InvitedPageActivity));
                Activity.StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        public static string ExtractPostId(NotificationObject item)
        {
            try
            {
                if (item == null) return string.Empty;

                if (!string.IsNullOrEmpty(item.PostId) && item.PostId != "0")
                {
                    var match = Regex.Match(item.PostId, @"^(\d+)");
                    if (match.Success)
                        return match.Groups[1].Value;
                    return item.PostId;
                }

                if (item is Facesofnaija.CustomApi.Classes.Global.CustomNotificationObject custom)
                {
                    if (!string.IsNullOrEmpty(custom.StoryId) && custom.StoryId != "0")
                    {
                        var match = Regex.Match(custom.StoryId, @"^(\d+)");
                        if (match.Success)
                            return match.Groups[1].Value;
                    }
                }

                var url = !string.IsNullOrEmpty(item.Url) ? item.Url : item.AjaxUrl;
                if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(item.FullLink))
                    url = item.FullLink;

                return ExtractPostIdFromUrl(url);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractPostIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                // Do not misidentify community/group/page profile links as posts unless they explicitly have a post identifier
                var isContainerUrl = (url.Contains("link1=community") || url.Contains("/community/") ||
                                      url.Contains("link1=group") || url.Contains("/group/") ||
                                      url.Contains("link1=page") || url.Contains("/page/"));
                var hasPostKeyword = url.Contains("link1=post") || url.Contains("/post/") || url.Contains("/p/") ||
                                     url.Contains("story_id=") || url.Contains("post_id=") || url.Contains("link1=story");

                if (isContainerUrl && !hasPostKeyword)
                {
                    return string.Empty;
                }

                // 1) /post/12345 or /p/12345
                var matchPostPath = Regex.Match(url, @"(?:/post/|/p/)(\d+)", RegexOptions.IgnoreCase);
                if (matchPostPath.Success)
                    return matchPostPath.Groups[1].Value;

                // 2) link1=post or link1=story with id parameter anywhere in query string
                if (url.IndexOf("link1=post", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    url.IndexOf("link1=story", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var matchId = Regex.Match(url, @"[?&]id=(\d+)", RegexOptions.IgnoreCase);
                    if (matchId.Success)
                        return matchId.Groups[1].Value;
                }

                // 3) story_id=12345 or post_id=12345
                var matchStoryId = Regex.Match(url, @"(?:story_id|post_id)=(\d+)", RegexOptions.IgnoreCase);
                if (matchStoryId.Success)
                    return matchStoryId.Groups[1].Value;

                // 4) ?id=12345 if url has post
                if (url.IndexOf("post", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var matchId = Regex.Match(url, @"[?&]id=(\d+)", RegexOptions.IgnoreCase);
                    if (matchId.Success)
                        return matchId.Groups[1].Value;
                }

                // 5) SEO link like domain.com/12345_post_title
                var matchSeo = Regex.Match(url, @"/(?:index\.php/)?(\d+)(?:_[a-zA-Z0-9_-]+)?/?$", RegexOptions.IgnoreCase);
                if (matchSeo.Success)
                    return matchSeo.Groups[1].Value;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractCommunityId(NotificationObject item)
        {
            try
            {
                if (item == null) return string.Empty;

                if (item is Facesofnaija.CustomApi.Classes.Global.CustomNotificationObject custom && !string.IsNullOrEmpty(custom.CommunityId) && custom.CommunityId != "0")
                    return custom.CommunityId;

                var url = !string.IsNullOrEmpty(item.Url) ? item.Url : item.AjaxUrl;
                if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(item.FullLink))
                    url = item.FullLink;

                return ExtractCommunityIdFromUrl(url);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractCommunityIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                var match = Regex.Match(url, @"(?:community_id=|/community/|community=|link1=community.*?id=|/c/)([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;

                if (url.IndexOf("link1=timeline", StringComparison.OrdinalIgnoreCase) >= 0 && url.IndexOf("community", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var matchTimeline = Regex.Match(url, @"[?&]u=([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase);
                    if (matchTimeline.Success)
                        return matchTimeline.Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractPageId(NotificationObject item)
        {
            try
            {
                if (item == null) return string.Empty;

                if (!string.IsNullOrEmpty(item.PageId) && item.PageId != "0")
                    return item.PageId;

                var url = !string.IsNullOrEmpty(item.Url) ? item.Url : item.AjaxUrl;
                if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(item.FullLink))
                    url = item.FullLink;

                return ExtractPageIdFromUrl(url);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractPageIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                var match = Regex.Match(url, @"(?:page_id=|/page/|page=|link1=page.*?id=)([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractGroupId(NotificationObject item)
        {
            try
            {
                if (item == null) return string.Empty;

                if (!string.IsNullOrEmpty(item.GroupId) && item.GroupId != "0")
                    return item.GroupId;

                var url = !string.IsNullOrEmpty(item.Url) ? item.Url : item.AjaxUrl;
                if (string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(item.FullLink))
                    url = item.FullLink;

                return ExtractGroupIdFromUrl(url);
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static string ExtractGroupIdFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return string.Empty;

            try
            {
                var match = Regex.Match(url, @"(?:group_id=|/group/|group=|link1=group.*?id=|/g/)([a-zA-Z0-9_-]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                    return match.Groups[1].Value;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public void EventClickNotification(Activity activity, NotificationObject item)
        {
            try
            {
                if (item == null || activity == null) return;

                if (string.IsNullOrWhiteSpace(Current.AccessToken) && !string.IsNullOrWhiteSpace(UserDetails.AccessToken))
                    Current.AccessToken = UserDetails.AccessToken;

                switch (item.Type?.ToLowerInvariant())
                {
                    case "following":
                    case "visited_profile":
                    case "accepted_request":
                    case "poke":
                        WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                        break;

                    case "invited_page":
                        {
                            var intent = new Intent(activity, typeof(InvitedPageActivity));
                            activity.StartActivity(intent);
                            break;
                        }

                    case "liked_page":
                    case "accepted_invite":
                    case "page_admin":
                        {
                            var pageId = ExtractPageId(item);
                            if (!string.IsNullOrEmpty(pageId) && pageId != "0")
                            {
                                var intent = new Intent(activity, typeof(PageProfileActivity));
                                intent.PutExtra("PageId", pageId);
                                activity.StartActivity(intent);
                            }
                            else
                            {
                                WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            }
                            break;
                        }

                    case "joined_group":
                    case "accepted_join_request":
                    case "added_you_to_group":
                    case "group_admin":
                        {
                            var groupId = ExtractGroupId(item);
                            if (!string.IsNullOrEmpty(groupId) && groupId != "0")
                            {
                                var intent = new Intent(activity, typeof(GroupProfileActivity));
                                intent.PutExtra("GroupId", groupId);
                                activity.StartActivity(intent);
                            }
                            else
                            {
                                WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            }
                            break;
                        }

                    case "requested_to_join_group":
                        {
                            var groupId = ExtractGroupId(item);
                            var intent = new Intent(activity, typeof(Facesofnaija.Activities.Communities.Groups.JoinRequestActivity));
                            intent.PutExtra("GroupId", groupId);
                            activity.StartActivity(intent);
                            break;
                        }

                    case "community_moderator":
                    case "accepted_community_join_request":
                    case "request_join_community":
                    case "requested_to_join_community":
                    case "joined_community":
                        {
                            var commId = ExtractCommunityId(item);
                            if (!string.IsNullOrEmpty(commId) && commId != "0")
                            {
                                var intent = new Intent(activity, typeof(CommunityProfileActivity));
                                intent.PutExtra("CommunityId", commId);
                                activity.StartActivity(intent);
                            }
                            else
                            {
                                WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            }
                            break;
                        }

                    case "going_event":
                    case "interested_event":
                    case "invited_event":
                        {
                            var intent = new Intent(activity, typeof(EventViewActivity));
                            intent.PutExtra("EventId", item.EventId);
                            if (item.Event != null)
                                intent.PutExtra("EventView", JsonConvert.SerializeObject(item.Event));
                            activity.StartActivity(intent);
                            break;
                        }

                    case "viewed_story":
                        {
                            StoryDataObject dataMyStory = GlobalContext?.NewsFeedTab?.PostFeedAdapter?.HolderStory?.StoryAdapter?.StoryList?.FirstOrDefault(o => o.UserId == UserDetails.UserId);
                            if (dataMyStory != null)
                            {
                                ObservableCollection<StoryDataObject> storyList = new ObservableCollection<StoryDataObject> { dataMyStory };

                                Intent intent = new Intent(activity, typeof(StoryDetailsActivity));
                                intent.PutExtra("UserId", dataMyStory.UserId);
                                intent.PutExtra("IndexItem", 0);
                                intent.PutExtra("StoriesCount", storyList.Count);
                                intent.PutExtra("DataItem", JsonConvert.SerializeObject(storyList));
                                activity.StartActivity(intent);
                            }
                            else
                            {
                                WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            }
                            break;
                        }

                    case "memory":
                        {
                            var intent = new Intent(activity, typeof(MemoriesActivity));
                            activity.StartActivity(intent);
                            break;
                        }

                    case "gift":
                        {
                            var ajaxUrl = item.AjaxUrl?.Split(new[] { "&", "gift_img=" }, StringSplitOptions.None);
                            var urlImage = ajaxUrl?.Length > 3 ? WoWonderTools.GetTheFinalLink(ajaxUrl[3]?.Replace("%2F", "/")) : "";

                            var intent = new Intent(activity, typeof(UserProfileActivity));
                            if (!string.IsNullOrEmpty(item.Notifier?.Username))
                                intent.PutExtra("UserObject", JsonConvert.SerializeObject(item.Notifier));

                            intent.PutExtra("UserId", item.Notifier?.UserId ?? item.NotifierId);
                            intent.PutExtra("GifLink", urlImage);
                            activity.StartActivity(intent);
                            break;
                        }

                    case "comment":
                    case "wondered_post":
                    case "wondered_comment":
                    case "reaction":
                    case "react":
                    case "wondered_reply_comment":
                    case "comment_mention":
                    case "comment_reply_mention":
                    case "liked_post":
                    case "liked_comment":
                    case "liked_reply_comment":
                    case "disliked_post":
                    case "disliked_comment":
                    case "disliked_reply_comment":
                    case "post_mention":
                    case "share_post":
                    case "shared_your_post":
                    case "shared_a_post_in_timeline":
                    case "comment_reply":
                    case "also_replied":
                    case "profile_wall_post":
                    case "community_post":
                    case "like_community_post":
                    case "new_post":
                    case "admin_notification":
                    case "live_video":
                    default:
                        {
                            // 1. Check if it has a post ID or post URL
                            var postId = ExtractPostId(item);
                            if (!string.IsNullOrEmpty(postId) && postId != "0")
                            {
                                var intent = new Intent(activity, typeof(ViewFullPostActivity));
                                intent.PutExtra("Id", postId);
                                intent.PutExtra("PostId", postId);
                                activity.StartActivity(intent);
                                break;
                            }

                            // 2. Check community
                            var commId = ExtractCommunityId(item);
                            if (!string.IsNullOrEmpty(commId) && commId != "0")
                            {
                                var intent = new Intent(activity, typeof(CommunityProfileActivity));
                                intent.PutExtra("CommunityId", commId);
                                activity.StartActivity(intent);
                                break;
                            }

                            // 3. Check group
                            var groupId = ExtractGroupId(item);
                            if (!string.IsNullOrEmpty(groupId) && groupId != "0")
                            {
                                var intent = new Intent(activity, typeof(GroupProfileActivity));
                                intent.PutExtra("GroupId", groupId);
                                activity.StartActivity(intent);
                                break;
                            }

                            // 4. Check page
                            var pageId = ExtractPageId(item);
                            if (!string.IsNullOrEmpty(pageId) && pageId != "0")
                            {
                                var intent = new Intent(activity, typeof(PageProfileActivity));
                                intent.PutExtra("PageId", pageId);
                                activity.StartActivity(intent);
                                break;
                            }

                            // 5. Check event
                            if (!string.IsNullOrEmpty(item.EventId) && item.EventId != "0")
                            {
                                var intent = new Intent(activity, typeof(EventViewActivity));
                                intent.PutExtra("EventId", item.EventId);
                                activity.StartActivity(intent);
                                break;
                            }

                            // 6. Fallback to notifier's profile
                            WoWonderTools.OpenProfile(activity, item.Notifier?.UserId ?? item.NotifierId, string.IsNullOrEmpty(item.Notifier?.Username) ? null : item.Notifier);
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #region Announcement

        private void AnnouncementImageOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(TextAnnouncement))
                    OpenDialogAnnouncement(TextAnnouncement);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void OpenDialogAnnouncement(string textAnnouncement)
        {
            try
            {
                Dialog mAlertDialog = new Dialog(Context);
                mAlertDialog.RequestWindowFeature((int)WindowFeatures.NoTitle); // before
                mAlertDialog.SetContentView(Resource.Layout.DialogAnnouncement);
                mAlertDialog.SetCancelable(false);
                mAlertDialog.Window.SetBackgroundDrawable(new ColorDrawable(Color.Transparent));

                var subTitle = mAlertDialog?.FindViewById<SuperTextView>(Resource.Id.text);
                TextSanitizer headlineSanitizer = new TextSanitizer(subTitle, Activity);
                headlineSanitizer.Load(Methods.FunString.DecodeString(textAnnouncement));

                ImageView closeButton = mAlertDialog.FindViewById<ImageView>(Resource.Id.CloseButton);

                closeButton.Click += (sender, args) =>
                {
                    try
                    {
                        mAlertDialog.Hide();
                        mAlertDialog.Dismiss();
                    }
                    catch (Exception e)
                    {
                        Methods.DisplayReportResultTrack(e);
                    }
                };

                mAlertDialog.Show();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        #endregion
    }
}