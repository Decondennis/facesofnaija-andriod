using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Ads.DoubleClick;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonder.Helpers.Utils;
using WoWonderClient.Classes.Page;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.Communities.Pages
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class CreatePageActivity : BaseActivity
    {
        private EditText EtPageTitle, EtPageName, EtDescription;
        private Spinner SpinnerCategory;
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
                SetContentView(Resource.Layout.CreatePageLayout);

                InitComponent();
                InitToolbar();
                InitBackPressed("CreatePageActivity");
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void InitComponent()
        {
            EtPageTitle = FindViewById<EditText>(Resource.Id.et_page_title);
            EtPageName = FindViewById<EditText>(Resource.Id.et_page_name);
            EtDescription = FindViewById<EditText>(Resource.Id.et_page_description);
            SpinnerCategory = FindViewById<Spinner>(Resource.Id.spinner_category);
            BtnCreate = FindViewById<Button>(Resource.Id.btn_create);
            PublisherAdView = FindViewById<PublisherAdView>(Resource.Id.multiple_ad_sizes_view);

            LoadCategories();
            BtnCreate.Click += BtnCreate_Click;
            AdsGoogle.InitPublisherAdView(PublisherAdView);
        }

        private async void LoadCategories()
        {
            try
            {
                if (CategoriesController.ListCategoriesPage.Count == 0)
                    await ApiRequest.GetSettings_Api(this);

                CategoryNames.Clear();
                CategoryIds.Clear();
                foreach (var cat in CategoriesController.ListCategoriesPage)
                {
                    CategoryNames.Add(cat.CategoriesName);
                    CategoryIds.Add(cat.CategoriesId);
                }

                if (CategoryNames.Count == 0)
                    CategoryNames.AddRange(new[] { "Business", "Company", "Artist", "Brand", "Entertainment", "Food & Drink", "Health", "Hotel", "News", "Non-Profit", "Organization", "Public Figure", "Real Estate", "School", "Shopping", "Sports", "Technology", "Website" });

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
                    toolBar.Title = GetText(Resource.String.Lbl_Create_New_Page);
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
                var title = EtPageTitle.Text?.Trim() ?? "";
                var pageName = EtPageName?.Text?.Trim() ?? "";
                var description = EtDescription.Text?.Trim() ?? "";
                var catIndex = SpinnerCategory.SelectedItemPosition;

                if (string.IsNullOrEmpty(title))
                { ToastUtils.ShowToast(this, "Please enter a page name", ToastLength.Short); return; }

                // Auto-slugify page name
                pageName = System.Text.RegularExpressions.Regex.Replace(pageName, @"[^\w]", "_");
                pageName = System.Text.RegularExpressions.Regex.Replace(pageName, @"_+", "_");
                pageName = pageName.Trim('_');
                if (string.IsNullOrEmpty(pageName))
                {
                    pageName = System.Text.RegularExpressions.Regex.Replace(title, @"[^\w]", "_");
                    pageName = System.Text.RegularExpressions.Regex.Replace(pageName, @"_+", "_");
                    pageName = pageName.Trim('_');
                }
                if (pageName.Length < 5) pageName = pageName.PadRight(5, '_');
                if (pageName.Length > 32) pageName = pageName.Substring(0, 32);

                var categoryId = catIndex >= 0 && catIndex < CategoryIds.Count ? CategoryIds[catIndex] : "1";

                if (!Methods.CheckConnectivity())
                { ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short); return; }

                ProgressDialogHelper.Show(this, GetString(Resource.String.Lbl_Loading) + "...");
                var (apiStatus, respond) = await RequestsAsync.Page.CreatePageAsync(pageName, title, categoryId, description);
                ProgressDialogHelper.Dismiss(this);

                if (apiStatus == 200 && respond is CreatePageObject result)
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);
                    Intent returnIntent = new Intent();
                    if (result.PageData != null)
                        returnIntent?.PutExtra("pageItem", JsonConvert.SerializeObject(result.PageData));
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
