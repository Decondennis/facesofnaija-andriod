using System;
using System.Collections.Generic;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using AndroidX.AppCompat.Content.Res;
using Facesofnaija.Activities.Base;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Utils;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RequestCommunityActivity : BaseActivity
    {
        private TextView TxtAdd;
        private EditText TxtCommunityTitle, TxtCommunityUrl, TxtDescription, TxtCategory, TxtReason;
        private Spinner PrivacySpinner;
        private string PrivacyStatus = "1";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                // Create your application here
                SetContentView(Resource.Layout.CreateCommunityLayout);

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
                TxtAdd.Click += TxtAddOnClick;
                PrivacySpinner.ItemSelected += PrivacySpinnerOnItemSelected;
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
                TxtAdd.Click -= TxtAddOnClick;
                PrivacySpinner.ItemSelected -= PrivacySpinnerOnItemSelected;
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

        private void InitComponent()
        {
            try
            {
                TxtAdd = FindViewById<TextView>(Resource.Id.toolbar_title);

                TxtCommunityTitle = FindViewById<EditText>(Resource.Id.CommunityTitleText);
                TxtCommunityUrl = FindViewById<EditText>(Resource.Id.CommunityUrlText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionText);
                TxtCategory = FindViewById<EditText>(Resource.Id.CategoryText);
                TxtReason = FindViewById<EditText>(Resource.Id.ReasonText);
                PrivacySpinner = FindViewById<Spinner>(Resource.Id.PrivacySpinner);

                TxtAdd.Text = GetString(Resource.String.Lbl_SubmitRequest);

                Methods.SetColorEditText(TxtCommunityTitle, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCommunityUrl, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtCategory, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtReason, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                var privacyItems = new List<string>
                {
                    GetString(Resource.String.Radio_Public),
                    GetString(Resource.String.Radio_Private)
                };

                var adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, privacyItems);
                PrivacySpinner.Adapter = adapter;
                PrivacySpinner.SetSelection(0);
                PrivacyStatus = "1";
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

        private void DestroyBasic()
        {
            try
            {
                TxtAdd = null!;
                TxtCommunityTitle = null!;
                TxtCommunityUrl = null!;
                TxtDescription = null!;
                TxtCategory = null!;
                TxtReason = null!;
                PrivacySpinner = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PrivacySpinnerOnItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            PrivacyStatus = e.Position == 1 ? "2" : "1";
        }

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
                    if (string.IsNullOrWhiteSpace(TxtCommunityTitle.Text) ||
                        string.IsNullOrWhiteSpace(TxtDescription.Text) ||
                        string.IsNullOrWhiteSpace(TxtCategory.Text) ||
                        string.IsNullOrWhiteSpace(TxtReason.Text))
                    {
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                        return;
                    }

                    var requestedUrl = string.IsNullOrWhiteSpace(TxtCommunityUrl.Text) ? TxtCommunityTitle.Text : TxtCommunityUrl.Text;
                    var communityName = BuildCommunityName(requestedUrl);

                    if (communityName.Length < 5)
                    {
                        ToastUtils.ShowToast(this, "Community URL must be at least 5 characters.", ToastLength.Short);
                        return;
                    }

                    //Show a progress
                    ProgressDialogHelper.Show(this, GetText(Resource.String.Lbl_Loading));

                    var dictionary = new Dictionary<string, string>
                    {
                        {"name", communityName},
                        {"country", TxtCategory.Text},
                        {"state", TxtCommunityTitle.Text},
                        {"about", TxtDescription.Text},
                        {"privacy", PrivacyStatus},
                        {"lga", TxtReason.Text},

                        // Web-form parity fields
                        {"community_title", TxtCommunityTitle.Text},
                        {"community_name", communityName},
                        {"category", TxtCategory.Text},
                        {"reason", TxtReason.Text},
                    };

                    var (apiStatus, respond) = await CustomRequests.RequestCommunity(dictionary);

                    ProgressDialogHelper.Dismiss(this);

                    if (apiStatus == 200)
                    {
                        ToastUtils.ShowToast(this, "Community request submitted successfully.", ToastLength.Short);
                        Finish();
                    } else{
                        ToastUtils.ShowToast(this, "Error while submitting request.", ToastLength.Short);
                    }
                }
            }
            catch (Exception exception)
            {
                ProgressDialogHelper.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string BuildCommunityName(string source)
        {
            var value = source?.Trim().ToLowerInvariant() ?? string.Empty;
            value = value.Replace(" ", "-");

            var chars = new List<char>(value.Length);
            foreach (var ch in value)
            {
                if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_' || ch == '-')
                    chars.Add(ch);
            }

            return new string(chars.ToArray());
        }

    }
}