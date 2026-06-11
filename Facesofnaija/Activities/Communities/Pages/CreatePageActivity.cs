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
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonder.Helpers.Utils;
using WoWonderClient;
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
                // Fallback immediately
                var fallbackNames = new List<string> { "Business", "Company", "Artist", "Brand", "Entertainment", "Food & Drink", "Health", "Hotel", "News", "Non-Profit", "Organization", "Public Figure", "Real Estate", "School", "Shopping", "Sports", "Technology", "Website" };
                var fallbackIds = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18" };
                RunOnUiThread(() =>
                {
                    var fbAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, fallbackNames);
                    fbAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    SpinnerCategory.Adapter = fbAdapter;
                });
                CategoryNames = new List<string>(fallbackNames);
                CategoryIds = new List<string>(fallbackIds);

                // Try SDK
                if (CategoriesController.ListCategoriesPage.Count > 0)
                {
                    ApplyCategories(CategoriesController.ListCategoriesPage);
                    return;
                }
                try { await ApiRequest.GetSettings_Api(this); } catch { }
                if (CategoriesController.ListCategoriesPage.Count > 0)
                {
                    ApplyCategories(CategoriesController.ListCategoriesPage);
                    return;
                }

                // Direct HTTP fallback for page_categories
                try
                {
                    string token = UserDetails.AccessToken ?? Current.AccessToken ?? "";
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var json = await client.GetStringAsync($"http://172.236.19.52/api-v2.php?type=get-site-settings&access_token={token}");
                    var obj = JObject.Parse(json);
                    var cats = obj["config"]?["page_categories"] as JObject;
                    if (cats != null)
                    {
                        var names = new List<string>();
                        var ids = new List<string>();
                        foreach (var prop in cats.Properties())
                        {
                            ids.Add(prop.Name);
                            names.Add(prop.Value.ToString());
                        }
                        RunOnUiThread(() =>
                        {
                            var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, names);
                            adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                            SpinnerCategory.Adapter = adapter;
                        });
                        CategoryNames = names;
                        CategoryIds = ids;
                    }
                }
                catch { }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ApplyCategories(ObservableCollection<Classes.Categories> list)
        {
            CategoryNames.Clear();
            CategoryIds.Clear();
            foreach (var cat in list)
            {
                CategoryNames.Add(cat.CategoriesName);
                CategoryIds.Add(cat.CategoriesId);
            }
            RunOnUiThread(() =>
            {
                var adapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, CategoryNames);
                adapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                SpinnerCategory.Adapter = adapter;
            });
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
                int apiStatus = 400;
                string errorMsg = "Request failed";
                try
                {
                    (apiStatus, dynamic respond) = await RequestsAsync.Page.CreatePageAsync(pageName, title, categoryId, description);
                    if (apiStatus == 200)
                    {
                        ProgressDialogHelper.Dismiss(this);
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);
                        var pageData = respond as CreatePageObject;
                        var pgName = pageData?.PageData?.PageName ?? pageName;
                        var profileIntent = new Intent(this, typeof(PageProfileActivity));
                        profileIntent.PutExtra("PageId", pgName);
                        StartActivity(profileIntent);
                        Finish();
                        return;
                    }
                    if (respond is WoWonderClient.Classes.Global.ErrorObject err && err.Error != null)
                        errorMsg = err.Error.ErrorText;
                    else
                        errorMsg = respond?.ToString() ?? "Page creation failed";
                }
                catch (Exception sdkEx)
                {
                    Console.WriteLine($"SDK CreatePageAsync exception: {sdkEx.Message}");
                }

                if (apiStatus != 200)
                {
                    string accessToken = UserDetails.AccessToken ?? Current.AccessToken ?? "";
                    string userId = UserDetails.UserId ?? "0";
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        foreach (var apiUrl in new[] {
                            $"http://172.236.19.52/api-v2.php?type=create-page&access_token={accessToken}",
                            $"http://172.236.19.52/app_api.php?application=phone&type=create_page"
                        })
                        {
                            try
                            {
                                using var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(20);
                                var formData = new List<KeyValuePair<string, string>>
                                {
                                    new KeyValuePair<string, string>("page_name", pageName),
                                    new KeyValuePair<string, string>("page_title", title),
                                    new KeyValuePair<string, string>("page_description", description),
                                    new KeyValuePair<string, string>("page_category", categoryId),
                                };
                                if (apiUrl.Contains("app_api.php"))
                                {
                                    formData.Add(new KeyValuePair<string, string>("user_id", userId));
                                    formData.Add(new KeyValuePair<string, string>("s", accessToken));
                                }
                                var response = await client.PostAsync(apiUrl, new FormUrlEncodedContent(formData));
                                var json = await response.Content.ReadAsStringAsync();
                                if (!string.IsNullOrWhiteSpace(json) && json.TrimStart().StartsWith("{"))
                                {
                                    var jobj = JObject.Parse(json);
                                    var status = jobj["api_status"]?.ToString();
                                                                    if (status == "200")
                                                                    {
                                                                        ProgressDialogHelper.Dismiss(this);
                                                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);
                                                                        var createdPageName = jobj["page_data"]?["page_name"]?.ToString() ?? jobj["page_name"]?.ToString() ?? pageName;
                                                                        var profileIntent = new Intent(this, typeof(PageProfileActivity));
                                                                        profileIntent.PutExtra("PageId", createdPageName);
                                                                        StartActivity(profileIntent);
                                                                        Finish();
                                                                        return;
                                                                    }
                                                                    errorMsg = jobj["errors"]?["error_text"]?.ToString() ?? jobj["error"]?.ToString() ?? "API returned error";
                                }
                                else errorMsg = "Invalid server response";
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Direct API exception: {ex.Message}");
                                errorMsg = ex.Message;
                            }
                        }
                    }
                }

                ProgressDialogHelper.Dismiss(this);
                ToastUtils.ShowToast(this, errorMsg, ToastLength.Short);
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
