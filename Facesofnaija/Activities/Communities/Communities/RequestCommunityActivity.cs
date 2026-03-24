using System;
using System.Collections.Generic;
//using MaterialDialogsCore;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidHUD;
using AndroidX.AppCompat.Content.Res;
//using TheArtOfDev.Edmodo.Cropper;
using Java.IO;
using Facesofnaija.Activities.Base;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Utils;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RequestCommunityActivity : BaseActivity//, MaterialDialog.IListCallback
    {
        #region Variables Basic

        private TextView TxtAdd; 
        private EditText TxtName, TxtCountry, TxtState, TxtLga, TxtDescription, TxtPrivacy;
        private string TypeDialog, PrivacyStatus;
        private PublisherAdView PublisherAdView;
        private TabbedMainActivity GlobalContextTabbed;

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
                SetContentView(Resource.Layout.CreateCommunityLayout);

                GlobalContextTabbed = TabbedMainActivity.GetInstance();

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
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
                //AddOrRemoveEvent(true);
                //PublisherAdView?.Resume();
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
                //AddOrRemoveEvent(false);
                //PublisherAdView?.Pause();
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
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);

                TxtName = FindViewById<EditText>(Resource.Id.NameText);
                TxtCountry = FindViewById<EditText>(Resource.Id.CountryText); 
                TxtState = FindViewById<EditText>(Resource.Id.StateText);
                TxtLga = FindViewById<EditText>(Resource.Id.LgaText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionText);
                TxtPrivacy = FindViewById<EditText>(Resource.Id.PrivacyText);

                PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);
                AdsGoogle.InitPublisherAdView(PublisherAdView);
                    
                Methods.SetColorEditText(TxtName, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCountry, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtState, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtLga, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtPrivacy, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                Methods.SetFocusable(TxtPrivacy);
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
                    toolBar.Title = GetString(Resource.String.Lbl_Request_Community);
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

        /*private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        TxtAdd.Click += TxtAddOnClick;
                        TxtPrivacy.Touch += TxtPrivacyOnTouch;
                        break;
                    default:
                        TxtAdd.Click -= TxtAddOnClick;
                        TxtPrivacy.Touch -= TxtPrivacyOnTouch;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }*/

        private void DestroyBasic()
        {
            try
            {
                PublisherAdView?.Destroy();

                TxtAdd = null!;
                TxtName = null!;
                TxtCountry = null!;
                TxtDescription = null!;
                TxtState = null!;
                TxtLga = null!;
                TxtPrivacy = null!;

                //PublisherAdView = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
      
        #endregion

        #region Events
         
                 
        /*private void TxtPrivacyOnTouch(object sender, View.TouchEventArgs e)
        {
            try
            {
                if (e?.Event?.Action != MotionEventActions.Down) return;

                TypeDialog = "Privacies";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialDialog.Builder(this).Theme(WoWonderTools.IsTabDark() ? MaterialDialogsTheme.Dark : MaterialDialogsTheme.Light);

                //arrayAdapter.Add(GetText(Resource.String.Lbl_All));

                arrayAdapter.Add(GetText(Resource.String.Radio_Public));
                arrayAdapter.Add(GetText(Resource.String.Radio_Private));

                dialogList.Title(GetText(Resource.String.Lbl_Privacy)).TitleColorRes(Resource.Color.primary);
                dialogList.Items(arrayAdapter);
                dialogList.NegativeText(GetText(Resource.String.Lbl_Close)).OnNegative(new WoWonderTools.MyMaterialDialog());
                dialogList.AlwaysCallSingleChoiceCallback();
                dialogList.ItemsCallback(this).Build().Show();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }*/
        
        //Save 
        private async void TxtAddOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
                else
                {    
                    if (string.IsNullOrEmpty(TxtName.Text) || string.IsNullOrWhiteSpace(TxtName.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }
                     
                    if (string.IsNullOrEmpty(TxtState.Text) || string.IsNullOrWhiteSpace(TxtState.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }
                     
                    if (string.IsNullOrEmpty(TxtDescription.Text) || string.IsNullOrWhiteSpace(TxtDescription.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }

                    if (string.IsNullOrEmpty(TxtCountry.Text) || string.IsNullOrWhiteSpace(TxtCountry.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }
                    
                    if (string.IsNullOrEmpty(TxtLga.Text) || string.IsNullOrWhiteSpace(TxtLga.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }
                    
                    if (string.IsNullOrEmpty(TxtPrivacy.Text) || string.IsNullOrWhiteSpace(TxtPrivacy.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    AndHUD.Shared.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", TxtName.Text},
                        {"country",TxtCountry.Text},
                        {"state",TxtState.Text}, 
                        {"about", TxtDescription.Text},
                        {"privacy", PrivacyStatus == "Private" ? "0" : "1"},
                        {"lga", TxtLga.Text},
                    };

                    var (apiStatus, respond) = await CustomRequests.RequestCommunity(dictionary);

                    AndHUD.Shared.Dismiss(this);

                    if (apiStatus == 200)
                    {
                        ToastUtils.ShowToast(this, "Community request submitted successfully.", ToastLength.Short);
                    } else{
                        ToastUtils.ShowToast(this, "Error while submitting request.", ToastLength.Short);
                    }

                    DestroyBasic();
                }
            }
            catch (Exception exception)
            {
                AndHUD.Shared.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion


        #region MaterialDialog

        /*public void OnSelection(MaterialDialog dialog, View itemView, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "Privacies" when itemString == GetText(Resource.String.Radio_Private):
                        TxtPrivacy.Text = GetText(Resource.String.Radio_Private);
                        PrivacyStatus = GetText(Resource.String.Radio_Private);
                        break;
                    case "Privacies" when itemString == GetText(Resource.String.Radio_Public):
                        TxtPrivacy.Text = GetText(Resource.String.Radio_Public);
                        PrivacyStatus = GetText(Resource.String.Radio_Public);
                        break;
                    case "Privacies":
                        TxtPrivacy.Text = GetText(Resource.String.Radio_Public);
                        PrivacyStatus = GetText(Resource.String.Radio_Public);
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }*/

         
        #endregion

    }
}