using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using System.Net;
using System.Text.RegularExpressions;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using AndroidX.AppCompat.Content.Res;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.Base;
using Facesofnaija.Activities.MyProfile;
using Facesofnaija.Activities.Communities.Groups;
using Facesofnaija.Activities.Communities.Pages;
using Facesofnaija.Activities.Tabbes;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.CustomApi.Requests;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Facesofnaija.Activities.NativePost.Share
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class SharePostActivity : BaseActivity
    {
        #region Variables Basic

        private Toolbar TopToolBar;
        private ImageView PostSectionImage;
        private TextView TxtSharePost, TxtUserName;
        private EditText TxtContentPost;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private GroupDataObject GroupData;
        private PageDataObject PageData;
        private PostDataObject PostData;
        private string TypePost = "";

        #endregion

        #region General

        protected override void OnCreate(Bundle savedInstanceState)
        {
            try
            {
                base.OnCreate(savedInstanceState);
                SetTheme(WoWonderTools.IsTabDark() ? Resource.Style.MyTheme_Dark : Resource.Style.MyTheme);

                Methods.App.FullScreenApp(this);

                SetContentView(Resource.Layout.SharePostLayout);

                var postdate = Intent?.GetStringExtra("ShareToType") ?? "Data not available";
                if (postdate != "Data not available" && !string.IsNullOrEmpty(postdate)) TypePost = postdate; //Group , Page , MyTimeline

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                SetRecyclerViewAdapters();

                GetDataPost();
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
                base.OnPause();
                AddOrRemoveEvent(false);
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
                TxtSharePost = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtContentPost = FindViewById<EditText>(Resource.Id.editTxtEmail);
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage);
                TxtUserName = FindViewById<TextView>(Resource.Id.card_name);

                Methods.SetColorEditText(TxtContentPost, WoWonderTools.IsTabDark() ? Color.White : Color.Black);

                TxtContentPost.ClearFocus();
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
                TopToolBar = FindViewById<Toolbar>(Resource.Id.toolbar);
                if (TopToolBar != null)
                {
                    TopToolBar.Title = GetText(Resource.String.Lbl_SharePost);
                    TopToolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(TopToolBar);
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

        private void SetRecyclerViewAdapters()
        {
            try
            {
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.Recyler);
                PostFeedAdapter = new NativePostAdapter(this, "", MainRecyclerView, NativeFeedType.Share);
                MainRecyclerView.SetXAdapter(PostFeedAdapter, null);
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
                        TxtSharePost.Click += TxtSharePostOnClick;
                        break;
                    default:
                        TxtSharePost.Click -= TxtSharePostOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        #endregion

        #region Event

        //Share Post 
        private async void TxtSharePostOnClick(object sender, EventArgs e)
        {
            try
            {
                Log.Info("ShareDebug", $"TxtSharePostOnClick typePost={TypePost} sharePostId={PostData?.PostId ?? PostData?.Id} alternateId={(string.IsNullOrEmpty(PostData?.Id) ? "" : PostData.Id)}");
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Long);
                    return;
                }

                //Show a progress
                ProgressDialogHelper.Show(this, GetText(Resource.String.Lbl_Loading));

                var sharePostId = !string.IsNullOrEmpty(PostData?.PostId) ? PostData.PostId : PostData?.Id;
                var alternatePostId = !string.IsNullOrEmpty(PostData?.Id) && PostData.Id != sharePostId ? PostData.Id : null;
                if (string.IsNullOrEmpty(sharePostId))
                {
                    ProgressDialogHelper.Dismiss(this);
                    Methods.DisplayAndHudErrorResult(this, GetText(Resource.String.Lbl_Error).ToString());
                    return;
                }

                switch (TypePost)
                {
                    case "Group":
                        {
                            if (GroupData == null || string.IsNullOrEmpty(GroupData.GroupId))
                            {
                                ProgressDialogHelper.Dismiss(this);
                                Methods.DisplayAndHudErrorResult(this, GetText(Resource.String.Lbl_Error).ToString());
                                return;
                            }

                            var attemptedPostId = sharePostId;
                            (int apiStatus, dynamic respond) = await SharePostWithTimeoutAsync(sharePostId, GroupData.GroupId, "share_post_on_group", TxtContentPost.Text);
                            if (apiStatus != 200 && !string.IsNullOrEmpty(alternatePostId))
                            {
                                attemptedPostId = alternatePostId;
                                (apiStatus, respond) = await SharePostWithTimeoutAsync(alternatePostId, GroupData.GroupId, "share_post_on_group", TxtContentPost.Text);
                            }
                            await HandleShareResponseAsync(apiStatus, respond, attemptedPostId, GroupData.GroupId, "share_post_on_group", "SocialGroup");
                            break;
                        }
                    case "Page":
                        {
                            if (PageData == null || string.IsNullOrEmpty(PageData.PageId))
                            {
                                ProgressDialogHelper.Dismiss(this);
                                Methods.DisplayAndHudErrorResult(this, GetText(Resource.String.Lbl_Error).ToString());
                                return;
                            }

                            var attemptedPostId = sharePostId;
                            (int apiStatus, dynamic respond) = await SharePostWithTimeoutAsync(sharePostId, PageData.PageId, "share_post_on_page", TxtContentPost.Text);
                            if (apiStatus != 200 && !string.IsNullOrEmpty(alternatePostId))
                            {
                                attemptedPostId = alternatePostId;
                                (apiStatus, respond) = await SharePostWithTimeoutAsync(alternatePostId, PageData.PageId, "share_post_on_page", TxtContentPost.Text);
                            }
                            await HandleShareResponseAsync(apiStatus, respond, attemptedPostId, PageData.PageId, "share_post_on_page", "SocialPage");
                            break;
                        }
                    case "MyTimeline":
                        {
                            Log.Info("ShareDebug", $"MyTimeline branch entered postId={sharePostId} userId={UserDetails.UserId}");
                            var attemptedPostId = sharePostId;
                            (int apiStatus, dynamic respond) = await SharePostWithTimeoutAsync(sharePostId, UserDetails.UserId, "share_post_on_timeline", TxtContentPost.Text);
                            if (apiStatus != 200 && !string.IsNullOrEmpty(alternatePostId))
                            {
                                Log.Warn("ShareDebug", $"Primary timeline share failed status={apiStatus}; retrying alternateId={alternatePostId}");
                                attemptedPostId = alternatePostId;
                                (apiStatus, respond) = await SharePostWithTimeoutAsync(alternatePostId, UserDetails.UserId, "share_post_on_timeline", TxtContentPost.Text);
                            }
                            await HandleShareResponseAsync(apiStatus, respond, attemptedPostId, UserDetails.UserId, "share_post_on_timeline", "Normal");
                            break;
                        }
                    default:
                        {
                            ProgressDialogHelper.Dismiss(this);
                            Methods.DisplayAndHudErrorResult(this, GetText(Resource.String.Lbl_Error).ToString());
                            break;
                        }
                }
            }
            catch (Exception exception)
            {
                ProgressDialogHelper.Dismiss(this);
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async Task<(int apiStatus, dynamic respond)> SharePostWithTimeoutAsync(string postId, string targetId, string mode, string text)
        {
            try
            {
                var requestTask = RequestsAsync.Posts.SharePostToAsync(postId, targetId, mode, text);
                var completedTask = await Task.WhenAny(requestTask, Task.Delay(TimeSpan.FromSeconds(40)));

                if (completedTask == requestTask)
                    return await requestTask;

                ProgressDialogHelper.Dismiss(this);
                Log.Warn("WoFallback", $"Share request timed out postId={postId} targetId={targetId} mode={mode} textLength={text?.Length ?? 0}");
                ToastUtils.ShowToast(this, "Request timed out. Please try again.", ToastLength.Long);
                return (408, "Request timed out.");
            }
            catch (Exception e)
            {
                ProgressDialogHelper.Dismiss(this);
                ToastUtils.ShowToast(this, e.Message, ToastLength.Long);
                return (400, e.Message);
            }
        }

        private async Task HandleShareResponseAsync(int apiStatus, dynamic respond, string sharedPostId, string targetId, string shareMode, string pagePost)
        {
            try
            {
                if (apiStatus == 200 && respond is SharePostToObject)
                {
                    Log.Info("WoFallback", $"Share SDK succeeded for sharedPostId={sharedPostId}, proceeding with ResultApi");
                    ResultApi(200, respond);
                    return;
                }

                Log.Info("WoFallback", $"Share SDK failed or invalid response sharedPostId={sharedPostId}, status={apiStatus}; trying fallback");

                var shareText = TxtContentPost?.Text ?? string.Empty;
                if (string.IsNullOrWhiteSpace(shareText))
                    shareText = PostData?.Orginaltext ?? PostData?.PostText ?? " ";
                var (fallbackStatus, fallbackRespond) = await CustomRequests.Posts.SharePostFallbackAsync(sharedPostId, targetId, shareMode, shareText);

                var fallbackText = fallbackRespond?.ToString() ?? string.Empty;
                Log.Info("WoFallback", $"Share fallback result sharedPostId={sharedPostId}, status={fallbackStatus}, hasResponse={!string.IsNullOrEmpty(fallbackText)}");
                if (fallbackStatus == 200)
                {
                    ProgressDialogHelper.Dismiss(this);
                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PostSuccessfullyShared), ToastLength.Short);

                    RefreshSharedFeedAfterFallback();
                    Finish();
                    return;
                }

                Log.Warn("WoFallback", $"Share fallback failed sharedPostId={sharedPostId}, status={fallbackStatus}, response={fallbackText}");
                ProgressDialogHelper.Dismiss(this);
                ToastUtils.ShowToast(this, "Unable to complete share due to an invalid server response.", ToastLength.Long);
            }
            catch (Exception e)
            {
                ProgressDialogHelper.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        }
        private bool ShouldFallbackShare(int apiStatus, dynamic respond)
        {
            try
            {
                if (apiStatus == 408)
                    return true;

                var text = respond?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                    return false;

                if (text.IndexOf("timed out", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;

                return IsUnexpectedShareParserError(respond);
            }
            catch
            {
                return false;
            }
        }

        private bool IsUnexpectedShareParserError(dynamic respond)
        {
            try
            {
                var text = respond?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(text))
                    return false;

                return text.IndexOf("Unexpected character encountered while parsing value: <", StringComparison.OrdinalIgnoreCase) >= 0
                       || text.IndexOf("Unexpected server response", StringComparison.OrdinalIgnoreCase) >= 0
                       || text.IndexOf("<html", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch
            {
                return false;
            }
        }

        private string CleanShareText(string text)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                    return string.Empty;

                var decoded = System.Net.WebUtility.HtmlDecode(text);
                return System.Text.RegularExpressions.Regex.Replace(decoded, "<.*?>", string.Empty).Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private void ResultApi(int apiStatus, dynamic respond)
        {
            try
            {
                switch (apiStatus)
                {
                    case 200:
                        {
                            switch (respond)
                            {
                                case SharePostToObject result:
                                    {
                                        ProgressDialogHelper.Dismiss(this);

                                        if (result.Data.SharedInfo.SharedInfoClass == null)
                                        {
                                            result.Data.ParentId = PostData.PostId;

                                            result.Data.SharedInfo = new SharedInfoUnion
                                            {
                                                SharedInfoClass = PostData
                                            };
                                        }

                                        //var globalContextTabbed = TabbedMainActivity.GetInstance();

                                        //var countList = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ItemCount;

                                        //var combine = new FeedCombiner(result.Data, globalContextTabbed?.NewsFeedTab?.PostFeedAdapter?.ListDiffer, this);
                                        //combine.CombineDefaultPostSections("Top");

                                        //int countIndex = 1;
                                        //var model1 = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                        //var model2 = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                                        //var model3 = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.FilterSection);
                                        //var model4 = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);
                                        //var model5 = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.SearchForPosts);

                                        //if (model5 != null)
                                        //    countIndex += globalContextTabbed.NewsFeedTab.postFeedAdapter.ListDiffer.IndexOf(model5) + 1;
                                        //else if (model4 != null)
                                        //    countIndex += globalContextTabbed.NewsFeedTab.postFeedAdapter.ListDiffer.IndexOf(model4) + 1;
                                        //else if (model3 != null)
                                        //    countIndex += globalContextTabbed.NewsFeedTab.postFeedAdapter.ListDiffer.IndexOf(model3) + 1;
                                        //else if (model2 != null)
                                        //    countIndex += globalContextTabbed.NewsFeedTab.postFeedAdapter.ListDiffer.IndexOf(model2) + 1;
                                        //else if (model1 != null)
                                        //    countIndex += globalContextTabbed.NewsFeedTab.postFeedAdapter.ListDiffer.IndexOf(model1) + 1;
                                        //else
                                        //    countIndex = 0;

                                        //var emptyStateChecker = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                        //if (emptyStateChecker != null && globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.Count > 1)
                                        //    globalContextTabbed.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                        //globalContextTabbed.NewsFeedTab.PostFeedAdapter?.NotifyItemRangeInserted(countIndex, globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.Count - countList);

                                        switch (TypePost)
                                        {
                                            case "MyTimeline":
                                                {
                                                    MyProfileActivity myProfileActivity = MyProfileActivity.GetInstance();
                                                    if (myProfileActivity != null)
                                                    {
                                                        var countList1 = myProfileActivity.PostFeedAdapter?.ItemCount ?? 0;

                                                        var combine1 = new FeedCombiner(result.Data, myProfileActivity.PostFeedAdapter?.ListDiffer, this);

                                                        var check1 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                                        if (check1 != null)
                                                            combine1.CombineDefaultPostSections("Top");
                                                        else
                                                            combine1.CombineDefaultPostSections();

                                                        int countIndex1 = 1;
                                                        var model11 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.Story);
                                                        var model21 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AddPostBox);
                                                        var model41 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.AlertBox);

                                                        if (model41 != null)
                                                            countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model41) + 1;
                                                        else if (model21 != null)
                                                            countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model21) + 1;
                                                        else if (model11 != null)
                                                            countIndex1 += myProfileActivity.PostFeedAdapter.ListDiffer.IndexOf(model11) + 1;
                                                        else
                                                            countIndex1 = 0;

                                                        var emptyStateChecker1 = myProfileActivity.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                                        if (emptyStateChecker1 != null && myProfileActivity.PostFeedAdapter?.ListDiffer?.Count > 1)
                                                            myProfileActivity.MainRecyclerView.RemoveByRowIndex(emptyStateChecker1);

                                                        myProfileActivity.PostFeedAdapter?.NotifyItemRangeInserted(countIndex1, myProfileActivity.PostFeedAdapter.ListDiffer.Count - countList1);
                                                    }

                                                    break;
                                                }
                                        }

                                        var globalContextTabbed = TabbedMainActivity.GetInstance();
                                        if (globalContextTabbed != null)
                                        {
                                            var countList = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ItemCount ?? 0;

                                            var combine = new FeedCombiner(result.Data, globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer, this);

                                            var check = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.PostData != null && a.TypeView != PostModelType.AddPostBox /*&& a.TypeView != PostModelType.SearchForPosts*/);
                                            if (check != null)
                                                combine.CombineDefaultPostSections("Top");
                                            else
                                                combine.CombineDefaultPostSections();

                                            var emptyStateChecker = globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.FirstOrDefault(a => a.TypeView == PostModelType.EmptyState);
                                            if (emptyStateChecker != null && globalContextTabbed.NewsFeedTab.PostFeedAdapter?.ListDiffer?.Count > 1)
                                                globalContextTabbed.NewsFeedTab.MainRecyclerView.RemoveByRowIndex(emptyStateChecker);

                                            globalContextTabbed.NewsFeedTab.PostFeedAdapter?.NotifyDataSetChanged();
                                        }

                                        ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_PostSuccessfullyShared), ToastLength.Short);

                                        switch (UserDetails.SoundControl)
                                        {
                                            case true:
                                                Methods.AudioRecorderAndPlayer.PlayAudioFromAsset("PopNotificationPost.mp3");
                                                break;
                                        }

                                        Finish();
                                        break;
                                    }
                            }

                            ProgressDialogHelper.Dismiss(this);
                            break;
                        }
                    default:
                        ProgressDialogHelper.Dismiss(this);
                        Methods.DisplayAndHudErrorResult(this, respond);
                        break;
                }
            }
            catch (Exception e)
            {
                ProgressDialogHelper.Dismiss(this);
                Methods.DisplayReportResultTrack(e);
            }
        }

            private void RefreshSharedFeedAfterFallback()
            {
                try
                {
                    switch (TypePost)
                    {
                        case "MyTimeline":
                            RunOnUiThread(() =>
                            {
                                TabbedMainActivity.GetInstance()?.NewsFeedTab?.SwipeRefreshLayoutOnRefresh(null, EventArgs.Empty);
                                MyProfileActivity.GetInstance()?.SwipeRefreshLayoutOnRefresh(null, EventArgs.Empty);
                            });
                            break;
                    }
                }
                catch (Exception e)
                {
                    Methods.DisplayReportResultTrack(e);
                }
            }

        private void GetDataPost()
        {
            try
            {
                switch (TypePost)
                {
                    case "Group":
                        {
                            GroupData = JsonConvert.DeserializeObject<GroupDataObject>(Intent?.GetStringExtra("ShareToGroup") ?? "");
                            if (GroupData != null)
                            {
                                GlideImageLoader.LoadImage(this, GroupData.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                TxtUserName.Text = GroupData.GroupName;
                            }

                            break;
                        }
                    case "Page":
                        {
                            PageData = JsonConvert.DeserializeObject<PageDataObject>(Intent?.GetStringExtra("ShareToPage") ?? "");
                            if (PageData != null)
                            {
                                GlideImageLoader.LoadImage(this, PageData.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                                TxtUserName.Text = PageData.PageName;
                            }

                            break;
                        }
                    case "MyTimeline":
                        GlideImageLoader.LoadImage(this, UserDetails.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                        TxtUserName.Text = UserDetails.FullName;
                        break;
                }

                PostData = JsonConvert.DeserializeObject<PostDataObject>(Intent?.GetStringExtra("PostObject") ?? "");
                if (PostData != null)
                {
                    switch (TypePost)
                    {
                        case "Group" when PostData.GroupRecipient == null:
                            {
                                if (GroupData != null)
                                {
                                    PostData.GroupId = GroupData.GroupId;
                                    PostData.GroupRecipient = GroupData;
                                }
                                break;
                            }
                        case "Page" when PostData.Publisher == null:
                            {
                                if (PageData != null)
                                {
                                    PostData.PageId = PageData.PageId;
                                    PostData.Publisher = new PublisherPost
                                    {
                                        Avatar = PageData.Avatar,
                                        About = PageData.About,
                                        Active = PageData.Active,
                                        Address = PageData.Address,
                                        BackgroundImage = PageData.BackgroundImage,
                                        Boosted = Convert.ToInt32(PageData.Boosted),
                                        CallActionType = Convert.ToInt32(PageData.CallActionType),
                                        Category = PageData.Category,
                                        Company = PageData.Company,
                                        Cover = PageData.Cover,
                                        Google = PageData.Google,
                                        Instgram = PageData.Instgram,
                                        IsPageOnwer = PageData.IsPageOnwer,
                                        Linkedin = PageData.Linkedin,
                                        Name = PageData.Name,
                                        PageCategory = Convert.ToInt32(PageData.PageCategory),
                                        PageDescription = PageData.PageDescription,
                                        PageId = Convert.ToInt32(PageData.PageId),
                                        PageName = PageData.PageName,
                                        PageTitle = PageData.PageTitle,
                                        Phone = PageData.Phone,
                                        Rating = Convert.ToInt32(PageData.Rating),
                                        Registered = PageData.Registered,
                                        Twitter = PageData.Twitter,
                                        Url = PageData.Url,
                                    };
                                }
                                break;
                            }
                    }

                    var combine = new FeedCombiner(PostData, PostFeedAdapter?.ListDiffer, this);
                    combine.CombineDefaultPostSections();

                    PostFeedAdapter?.NotifyDataSetChanged();
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

