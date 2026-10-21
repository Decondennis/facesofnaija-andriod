using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using WoWonder.Helpers.Utils;
using AndroidX.AppCompat.Content.Res;
using AndroidX.AppCompat.Widget;
using Google.Android.Material.Dialog; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Facesofnaija.Activities.Base;
using Facesofnaija.Helpers.CacheLoaders;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Requests;
using Exception = System.Exception;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Facesofnaija.Activities.NativePost.Extra;
using Facesofnaija.Activities.NativePost.Post;
using AndroidX.Activity.Result;
using Com.Canhub.Cropper;
using Facesofnaija.Activities.AddPost.Adapters;
using Facesofnaija.Helpers.Controller;
using Facesofnaija.CustomApi.Requests;
using AndroidX.RecyclerView.Widget;
using Com.Zhihu.Matisse;

namespace Facesofnaija.Activities.EditPost
{
    [Activity(Icon = "@mipmap/icon", Theme = "@style/MyTheme", ConfigurationChanges = ConfigChanges.Locale | ConfigChanges.UiMode | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class EditPostActivity : BaseActivity, IDialogListCallBack, IActivityResultCallback
    {
        #region Variables Basic

        public static PostDataObject PostDataExtra;
        public static PostDataObject UpdatedPostResult;

        private ImageView PostSectionImage;
        private TextView TxtAddPost, TxtUserName;
        private EditText TxtContentPost;
        private AppCompatButton PostPrivacyButton;
        private WRecyclerView MainRecyclerView;
        private NativePostAdapter PostFeedAdapter;
        private RecyclerView AttachmentRecyclerView;
        private AttachmentsAdapter AttachmentAdapter;
        private LinearLayout LlAddImage;
        private DialogGalleryController GalleryController;
        private string IdPost = "", PostPrivacy = "", TypeDialog = "";
        private PostDataObject PostData;

        private readonly List<string> NewImagePaths = new List<string>();
        private readonly HashSet<string> DeletedImageIds = new HashSet<string>();
        private readonly Dictionary<Attachments, string> ExistingImageMap = new Dictionary<Attachments, string>();
        private bool AllImagesDeleted = false;
        private int InitialServerImageCount = 0;

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
                SetContentView(Resource.Layout.EditPostLayout);

                var id = Intent?.GetStringExtra("PostId") ?? "Data not available";
                if (id != "Data not available" && !string.IsNullOrEmpty(id))
                    IdPost = id;

                if (PostDataExtra != null && (string.IsNullOrEmpty(IdPost) || PostDataExtra.Id == IdPost))
                {
                    PostData = PostDataExtra;
                    PostDataExtra = null;
                }

                if (PostData == null)
                {
                    var postItemString = Intent?.GetStringExtra("PostItem") ?? string.Empty;
                    if (!string.IsNullOrEmpty(postItemString))
                    {
                        try
                        {
                            PostData = JsonConvert.DeserializeObject<PostDataObject>(postItemString);
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    }
                }

                if (PostData != null && string.IsNullOrEmpty(IdPost))
                {
                    IdPost = PostData.Id;
                }

                //Get Value And Set Toolbar
                InitComponent();
                InitToolbar();
                GalleryController = new DialogGalleryController(this, this);

                if (!string.IsNullOrEmpty(IdPost))
                {
                    if (PostData == null || string.IsNullOrWhiteSpace(TxtContentPost?.Text))
                    {
                        LoadPostDataFromServer();
                    }
                }
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
                TxtAddPost = FindViewById<TextView>(Resource.Id.toolbar_title);
                TxtContentPost = FindViewById<EditText>(Resource.Id.editTxtEmail);
                TxtUserName = FindViewById<TextView>(Resource.Id.card_name);
                PostSectionImage = FindViewById<ImageView>(Resource.Id.postsectionimage);
                PostPrivacyButton = FindViewById<AppCompatButton>(Resource.Id.cont);
                MainRecyclerView = FindViewById<WRecyclerView>(Resource.Id.Recyler);
                LlAddImage = FindViewById<LinearLayout>(Resource.Id.ll_add_image);
                AttachmentRecyclerView = FindViewById<RecyclerView>(Resource.Id.AttachementRecyler);

                if (AttachmentRecyclerView != null)
                {
                    AttachmentRecyclerView.SetLayoutManager(new LinearLayoutManager(this, LinearLayoutManager.Horizontal, false));
                    AttachmentAdapter = new AttachmentsAdapter(this);
                    AttachmentRecyclerView.SetAdapter(AttachmentAdapter);
                }

                if (MainRecyclerView != null)
                {
                    PostFeedAdapter = new NativePostAdapter(this, "", MainRecyclerView, NativeFeedType.Share);
                    MainRecyclerView.NestedScrollingEnabled = false;
                    MainRecyclerView.SetXAdapter(PostFeedAdapter, null);
                }

                if (TxtContentPost != null)
                {
                    Methods.SetColorEditText(TxtContentPost, WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                }

                PopulatePostData();
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PopulatePostData()
        {
            try
            {
                string text = string.Empty;
                if (PostData != null)
                {
                    if (!string.IsNullOrWhiteSpace(PostData.Orginaltext))
                        text = PostData.Orginaltext;
                    else if (!string.IsNullOrWhiteSpace(PostData.PostText))
                        text = PostData.PostText;
                    else if (!string.IsNullOrWhiteSpace(PostData.PostTextApi))
                        text = PostData.PostTextApi;
                }

                if (TxtContentPost != null)
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        var decoded = Methods.FunString.DecodeString(text);
                        TxtContentPost.Text = !string.IsNullOrEmpty(decoded) ? decoded : text;
                        TxtContentPost.SetSelection(TxtContentPost.Text.Length);
                    }
                }

                GetPrivacyPost();

                // Populate editable images in AttachmentAdapter
                if (AttachmentAdapter != null)
                {
                    AttachmentAdapter.AttachmentList.Clear();
                    ExistingImageMap.Clear();
                    DeletedImageIds.Clear();
                    NewImagePaths.Clear();
                    AllImagesDeleted = false;
                    InitialServerImageCount = 0;

                    var photos = PostData?.PhotoMulti ?? PostData?.PhotoAlbum;
                    if (photos != null && photos.Count > 0)
                    {
                        foreach (var photo in photos)
                        {
                            var imageUrl = photo.Image;
                            if (!string.IsNullOrEmpty(imageUrl))
                            {
                                var attach = new Attachments
                                {
                                    Id = int.TryParse(photo.Id, out var parsedId) ? parsedId : 0,
                                    TypeAttachment = "postPhotos[]",
                                    FileUrl = imageUrl,
                                    FileSimple = imageUrl,
                                    FileName = System.IO.Path.GetFileName(imageUrl.Split('?')[0])
                                };
                                ExistingImageMap[attach] = photo.Id ?? "";
                                AttachmentAdapter.AttachmentList.Add(attach);
                                InitialServerImageCount++;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(PostData?.PostFile) || !string.IsNullOrEmpty(PostData?.PostFileFull))
                    {
                        var fileUrl = !string.IsNullOrEmpty(PostData.PostFileFull) ? PostData.PostFileFull : PostData.PostFile;
                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            var cleanUrl = fileUrl.Split('?')[0].ToLowerInvariant();
                            var ext = System.IO.Path.GetExtension(cleanUrl);
                            bool isImage = ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".gif" || ext == ".webp" || ext == ".bmp"
                                || cleanUrl.Contains("/photos/") || cleanUrl.Contains("upload/photos")
                                || string.Equals(PostData.PostType, "photo", StringComparison.OrdinalIgnoreCase)
                                || string.Equals(PostData.PostType, "image", StringComparison.OrdinalIgnoreCase)
                                || (PostFunctions.GetAdapterType(PostData) == PostModelType.ImagePost);

                            if (isImage)
                            {
                                var attach = new Attachments
                                {
                                    Id = 0,
                                    TypeAttachment = "postPhotos[]",
                                    FileUrl = fileUrl,
                                    FileSimple = fileUrl,
                                    FileName = System.IO.Path.GetFileName(cleanUrl)
                                };
                                ExistingImageMap[attach] = "single_image";
                                AttachmentAdapter.AttachmentList.Add(attach);
                                InitialServerImageCount++;
                            }
                        }
                    }

                    // Add the default add-more-images tile
                    AttachmentAdapter.AttachmentList.Add(new Attachments
                    {
                        Id = -1,
                        TypeAttachment = "Default",
                        FileSimple = "Default",
                        FileUrl = "Default"
                    });

                    AttachmentAdapter.NotifyDataSetChanged();
                }

                // Render other media (e.g. video, audio, poll) in PostFeedAdapter if applicable
                if (PostData != null && PostFeedAdapter != null)
                {
                    PostFeedAdapter.ListDiffer?.Clear();
                    var combine = new FeedCombiner(PostData, PostFeedAdapter.ListDiffer, this);
                    combine.CombineDefaultPostSections();

                    PostFeedAdapter.ListDiffer?.RemoveAll(a =>
                        a.TypeView == PostModelType.PromotePost ||
                        a.TypeView == PostModelType.HeaderPost ||
                        a.TypeView == PostModelType.TextSectionPostPart ||
                        a.TypeView == PostModelType.PrevBottomPostPart ||
                        a.TypeView == PostModelType.BottomPostPart ||
                        a.TypeView == PostModelType.AddCommentSection ||
                        a.TypeView == PostModelType.CommentSection ||
                        a.TypeView == PostModelType.Divider ||
                        a.TypeView == PostModelType.ImagePost ||
                        a.TypeView == PostModelType.MultiImage2 ||
                        a.TypeView == PostModelType.MultiImage3 ||
                        a.TypeView == PostModelType.MultiImage4 ||
                        a.TypeView == PostModelType.MultiImages ||
                        a.TypeView == PostModelType.MultiImage5 ||
                        a.TypeView == PostModelType.MultiImage6 ||
                        a.TypeView == PostModelType.MultiImage7 ||
                        a.TypeView == PostModelType.MultiImage8 ||
                        a.TypeView == PostModelType.MultiImage9 ||
                        a.TypeView == PostModelType.MultiImage10);

                    PostFeedAdapter.NotifyDataSetChanged();

                    if (MainRecyclerView != null)
                    {
                        MainRecyclerView.Visibility = (PostFeedAdapter.ListDiffer?.Count ?? 0) > 0 ? ViewStates.Visible : ViewStates.Gone;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private async void LoadPostDataFromServer()
        {
            try
            {
                if (!Methods.CheckConnectivity())
                    return;

                var (apiStatus, respond) = await RequestsAsync.Posts.GetPostDataAsync(IdPost, "post_data");
                if (apiStatus == 200 && respond is GetPostDataObject result && result.PostData != null)
                {
                    PostData = result.PostData;
                    PopulatePostData();
                }
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
                    toolBar.Title = GetText(Resource.String.Lbl_EditPost);
                    toolBar.SetTitleTextColor(WoWonderTools.IsTabDark() ? Color.White : Color.Black);
                    SetSupportActionBar(toolBar);
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

        private void AddOrRemoveEvent(bool addEvent)
        {
            try
            {
                switch (addEvent)
                {
                    // true +=  // false -=
                    case true:
                        PostPrivacyButton.Click += PostPrivacyButton_Click;
                        TxtAddPost.Click += TxtAddPostOnClick;
                        if (LlAddImage != null)
                            LlAddImage.Click += LlAddImageOnClick;
                        if (AttachmentAdapter != null)
                        {
                            AttachmentAdapter.ItemClick += AttachmentAdapterOnItemClick;
                            AttachmentAdapter.DeleteItemClick += AttachmentAdapterOnDeleteItemClick;
                        }
                        break;
                    default:
                        PostPrivacyButton.Click -= PostPrivacyButton_Click;
                        TxtAddPost.Click -= TxtAddPostOnClick;
                        if (LlAddImage != null)
                            LlAddImage.Click -= LlAddImageOnClick;
                        if (AttachmentAdapter != null)
                        {
                            AttachmentAdapter.ItemClick -= AttachmentAdapterOnItemClick;
                            AttachmentAdapter.DeleteItemClick -= AttachmentAdapterOnDeleteItemClick;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void LlAddImageOnClick(object sender, EventArgs e)
        {
            OpenGallery();
        }

        private void AttachmentAdapterOnItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0 && position < (AttachmentAdapter?.AttachmentList?.Count ?? 0))
                {
                    var item = AttachmentAdapter.AttachmentList[position];
                    if (item.TypeAttachment == "Default")
                    {
                        OpenGallery();
                    }
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private void OpenGallery()
        {
            try
            {
                if (!PermissionsController.CheckPermissionStorage("image"))
                {
                    new PermissionsController(this).RequestPermission(108, "image");
                }
                else
                {
                    new IntentController(this).OpenIntentImageGallery(GetText(Resource.String.image), true);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            try
            {
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
                if (requestCode == 108 && grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    OpenGallery();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void AttachmentAdapterOnDeleteItemClick(object sender, AttachmentsAdapterClickEventArgs e)
        {
            try
            {
                var position = e.Position;
                if (position >= 0 && position < (AttachmentAdapter?.AttachmentList?.Count ?? 0))
                {
                    var item = AttachmentAdapter.AttachmentList[position];
                    if (item.TypeAttachment == "Default")
                        return;

                    if (NewImagePaths.Contains(item.FileUrl))
                    {
                        NewImagePaths.Remove(item.FileUrl);
                    }
                    else
                    {
                        if (ExistingImageMap.TryGetValue(item, out var serverId))
                        {
                            if (serverId == "single_image")
                            {
                                AllImagesDeleted = true;
                            }
                            else if (!string.IsNullOrEmpty(serverId))
                            {
                                DeletedImageIds.Add(serverId);
                            }
                        }
                        InitialServerImageCount--;
                        if (InitialServerImageCount <= 0)
                        {
                            AllImagesDeleted = true;
                        }
                    }

                    AttachmentAdapter.AttachmentList.RemoveAt(position);
                    AttachmentAdapter.NotifyItemRemoved(position);
                    AttachmentAdapter.NotifyItemRangeChanged(position, AttachmentAdapter.AttachmentList.Count - position);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            try
            {
                base.OnActivityResult(requestCode, resultCode, data);
                if (requestCode == 500 && resultCode == Result.Ok)
                {
                    var selectedPaths = new List<string>();
                    if (AppSettings.GalleryIntentSystem == GalleryIntentSystem.Matisse)
                    {
                        var mPaths = Matisse.ObtainPathResult(data);
                        if (mPaths != null && mPaths.Count > 0)
                        {
                            selectedPaths.AddRange(mPaths);
                        }
                        else
                        {
                            var mUris = Matisse.ObtainResult(data);
                            if (mUris != null)
                            {
                                foreach (var uri in mUris)
                                {
                                    var path = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                                    if (!string.IsNullOrEmpty(path))
                                        selectedPaths.Add(path);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (data?.ClipData != null)
                        {
                            for (int i = 0; i < data.ClipData.ItemCount; i++)
                            {
                                var uri = data.ClipData.GetItemAt(i)?.Uri;
                                if (uri != null)
                                {
                                    var path = Methods.AttachmentFiles.GetActualPathFromFile(this, uri);
                                    if (!string.IsNullOrEmpty(path))
                                        selectedPaths.Add(path);
                                }
                            }
                        }
                        else if (data?.Data != null)
                        {
                            var path = Methods.AttachmentFiles.GetActualPathFromFile(this, data.Data);
                            if (!string.IsNullOrEmpty(path))
                                selectedPaths.Add(path);
                        }
                    }

                    foreach (var filepath in selectedPaths)
                    {
                        if (string.IsNullOrEmpty(filepath) || NewImagePaths.Contains(filepath))
                            continue;

                        NewImagePaths.Add(filepath);
                        var attach = new Attachments
                        {
                            Id = 0,
                            TypeAttachment = "postPhotos[]",
                            FileUrl = filepath,
                            FileSimple = filepath,
                            FileName = System.IO.Path.GetFileName(filepath)
                        };

                        var defaultItem = AttachmentAdapter?.AttachmentList?.FirstOrDefault(a => a.TypeAttachment == "Default");
                        var defaultIndex = defaultItem != null ? AttachmentAdapter.AttachmentList.IndexOf(defaultItem) : -1;
                        if (defaultIndex >= 0)
                        {
                            AttachmentAdapter.AttachmentList.Insert(defaultIndex, attach);
                            AttachmentAdapter.NotifyItemInserted(defaultIndex);
                        }
                        else
                        {
                            AttachmentAdapter?.AttachmentList?.Add(attach);
                            AttachmentAdapter?.NotifyItemInserted((AttachmentAdapter.AttachmentList?.Count ?? 1) - 1);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        public void OnActivityResult(Java.Lang.Object p0)
        {
            try
            {
                if (p0 is CropImageView.CropResult result)
                {
                    if (result.IsSuccessful)
                    {
                        var resultUri = result.UriContent;
                        var filepath = Methods.AttachmentFiles.GetActualPathFromFile(this, resultUri);
                        if (!string.IsNullOrEmpty(filepath))
                        {
                            NewImagePaths.Add(filepath);
                            var attach = new Attachments
                            {
                                Id = 0,
                                TypeAttachment = "postPhotos[]",
                                FileUrl = filepath,
                                FileSimple = filepath,
                                FileName = System.IO.Path.GetFileName(filepath)
                            };

                            var defaultItem = AttachmentAdapter?.AttachmentList?.FirstOrDefault(a => a.TypeAttachment == "Default");
                            var defaultIndex = defaultItem != null ? AttachmentAdapter.AttachmentList.IndexOf(defaultItem) : -1;
                            if (defaultIndex >= 0)
                            {
                                AttachmentAdapter.AttachmentList.Insert(defaultIndex, attach);
                                AttachmentAdapter.NotifyItemInserted(defaultIndex);
                            }
                            else
                            {
                                AttachmentAdapter?.AttachmentList?.Add(attach);
                                AttachmentAdapter?.NotifyItemInserted((AttachmentAdapter.AttachmentList?.Count ?? 1) - 1);
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, GetText(Resource.String.Lbl_something_went_wrong), ToastLength.Long)?.Show();
                    }
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
                PostDataExtra = null!;
                PostSectionImage = null!;
                TxtUserName = null!;
                TxtAddPost = null!;
                TxtContentPost = null!;
                PostPrivacyButton = null!;
                MainRecyclerView = null!;
                PostFeedAdapter = null!;
                AttachmentRecyclerView = null!;
                AttachmentAdapter = null!;
                LlAddImage = null!;
                GalleryController = null!;
                IdPost = null!; PostPrivacy = null!; TypeDialog = null!;
                PostData = null!;
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
        #endregion

        #region Events

        private async void TxtAddPostOnClick(object sender, EventArgs e)
        {
            try
            {
                if (!Methods.CheckConnectivity())
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                    return;
                }

                var remainingImagesCount = AttachmentAdapter?.AttachmentList?.Count(a => a.TypeAttachment != "Default") ?? 0;
                var hasOtherMedia = PostData != null && (!string.IsNullOrEmpty(PostData.PostRecord) || !string.IsNullOrEmpty(PostData.PostYoutube) || (!string.IsNullOrEmpty(PostData.PostFile) && !AllImagesDeleted));
                if (remainingImagesCount == 0 && !hasOtherMedia && string.IsNullOrWhiteSpace(TxtContentPost?.Text))
                {
                    ToastUtils.ShowToast(this, GetString(Resource.String.Lbl_YouCannot_PostanEmptyPost), ToastLength.Long);
                    return;
                }

                // Show progress
                ProgressDialogHelper.Show(this, GetText(Resource.String.Lbl_Loading) + "...");

                string deletedIdsString = DeletedImageIds.Count > 0 ? string.Join(",", DeletedImageIds) : null;
                bool allDeleted = AllImagesDeleted || (InitialServerImageCount > 0 && remainingImagesCount == NewImagePaths.Count);

                var (apiStatus, respond) = await CustomRequests.Posts.EditPostAsync(
                    IdPost,
                    TxtContentPost?.Text ?? "",
                    PostPrivacy,
                    NewImagePaths,
                    deletedIdsString,
                    allDeleted);

                if (apiStatus == 200)
                {
                    PostDataObject updatedPost = null;
                    try
                    {
                        if (respond != null && respond["post_data"] != null)
                        {
                            var postDataString = respond["post_data"].ToString();
                            updatedPost = JsonConvert.DeserializeObject<PostDataObject>(postDataString);
                        }
                    }
                    catch (Exception ex)
                    {
                        Methods.DisplayReportResultTrack(ex);
                    }

                    if (updatedPost == null || string.IsNullOrEmpty(updatedPost.Id))
                    {
                        try
                        {
                            var (fetchStatus, fetchRespond) = await RequestsAsync.Posts.GetPostDataAsync(IdPost, "post_data");
                            if (fetchStatus == 200 && fetchRespond is GetPostDataObject result && result.PostData != null)
                            {
                                updatedPost = result.PostData;
                            }
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    }

                    ToastUtils.ShowToast(this, GetText(Resource.String.Lbl_postSuccessfullySaved), ToastLength.Short);
                    ProgressDialogHelper.Dismiss(this);

                    UpdatedPostResult = updatedPost;

                    var resultIntent = new Intent();
                    resultIntent?.PutExtra("PostId", IdPost);
                    resultIntent?.PutExtra("PostText", TxtContentPost?.Text ?? "");
                    if (updatedPost != null)
                    {
                        try
                        {
                            resultIntent?.PutExtra("PostItem", JsonConvert.SerializeObject(updatedPost));
                        }
                        catch (Exception ex)
                        {
                            Methods.DisplayReportResultTrack(ex);
                        }
                    }
                    SetResult(Result.Ok, resultIntent);
                    Finish();
                }
                else
                {
                    ProgressDialogHelper.Dismiss(this);
                    var errorMsg = respond?.ToString() ?? "Edit post failed";
                    ToastUtils.ShowToast(this, errorMsg, ToastLength.Long);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                ProgressDialogHelper.Dismiss(this);
            }
        }

        #endregion

        #region MaterialDialog

        public void OnSelection(IDialogInterface dialog, int position, string itemString)
        {
            try
            {
                switch (TypeDialog)
                {
                    case "PostPrivacy":
                        {
                            if (PostPrivacyButton != null)
                                PostPrivacyButton.Text = itemString;

                            if (itemString == GetString(Resource.String.Lbl_Everyone))
                                PostPrivacy = "0";
                            else if (itemString == GetString(Resource.String.Lbl_People_i_Follow))
                                PostPrivacy = "2";
                            else if (itemString == GetString(Resource.String.Lbl_People_Follow_Me))
                                PostPrivacy = "1";
                            else if (itemString == GetString(Resource.String.Lbl_No_body))
                                PostPrivacy = "3";
                            else
                                PostPrivacy = "0";
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

        #region Privacy

        private void LoadDataUser()
        {
            try
            {
                var dataUser = ListUtils.MyProfileList?.FirstOrDefault();
                if (dataUser != null)
                {
                    if (PostSectionImage != null)
                        GlideImageLoader.LoadImage(this, dataUser.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);

                    if (TxtUserName != null)
                        TxtUserName.Text = WoWonderTools.GetNameFinal(dataUser);

                    if (PostPrivacyButton != null)
                    {
                        if (PostData?.PostPrivacy != null)
                        {
                            if (PostData.PostPrivacy.Contains("0"))
                                PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);
                            else if (PostData.PostPrivacy.Contains("ifollow") || PostData.PostPrivacy == "2")
                                PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_i_Follow);
                            else if (PostData.PostPrivacy.Contains("me") || PostData.PostPrivacy == "1")
                                PostPrivacyButton.Text = GetString(Resource.String.Lbl_People_Follow_Me);
                            else
                                PostPrivacyButton.Text = GetString(Resource.String.Lbl_No_body);

                            PostPrivacy = PostData.PostPrivacy;
                        }
                        else
                        {
                            PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);
                            PostPrivacy = "0";
                        }
                    }
                }
                else
                {
                    if (TxtUserName != null)
                        TxtUserName.Text = UserDetails.Username;
                    if (PostPrivacyButton != null)
                        PostPrivacyButton.Text = GetString(Resource.String.Lbl_Everyone);
                    PostPrivacy = "0";
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void GetPrivacyPost()
        {
            try
            {
                if (PostData == null)
                {
                    LoadDataUser();
                    return;
                }

                if (!string.IsNullOrEmpty(PostData.GroupId) && PostData.GroupId != "0")
                {
                    var dataGroup = PostData.GroupRecipient;
                    if (dataGroup != null)
                    {
                        if (PostPrivacyButton != null) PostPrivacyButton.Visibility = ViewStates.Gone;
                        if (PostSectionImage != null) GlideImageLoader.LoadImage(this, dataGroup.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                        if (TxtUserName != null) TxtUserName.Text = dataGroup.GroupName;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else if (!string.IsNullOrEmpty(PostData.PageId) && PostData.PageId != "0")
                {
                    if (PostPrivacyButton != null) PostPrivacyButton.Visibility = ViewStates.Gone;
                    if (PostData.Publisher != null)
                    {
                        if (PostSectionImage != null) GlideImageLoader.LoadImage(this, PostData.Publisher.Avatar, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.DrawableUser);
                        if (TxtUserName != null) TxtUserName.Text = PostData.Publisher.Name;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else if (!string.IsNullOrEmpty(PostData.EventId) && PostData.EventId != "0")
                {
                    if (PostData.Event != null && PostData.Event.Value.EventClass != null)
                    {
                        var dataEvent = PostData.Event.Value.EventClass;
                        if (PostPrivacyButton != null) PostPrivacyButton.Visibility = ViewStates.Gone;
                        if (PostSectionImage != null) GlideImageLoader.LoadImage(this, dataEvent.Cover, PostSectionImage, ImageStyle.CircleCrop, ImagePlaceholders.Drawable);
                        if (TxtUserName != null) TxtUserName.Text = dataEvent.Name;
                    }
                    else
                    {
                        LoadDataUser();
                    }
                }
                else
                {
                    LoadDataUser();
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void PostPrivacyButton_Click(object sender, EventArgs e)
        {
            try
            {
                TypeDialog = "PostPrivacy";

                var arrayAdapter = new List<string>();
                var dialogList = new MaterialAlertDialogBuilder(this);

                arrayAdapter.Add(GetString(Resource.String.Lbl_Everyone));
                arrayAdapter.Add(GetString(Resource.String.Lbl_People_i_Follow));
                arrayAdapter.Add(GetText(Resource.String.Lbl_People_Follow_Me));
                arrayAdapter.Add(GetString(Resource.String.Lbl_No_body));

                dialogList.SetTitle(GetText(Resource.String.Lbl_PostPrivacy));
                dialogList.SetItems(arrayAdapter.ToArray(), new MaterialDialogUtils(arrayAdapter, this));
                dialogList.SetNegativeButton(GetText(Resource.String.Lbl_Close), new MaterialDialogUtils());
                dialogList.Show();
                
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

    }
}