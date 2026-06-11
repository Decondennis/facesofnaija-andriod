using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Group;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Groups
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreateGroupActivity : BaseActivity
    {
        private EditText EtGroupTitle, EtGroupUrl, EtDescription;
        private Spinner SpinnerCategory, SpinnerPrivacy;
        private Button BtnCreate;
        private PublisherAdView PublisherAdView;
        private List<string> CategoryNames = new List<string>();
        private List<string> CategoryIds = new List<string>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);
                Methods.App.FullScreenApp(this);
                SetContentView(Resource.Layout.CreateGroupLayout);

                InitComponent();
                InitToolbar();
                InitBackPressed("CreateGroupActivity");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            EtGroupTitle = FindViewById<EditText>(Resource.Id.et_step12);
            EtGroupUrl = FindViewById<EditText>(Resource.Id.et_step3);
            EtDescription = FindViewById<EditText>(Resource.Id.et_description);
            SpinnerCategory = FindViewById<Spinner>(Resource.Id.spinner_category);
            SpinnerPrivacy = FindViewById<Spinner>(Resource.Id.spinner_privacy);
            BtnCreate = FindViewById<Button>(Resource.Id.btn_next);
            PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);

            // Privacy options
            var privacyList = new List<string> { "Public", "Private" };
            var privacyAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, privacyList);
            privacyAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
            SpinnerPrivacy.Adapter = privacyAdapter;

            // Load categories
            LoadCategories();

            BtnCreate.Click += BtnCreate_Click;
            AdsGoogle.InitPublisherAdView(PublisherAdView);
        }

        private async void LoadCategories()
        {
            try
            {
                if (CategoriesController.ListCategoriesGroup.Count == 0)
                    await ApiRequest.GetSettings_Api(this);

                CategoryNames.Clear();
                CategoryIds.Clear();
                foreach (var cat in CategoriesController.ListCategoriesGroup)
                {
                    CategoryNames.Add(cat.CategoriesName);
                    CategoryIds.Add(cat.CategoriesId);
                }

                if (CategoryNames.Count == 0)
                    CategoryNames.AddRange(new[] { "General", "Education", "Entertainment", "Music", "Sports", "Technology", "Business", "Gaming", "Health", "News", "Travel", "Art", "Photography", "Fashion", "Food", "Science", "Charity", "Other" });

                RunOnUiThread(() =>
                {
                    var catAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, CategoryNames);
                    catAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    SpinnerCategory.Adapter = catAdapter;
                });
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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Group);
                    SetSupportActionBar(toolBar);
                    SupportActionBar.SetDisplayShowCustomEnabled(true);
                    SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                    SupportActionBar.SetHomeButtonEnabled(true);
                    SupportActionBar.SetDisplayShowHomeEnabled(true);
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            try
            {
                var title = EtGroupTitle.Text?.Trim() ?? "";
                var url = EtGroupUrl?.Text?.Trim() ?? "";
                var description = EtDescription.Text?.Trim() ?? "";
                var catIndex = SpinnerCategory.SelectedItemPosition;
                var privacyIndex = SpinnerPrivacy.SelectedItemPosition;

                if (string.IsNullOrEmpty(title))
                { ToastUtils.ShowToast(this, "Please enter a group name", ToastLength.Short); return; }

                // Auto-slugify URL
                url = System.Text.RegularExpressions.Regex.Replace(url, @"[^\w]", "_");
                url = System.Text.RegularExpressions.Regex.Replace(url, @"_+", "_");
                url = url.Trim('_');
                if (string.IsNullOrEmpty(url))
                {
                    url = System.Text.RegularExpressions.Regex.Replace(title, @"[^\w]", "_");
                    url = System.Text.RegularExpressions.Regex.Replace(url, @"_+", "_");
                    url = url.Trim('_');
                }
                if (url.Length < 5) url = url.PadRight(5, '_');
                if (url.Length > 32) url = url.Substring(0, 32);

                var categoryId = catIndex >= 0 && catIndex < CategoryIds.Count ? CategoryIds[catIndex] : "1";
                var privacy = privacyIndex == 0 ? "1" : "2";

                if (!Methods.CheckConnectivity())
                { ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short); return; }

                ProgressDialogHelper.Show(this, GetString(Resource.String.Lbl_Loading) + "...");
                var (apiStatus, respond) = await RequestsAsync.Group.CreateGroupAsync(url, title, description, categoryId, privacy);
                ProgressDialogHelper.Dismiss(this);

                if (apiStatus == 200 && respond is CreateGroupObject result)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);
                    Intent returnIntent = new Intent();
                    if (result.GroupData != null)
                        returnIntent?.PutExtra("groupItem", JsonConvert.SerializeObject(result.GroupData));
                    SetResult(Result.Ok, returnIntent);
                    Finish();
                }
                else
                {
                    Methods.DisplayAndHudErrorResult(this, respond);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                ProgressDialogHelper.Dismiss(this);
            }
        }

        public void BackPressed()
        {
            Finish();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home) Finish();
            return base.OnOptionsItemSelected(item);
        }
    }
}
