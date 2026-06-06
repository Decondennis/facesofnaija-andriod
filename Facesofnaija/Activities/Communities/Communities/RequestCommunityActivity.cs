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
using AndroidX.AppCompat.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Communities
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class RequestCommunityActivity : BaseActivity
    {
        private AppCompatButton SubmitButton;
        private EditText TxtCommunityTitle, TxtDescription, TxtReason;
        private Spinner PrivacySpinner, CategorySpinner;
        private string PrivacyStatus = "1";
        private string SelectedCategory = "1";

        private static readonly Dictionary<string, string> Categories = new Dictionary<string, string>
        {
            {"1", "Other"},
            {"2", "Education"},
            {"3", "Entertainment"},
            {"4", "Music"},
            {"5", "Business"},
            {"6", "Sports"},
            {"7", "News"},
            {"8", "Technology"},
            {"9", "Fashion"},
            {"10", "Art"},
            {"11", "Travel"},
            {"12", "Food"},
            {"13", "Health"},
            {"14", "Religion"},
            {"15", "Family"},
            {"16", "Gaming"},
        };

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.CreateCommunityLayout);

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
                SubmitButton.Click += SubmitButtonOnClick;
                PrivacySpinner.ItemSelected += PrivacySpinnerOnItemSelected;
                CategorySpinner.ItemSelected += CategorySpinnerOnItemSelected;
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
                SubmitButton.Click -= SubmitButtonOnClick;
                PrivacySpinner.ItemSelected -= PrivacySpinnerOnItemSelected;
                CategorySpinner.ItemSelected -= CategorySpinnerOnItemSelected;
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
                TxtCommunityTitle = FindViewById<EditText>(Resource.Id.CommunityTitleText);
                TxtDescription = FindViewById<EditText>(Resource.Id.DescriptionText);
                TxtReason = FindViewById<EditText>(Resource.Id.ReasonText);
                PrivacySpinner = FindViewById<Spinner>(Resource.Id.PrivacySpinner);
                CategorySpinner = FindViewById<Spinner>(Resource.Id.CategorySpinner);
                SubmitButton = FindViewById<AppCompatButton>(Resource.Id.SubmitButton);

                Methods.SetColorEditText(TxtCommunityTitle, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtDescription, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                Methods.SetColorEditText(TxtReason, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                var privacyItems = new List<string>
                {
                    GetString(Resource.String.Radio_Public),
                    GetString(Resource.String.Radio_Private)
                };
                var privacyAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, privacyItems);
                PrivacySpinner.Adapter = privacyAdapter;
                PrivacySpinner.SetSelection(0);

                var categoryNames = new List<string>(Categories.Values);
                var categoryAdapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerDropDownItem, categoryNames);
                CategorySpinner.Adapter = categoryAdapter;
                CategorySpinner.SetSelection(0);
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
                SubmitButton = null!;
                TxtCommunityTitle = null!;
                TxtDescription = null!;
                TxtReason = null!;
                PrivacySpinner = null!;
                CategorySpinner = null!;
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

        private void CategorySpinnerOnItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            var keys = new List<string>(Categories.Keys);
            SelectedCategory = e.Position < keys.Count ? keys[e.Position] : "1";
        }

        private async void SubmitButtonOnClick(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtCommunityTitle.Text) ||
                    string.IsNullOrWhiteSpace(TxtDescription.Text) ||
                    string.IsNullOrWhiteSpace(TxtReason.Text))
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_All_fields_are_required), ToastLength.Short);
                    return;
                }

                var communityName = BuildCommunityName(TxtCommunityTitle.Text);

                if (communityName.Length < 5)
                {
                    ToastUtils.ShowToast(this, "Community URL must be at least 5 characters.", ToastLength.Short);
                    return;
                }

                SubmitButton.Enabled = false;
                ProgressDialogHelper.Show(this, GetText(Resource.String.Lbl_Loading));

                var dictionary = new Dictionary<string, string>
                {
                    {"community_name", communityName},
                    {"community_title", TxtCommunityTitle.Text.Trim()},
                    {"about", TxtDescription.Text.Trim()},
                    {"category", SelectedCategory},
                    {"reason", TxtReason.Text.Trim()},
                    {"privacy", PrivacyStatus},
                    {"server_key", ""},
                };

                var (apiStatus, respond) = await CustomRequests.RequestCommunity(dictionary);

                ProgressDialogHelper.Dismiss(this);
                SubmitButton.Enabled = true;

                if (apiStatus == 200)
                {
                    ToastUtils.ShowToast(this, "Community request submitted successfully.", ToastLength.Short);
                    Finish();
                }
                else
                {
                    ToastUtils.ShowToast(this, "Error while submitting request.", ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                ProgressDialogHelper.Dismiss(this);
                SubmitButton.Enabled = true;
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
