using System;
using System.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.Graphics;
using Android.OS;


using Android.Views;
using AndroidX.AppCompat.Content.Res;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Newtonsoft.Json;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.Communities.Adapters;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Helpers.Model;
using WoWonderClient.Classes.Global;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.Activities.Communities.Communities.Settings
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SettingsCommunityActivity : BaseActivity
    {
        #region Variables Basic

        private SettingsAdapter MAdapter;
        private SwipeRefreshLayout SwipeRefreshLayout;
        private RecyclerView MRecycler;
        private LinearLayoutManager LayoutManager;
        private ViewStub EmptyStateLayout;
        private AdView MAdView;
        private string CommunityId;
        private CommunityDataObject CommunityDataClass;


        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);

                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                // Create your application here
                SetContentView(Resource.Layout.RecyclerDefaultLayout);

                CommunityId = Intent?.GetStringExtra("CommunityId");

                if (!string.IsNullOrEmpty(Intent?.GetStringExtra("itemObject")))
                    CommunityDataClass = JsonConvert.DeserializeObject<CommunityDataObject>(Intent?.GetStringExtra("itemObject") ?? "");

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();
                AdsGoogle.Ad_RewardedVideo(this);
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
                MAdView?.Resume();

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
                MAdView?.Pause();
                base.OnPause();
                AddOrRemoveEvent(false);

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
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
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

                EmptyStateLayout.Visibility = ViewStates.Gone;

                SwipeRefreshLayout = (SwipeRefreshLayout)FindViewById(Resource.Id.swipeRefreshLayout);
                SwipeRefreshLayout.SetColorSchemeResources(Android.Resource.Color.HoloBlueLight, Android.Resource.Color.HoloGreenLight, Android.Resource.Color.HoloOrangeLight, Android.Resource.Color.HoloRedLight);
                SwipeRefreshLayout.Refreshing = false;
                SwipeRefreshLayout.Enabled = false;
                SwipeRefreshLayout.SetProgressBackgroundColorSchemeColor(WoWonderTools.IsTabDark() ? Color.ParseColor("#424242") : Color.ParseColor("#f7f7f7"));


                MAdView = FindViewById<AdView>(Resource.Id.adView);
                AdsGoogle.InitAdView(MAdView, MRecycler);
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
                MAdapter = new SettingsAdapter(this, "Community", null);
                LayoutManager = new LinearLayoutManager(this);
                MRecycler.SetLayoutManager(LayoutManager);
                MRecycler.HasFixedSize = true;
                MRecycler.SetItemViewCacheSize(10);
                MRecycler.GetLayoutManager().ItemPrefetchEnabled = true;
                MRecycler.SetAdapter(MAdapter);
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
                    toolBar.Title = GetText(Resource.String.Lbl_Settings);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        MAdapter.ItemClick += MAdapterOnItemClick;
                        break;
                    default:
                        MAdapter.ItemClick -= MAdapterOnItemClick;
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
                CommunityId = null!;
                CommunityDataClass = null!;
                MAdView = null!;

            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Events

        private void MAdapterOnItemClick(object sender, SettingsAdapterClickEventArgs adapterClickEvents)
        {
            try
            {
                var position = adapterClickEvents.Position;
                switch (position)
                {
                    case >= 0:
                        {
                            var item = MAdapter.GetItem(position);
                            if (item != null)
                            {
                                switch (item.Id)
                                {
                                    // General
                                    case 1:
                                        {
                                            var intent = new Intent(this, typeof(CommunityGeneralActivity));
                                            intent.PutExtra("CommunityData", JsonConvert.SerializeObject(CommunityDataClass));
                                            intent.PutExtra("CommunityId", CommunityId);
                                            StartActivityForResult(intent, 1250);
                                            break;
                                        }
                                    //Privacy
                                    case 2:
                                        {
                                            var intent = new Intent(this, typeof(CommunityPrivacyActivity));
                                            intent.PutExtra("CommunityData", JsonConvert.SerializeObject(CommunityDataClass));
                                            intent.PutExtra("CommunityId", CommunityId);
                                            StartActivityForResult(intent, 1250);
                                            break;
                                        }
                                    //Members
                                    case 3:
                                        {
                                            var intent = new Intent(this, typeof(CommunityMembersActivity));
                                            intent.PutExtra("itemObject", JsonConvert.SerializeObject(CommunityDataClass));
                                            intent.PutExtra("CommunityId", CommunityId);
                                            StartActivity(intent);
                                            break;
                                        }
                                    //DeleteGroup
                                    case 4:
                                        {
                                            var intent = new Intent(this, typeof(DeleteCommunitiesActivity));
                                            intent.PutExtra("Id", CommunityId);
                                            intent.PutExtra("Type", "Community");
                                            StartActivityForResult(intent, 2019);
                                            break;
                                        }
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

        #region Result

        //Result
        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                switch (requestCode)
                {
                    //If its from Camera or Gallery
                    case 2019 when resultCode == Result.Ok:
                        {
                            var manged = CommunitiesActivity.GetInstance()?.MAdapter?.SocialList?.FirstOrDefault(a => a.Community?.CommunityId == CommunityId && a.TypeView == SocialModelType.ManagedCommunities);
                            if (manged?.Group != null)
                            {
                                CommunitiesActivity.GetInstance().MAdapter.SocialList.Remove(manged);
                                CommunitiesActivity.GetInstance().MAdapter.NotifyDataSetChanged();

                                ListUtils.MyCommunityList.Remove(manged?.Community);
                            }
                            Intent returnIntent = new Intent();
                            SetResult(Result.Ok, returnIntent);
                            Finish();
                            break;
                        }
                    case 1250 when resultCode == Result.Ok:
                        {
                            var communityItem = data.GetStringExtra("communityItem") ?? "";
                            if (string.IsNullOrEmpty(communityItem))
                            {
                                CommunityDataClass = JsonConvert.DeserializeObject<CommunityDataObject>(Intent?.GetStringExtra("communityItem") ?? "");
                                CommunityProfileActivity.CommunityDataClass = CommunityDataClass;
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
    }
}