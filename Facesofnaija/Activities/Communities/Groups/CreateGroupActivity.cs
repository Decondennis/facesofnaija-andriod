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
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.Ads;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonder.Helpers.Utils;
using WoWonderClient;
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
                // Set fallback categories immediately
                var fallbackNames = new List<string> { "General", "Education", "Entertainment", "Music", "Sports", "Technology", "Business", "Gaming", "Health", "News", "Travel", "Art", "Photography", "Fashion", "Food", "Science", "Charity", "Other" };
                var fallbackIds = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "15", "16", "17", "18" };
                RunOnUiThread(() =>
                {
                    var fbAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, fallbackNames);
                    fbAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                    SpinnerCategory.Adapter = fbAdapter;
                });
                CategoryNames = new List<string>(fallbackNames);
                CategoryIds = new List<string>(fallbackIds);

                // Try SDK first, then direct HTTP
                if (CategoriesController.ListCategoriesGroup.Count > 0)
                {
                    ApplyCategories(CategoriesController.ListCategoriesGroup);
                    return;
                }

                try { await ApiRequest.GetSettings_Api(this); } catch { }
                if (CategoriesController.ListCategoriesGroup.Count > 0)
                {
                    ApplyCategories(CategoriesController.ListCategoriesGroup);
                    return;
                }

                // Direct HTTP fallback
                try
                {
                    string token = UserDetails.AccessToken ?? Current.AccessToken ?? "";
                    using var client = new HttpClient();
                    client.Timeout = TimeSpan.FromSeconds(10);
                    var json = await client.GetStringAsync($"http://172.236.19.52/api-v2.php?type=get-site-settings&access_token={token}");
                    var obj = JObject.Parse(json);
                    var cats = obj["config"]?["group_categories"] as JObject;
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
                int apiStatus = 400;
                string errorMsg = "Request failed";
                try
                {
                    // Try V2 API via SDK
                    (apiStatus, dynamic respond) = await RequestsAsync.Group.CreateGroupAsync(url, title, description, categoryId, privacy);
                    if (apiStatus == 200)
                    {
                        ProgressDialogHelper.Dismiss(this);
                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CreatedSuccessfully), ToastLength.Short);
                        var groupData = respond as CreateGroupObject;
                        var groupName = groupData?.GroupData?.GroupName ?? url;
                        var profileIntent = new Intent(this, typeof(GroupProfileActivity));
                        profileIntent.PutExtra("GroupId", groupName);
                        StartActivity(profileIntent);
                        Finish();
                        return;
                    }
                    if (respond is WoWonderClient.Classes.Global.ErrorObject err && err.Error != null)
                        errorMsg = err.Error.ErrorText;
                    else
                        errorMsg = respond?.ToString() ?? "Group creation failed";
                }
                catch (Exception sdkEx)
                {
                    Console.WriteLine($"SDK CreateGroupAsync exception: {sdkEx.Message}");
                }

                // If SDK failed, try direct API calls
                if (apiStatus != 200)
                {
                    string accessToken = UserDetails.AccessToken ?? Current.AccessToken ?? "";
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        foreach (var apiUrl in new[] {
                            $"http://172.236.19.52/api-v2.php?type=create-group&access_token={accessToken}",
                            $"http://172.236.19.52/app_api.php?application=phone&type=create_group"
                        })
                        {
                            try
                            {
                                using var client = new HttpClient();
                                client.Timeout = TimeSpan.FromSeconds(20);
                                var formData = new List<KeyValuePair<string, string>>
                                {
                                    new KeyValuePair<string, string>("group_name", url),
                                    new KeyValuePair<string, string>("group_title", title),
                                    new KeyValuePair<string, string>("about", description),
                                    new KeyValuePair<string, string>("category", categoryId),
                                    new KeyValuePair<string, string>("privacy", privacy),
                                };
                                if (apiUrl.Contains("app_api.php"))
                                {
                                    formData.Add(new KeyValuePair<string, string>("user_id", UserDetails.UserId ?? "0"));
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
                                                                        var groupName = jobj["group_data"]?["group_name"]?.ToString() ?? jobj["group_name"]?.ToString() ?? url;
                                                                        var profileIntent = new Intent(this, typeof(GroupProfileActivity));
                                                                        profileIntent.PutExtra("GroupId", groupName);
                                                                        StartActivity(profileIntent);
                                                                        Finish();
                                                                        return;
                                                                    }
                                                                    errorMsg = jobj["errors"]?["error_text"]?.ToString() ?? jobj["error"]?.ToString() ?? "API returned error";
                                }
                                else
                                {
                                    errorMsg = "Invalid server response";
                                }
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
