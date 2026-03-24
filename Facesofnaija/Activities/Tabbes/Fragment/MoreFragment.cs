using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Google.Android.Material.Dialog; 
using System;
using System.Linq;
using Facesofnaija.Activities.Advertise;
using Facesofnaija.Activities.Album;
using Facesofnaija.Activities.Articles;
using Facesofnaija.Activities.Boosted;
using Facesofnaija.Activities.CommonThings;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Contacts;
using Facesofnaija.Activities.Covid19;
using Facesofnaija.Activities.Events;
using Facesofnaija.Activities.Fundings;
using Facesofnaija.Activities.Games;
using Facesofnaija.Activities.Jobs;
using Facesofnaija.Activities.Live.Page;
using Facesofnaija.Activities.Market;
using Facesofnaija.Activities.Memories;
using Facesofnaija.Activities.Movies;
using Facesofnaija.Activities.MyPhoto;
using Facesofnaija.Activities.MyProfile;
using Facesofnaija.Activities.MyVideo;
using Facesofnaija.Activities.NativePost.Pages;
using Facesofnaija.Activities.NearBy;
using Facesofnaija.Activities.Offers;
using Facesofnaija.Activities.Pokes;
using Facesofnaija.Activities.PopularPosts;
using Facesofnaija.Activities.SettingsPreferences.General;
using Facesofnaija.Activities.SettingsPreferences.InvitationLinks;
using Facesofnaija.Activities.SettingsPreferences.MyInformation;
using Facesofnaija.Activities.SettingsPreferences.Notification;
using Facesofnaija.Activities.SettingsPreferences.Privacy;
using Facesofnaija.Activities.SettingsPreferences.Support;
using Facesofnaija.Activities.SettingsPreferences.TellFriend;
using Facesofnaija.Activities.Tabbes.Adapters;
using Facesofnaija.Activities.Upgrade;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using Exception = System.Exception;
using Facesofnaija.Activities.Communities.Communities;

namespace Facesofnaija.Activities.Tabbes.Fragment
{
    public class MoreFragment : AndroidX.Fragment.App.Fragment
    {
        #region  Variables Basic

        public MoreSectionAdapter MoreSectionAdapter1, MoreSectionAdapter2;
        private RecyclerView MRecycler1, MRecycler2;
        private LinearLayout GoProLayout;
        private RelativeLayout profileLayout;
        private ImageView profileImage;
        private TextView profileName;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                View view = inflater.Inflate(Resource.Layout.TMoreLayout, container, false);
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

                InitComponent(view);
                SetRecyclerViewAdapters();
                LoadData();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                profileLayout = (RelativeLayout)view.FindViewById(Resource.Id.profileLayout);
                profileImage = (ImageView)view.FindViewById(Resource.Id.image);
                profileName = (TextView)view.FindViewById(Resource.Id.tv_name);
                profileLayout.Click += ProfileLayoutOnClick;

                GoProLayout = (LinearLayout)view.FindViewById(Resource.Id.GoProLayout);
                GoProLayout.Click += GoProLayoutOnClick;

                MRecycler1 = (RecyclerView)view.FindViewById(Resource.Id.recyler1);
                MRecycler2 = (RecyclerView)view.FindViewById(Resource.Id.recyler2);
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
                MRecycler1.NestedScrollingEnabled = true;

                if (AppSettings.MoreTheme == MoreTheme.Grid)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Grid);

                    var layoutManager = new GridLayoutManager(Activity, 4);
                    var countListFirstRow = MoreSectionAdapter1.SectionList.Where(q => q.StyleRow == StyleRowMore.Grid).ToList().Count;

                    layoutManager.SetSpanSizeLookup(new MySpanSizeLookup2(countListFirstRow, 1, 4));//20, 1, 4
                    MRecycler1.SetLayoutManager(layoutManager);
                }
                else if (AppSettings.MoreTheme == MoreTheme.Card)
                {
                    MoreSectionAdapter1 = new MoreSectionAdapter(Activity, StyleRowMore.Card);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.20f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    }
                    else
                    {
                        var layoutManager = new SpannedGridLayoutManager(new MySpannedGridLayoutManager(), 3, 1.12f);
                        MRecycler1.AddItemDecoration(new GridSpacingItemDecoration(3, 15, true));
                        MRecycler1.SetLayoutManager(layoutManager);
                    }
                }

                MoreSectionAdapter1.ItemClick += MoreSectionAdapter1OnItemClick;
                MRecycler1.SetAdapter(MoreSectionAdapter1);
                MRecycler1.HasFixedSize = true;
                MRecycler1.SetItemViewCacheSize(50);
                MRecycler1.GetLayoutManager().ItemPrefetchEnabled = true;

                //=================================


                MRecycler2.NestedScrollingEnabled = true;

                MoreSectionAdapter2 = new MoreSectionAdapter(Activity, StyleRowMore.Row);
                MoreSectionAdapter2.ItemClick += MoreSectionAdapter2OnItemClick;

                MRecycler2.SetLayoutManager(new LinearLayoutManager(Activity));
                MRecycler2.SetAdapter(MoreSectionAdapter2);
                MRecycler2.HasFixedSize = true;
                MRecycler2.SetItemViewCacheSize(50);
                MRecycler2.GetLayoutManager().ItemPrefetchEnabled = true;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private class MySpannedGridLayoutManager : SpannedGridLayoutManager.IGridSpanLookup
        {
            private bool Big1 = true;
            private bool Big2 = true;

            public SpannedGridLayoutManager.SpanInfo GetSpanInfo(int position)
            {
                try
                {
                    // Conditions for 2x2 items 
                    if (position == 0)
                    {
                        return new SpannedGridLayoutManager.SpanInfo(3, 1);
                    }

                    if (position % 2 == 0)
                    {
                        if (Big1)
                        {
                            Big1 = false;
                            return new SpannedGridLayoutManager.SpanInfo(2, 1);
                        }
                        else
                        {
                            Big1 = true;
                            return new SpannedGridLayoutManager.SpanInfo(1, 1);
                        }
                    }
                    else
                    {
                        if (Big2)
                        {
                            Big2 = false;
                            return new SpannedGridLayoutManager.SpanInfo(1, 1);
                        }
                        else
                        {
                            Big2 = true;
                            return new SpannedGridLayoutManager.SpanInfo(2, 1);
                        }
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                    return new SpannedGridLayoutManager.SpanInfo(2, 1);
                }
            }
        }

        #endregion

        #region Event

        private void GoProLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(GoProActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ProfileLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                var intent = new Intent(Context, typeof(MyProfileActivity));
                StartActivity(intent);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreSectionAdapter1OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter1?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    {
                        // My Profile
                        case 1:
                            {
                                var intent = new Intent(Context, typeof(MyProfileActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Messages
                        case 2:
                            {
                                Methods.App.OpenAppByPackageName(Context, AppSettings.MessengerPackageName, "OpenChatApp");
                                break;
                            }
                        // Contacts
                        case 3:
                            {
                                var intent = new Intent(Context, typeof(MyContactsActivity));
                                intent.PutExtra("ContactsType", "Following");
                                intent.PutExtra("UserId", UserDetails.UserId);
                                StartActivity(intent);
                                break;
                            }
                        // Pokes
                        case 4:
                            {
                                var intent = new Intent(Context, typeof(PokesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Album
                        case 5:
                            {
                                var intent = new Intent(Context, typeof(MyAlbumActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyImages
                        case 6:
                            {
                                var intent = new Intent(Context, typeof(MyPhotosActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyVideos
                        case 7:
                            {
                                var intent = new Intent(Context, typeof(MyVideoActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Saved Posts
                        case 8:
                            {
                                var intent = new Intent(Context, typeof(SavedPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Groups
                        case 9:
                            {
                                var intent = new Intent(Context, typeof(GroupsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Communities
                        case 60:
                            {
                                var intent = new Intent(Context, typeof(CommunitiesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Pages
                        case 10:
                            {
                                var intent = new Intent(Context, typeof(PagesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Blogs
                        case 11:
                            StartActivity(new Intent(Context, typeof(ArticlesActivity)));
                            break;
                        // Market
                        case 12:
                            StartActivity(new Intent(Context, typeof(TabbedMarketActivity)));
                            break;
                        // Boosted Posts & Pages
                        case 13:
                            {
                                var intent = new Intent(Context, typeof(BoostedActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Popular Posts
                        case 14:
                            {
                                var intent = new Intent(Context, typeof(PopularPostsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Events
                        case 15:
                            {
                                var intent = new Intent(Context, typeof(EventMainActivity));
                                StartActivity(intent);
                                break;
                            }
                        // NearBy Find Friends
                        case 16:
                            {
                                var intent = new Intent(Context, typeof(PeopleNearByActivity));
                                StartActivity(intent);
                                break;
                            }
                        //Offers
                        case 17:
                            {
                                var intent = new Intent(Context, typeof(OffersActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Movies
                        case 18:
                            {
                                var intent = new Intent(Context, typeof(MoviesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // jobs
                        case 19:
                            {
                                var intent = new Intent(Context, typeof(JobsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // common things
                        case 20:
                            {
                                var intent = new Intent(Context, typeof(CommonThingsActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Memories
                        case 21:
                            {
                                var intent = new Intent(Context, typeof(MemoriesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Funding
                        case 22:
                            {
                                var intent = new Intent(Context, typeof(FundingActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Games
                        case 23:
                            {
                                var intent = new Intent(Context, typeof(GamesActivity));
                                StartActivity(intent);
                                break;
                            }
                        // CoronaVirus
                        case 24:
                            {
                                var intent = new Intent(Context, typeof(Covid19Activity));
                                StartActivity(intent);
                                break;
                            }
                        // Live
                        case 25:
                            {
                                var intent = new Intent(Context, typeof(LiveActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Advertising
                        case 26:
                            {
                                var intent = new Intent(Context, typeof(MyAdvertiseActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Go pro
                        case 27:
                            {
                                var intent = new Intent(Context, typeof(GoProActivity));
                                StartActivity(intent);
                                break;
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void MoreSectionAdapter2OnItemClick(object sender, MoreSectionAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                var item = MoreSectionAdapter2?.GetItem(position);
                if (item != null)
                {
                    switch (item.Id)
                    {
                        // General Account
                        case 100:
                            {
                                var intent = new Intent(Context, typeof(GeneralAccountActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Privacy
                        case 101:
                            {
                                var intent = new Intent(Context, typeof(PrivacyActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Notification
                        case 102:
                            {
                                var intent = new Intent(Context, typeof(MessegeNotificationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // InvitationLinks
                        case 103:
                            {
                                var intent = new Intent(Context, typeof(InvitationLinksActivity));
                                StartActivity(intent);
                                break;
                            }
                        // MyInformation
                        case 104:
                            {
                                var intent = new Intent(Context, typeof(MyInformationActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Tell Friends
                        case 105:
                            {
                                var intent = new Intent(Context, typeof(TellFriendActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Help & Support
                        case 106:
                            {
                                var intent = new Intent(Context, typeof(SupportActivity));
                                StartActivity(intent);
                                break;
                            }
                        // Logout
                        case 107:
                        {
                            var dialog = new MaterialAlertDialogBuilder(Context);//.ba(WoWonderTools.IsTabDark() ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                            dialog.SetTitle(Resource.String.Lbl_Warning);
                            dialog.SetMessage(Context.GetText(Resource.String.Lbl_Are_you_logout));
                            dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Ok), (o, args) =>
                            {
                                try
                                {
                                    ToastUtils.ShowToast(Activity, Activity.GetText(Resource.String.Lbl_You_will_be_logged), ToastLength.Long);
                                    ApiRequest.Logout(Activity);
                                }
                                catch (Exception exception)
                                {
                                    Methods.DisplayReportResultTrack(exception);
                                }
                            });
                            dialog.SetNegativeButton(Context.GetText(Resource.String.Lbl_Cancel), new MaterialDialogUtils());
                            dialog.Show();
                            break;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion
        
        private void LoadData()
        {
            try
            {

                var myProfile = ListUtils.MyProfileList?.FirstOrDefault();
                GlideImageLoader.LoadImage(Activity, myProfile != null ? myProfile.Avatar : UserDetails.Avatar, profileImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                profileName.Text = WoWonderTools.GetNameFinal(myProfile);

                GoProLayout.Visibility = AppSettings.ShowGoPro ? ViewStates.Visible : ViewStates.Gone;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }
    }
}