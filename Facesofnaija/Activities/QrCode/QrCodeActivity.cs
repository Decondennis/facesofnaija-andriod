using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using AndroidX.AppCompat.Content.Res;
using AndroidX.ViewPager2.Widget;
using Google.Android.Material.Tabs;
using System;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.QrCode.Fragment;
using Facesofnaija.Adapters;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Utils;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.QrCode
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class QrCodeActivity : BaseActivity, TabLayoutMediator.ITabConfigurationStrategy
    {
        #region Variables Basic

        private MainTabAdapter Adapter;
        private ViewPager2 ViewPager;
        private QrScannerFragment QrScannerTab;
        private MyQrCodeFragment MyQrCodeTab;
        private TabLayout TabLayout;
        private Toolbar ToolBar;

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
                SetContentView(Resource.Layout.GamesMain_Layout);

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();

                AdsGoogle.Ad_RewardedVideo(this);
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
                ViewPager = FindViewById<ViewPager2>(Resource.Id.viewpager);
                TabLayout = FindViewById<TabLayout>(Resource.Id.tabs);

                ViewPager.OffscreenPageLimit = 2;
                SetUpViewPager(ViewPager);
                new TabLayoutMediator(TabLayout, ViewPager, this).Attach();
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
                ToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (ToolBar != null)
                {
                    ToolBar.Title = " ";
                    ToolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(ToolBar);
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

        private void DestroyBasic()
        {
            try
            {
                ViewPager = null!;
                TabLayout = null!;
                ToolBar = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Set Tap

        private void SetUpViewPager(ViewPager2 viewPager)
        {
            try
            {
                MyQrCodeTab = new MyQrCodeFragment();
                QrScannerTab = new QrScannerFragment();

                Adapter = new MainTabAdapter(this);
                Adapter.AddFragment(MyQrCodeTab, GetText(Resource.String.Lbl_MyCode));
                Adapter.AddFragment(QrScannerTab, GetText(Resource.String.Lbl_ScanCode));

                viewPager.CurrentItem = Adapter.ItemCount;
                viewPager.OffscreenPageLimit = Adapter.ItemCount;

                viewPager.Orientation = ViewPager2.OrientationHorizontal;
                viewPager.RegisterOnPageChangeCallback(new MyOnPageChangeCallback(this));

                viewPager.Adapter = Adapter;
                viewPager.Adapter.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public void OnConfigureTab(TabLayout.Tab tab, int position)
        {
            try
            {
                tab.SetText(Adapter.GetFragment(position));
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private class MyOnPageChangeCallback : ViewPager2.OnPageChangeCallback
        {
            private readonly QrCodeActivity Activity;

            public MyOnPageChangeCallback(QrCodeActivity activity)
            {
                try
                {
                    Activity = activity;
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }

            public override void OnPageSelected(int position)
            {
                try
                {
                    base.OnPageSelected(position);
                    var number = Activity.GetSelectedTab(position);
                    switch (number)
                    {
                        //MyCode
                        case 1:

                            break;
                        //ScanCode
                        case 2:
                            Activity.QrScannerTab?.RequestCamera();
                            break;
                    }
                }
                catch (Exception exception)
                {
                    Methods.DisplayReportResultTrack(exception);
                }
            }
        }

        private int GetSelectedTab(int number)
        {
            try
            {
                var tabName = TabLayout.GetTabAt(number).Text;
                if (tabName == Resources.GetText(Resource.String.Lbl_MyCode))
                {
                    return 1;
                }
                if (tabName == Resources.GetText(Resource.String.Lbl_ScanCode))
                {
                    return 2;
                }

                return 0;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return 0;
            }
        }

        #endregion

    }
}