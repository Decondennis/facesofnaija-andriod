using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomSheet;
using Google.Android.Material.Dialog; 
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Facesofnaija.Activities.NativePost.Post;
using Facesofnaija.Helpers.Utils;
using Facesofnaija.Library.Anjo.Share;
using Facesofnaija.Library.Anjo.Share.Abstractions;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using Exception = System.Exception;

namespace Facesofnaija.Activities.NativePost.Share
{
    public class ShareBottomDialogFragment : BottomSheetDialogFragment 
    {
        #region  Variables Basic

        private const string ShareTag = "ShareDebug";

        private LinearLayout ShareTimelineLayout, ShareGroupLayout, ShareOptionsLayout, SharePageLayout;
        private PostDataObject DataPost;
        private PostModelType TypePost;
        private string TypeDialog;

        #endregion

        #region General

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            try
            {
                Context contextThemeWrapper = WoWonderTools.IsTabDark() ? new ContextThemeWrapper(Activity, Resource.Style.MyTheme_Dark) : new ContextThemeWrapper(Activity, Resource.Style.MyTheme);
                LayoutInflater localInflater = inflater.CloneInContext(contextThemeWrapper);
                View view = localInflater?.Inflate(Resource.Layout.NativeShareBottomDialog, container, false);
                return view;
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
                return null!;
            }
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            try
            {
                base.OnViewCreated(view, savedInstanceState);

                DataPost = JsonConvert.DeserializeObject<PostDataObject>(Arguments?.GetString("ItemData") ?? "");
                TypePost = JsonConvert.DeserializeObject<PostModelType>(Arguments?.GetString("TypePost") ?? "");

                InitComponent(view);
                AddOrRemoveEvent(true);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        public override void OnLowMemory()
        {
            try
            {
                GC.Collect(GC.MaxGeneration);
                base.OnLowMemory();
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion

        #region Functions

        private void InitComponent(View view)
        {
            try
            {
                ShareTimelineLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareTimelineLayout);
                ShareGroupLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareGroupLayout);
                ShareOptionsLayout = view.FindViewById<LinearLayout>(Resource.Id.ShareOptionsLayout);
                SharePageLayout = view.FindViewById<LinearLayout>(Resource.Id.SharePageLayout);

                if (TypePost == PostModelType.AdsPost)
                {
                    ShareTimelineLayout.Visibility = ViewStates.Gone;
                    ShareGroupLayout.Visibility = ViewStates.Gone;
                    SharePageLayout.Visibility = ViewStates.Gone;
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
                        ShareTimelineLayout.Click += ShareTimelineLayoutOnClick;
                        ShareGroupLayout.Click += ShareGroupLayoutOnClick;
                        ShareOptionsLayout.Click += ShareOptionsLayoutOnClick;
                        SharePageLayout.Click += SharePageLayoutOnClick;
                        break;
                    default:
                        ShareTimelineLayout.Click -= ShareTimelineLayoutOnClick;
                        ShareGroupLayout.Click -= ShareGroupLayoutOnClick;
                        ShareOptionsLayout.Click -= ShareOptionsLayoutOnClick;
                        SharePageLayout.Click -= SharePageLayoutOnClick;
                        break;
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }

        private void ShareWithNativeChooser(string title, string text)
        {
            try
            {
                ShareFileImplementation.ShareText(Activity, text, title);
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private string BuildShareText()
        {
            try
            {
                var postText = CleanShareText(DataPost?.PostText);
                var postUrl = !string.IsNullOrWhiteSpace(DataPost?.Url) ? DataPost.Url : string.Empty;

                if (!string.IsNullOrWhiteSpace(postText) && !string.IsNullOrWhiteSpace(postUrl))
                    return $"{postText}\n\n{postUrl}";
                if (!string.IsNullOrWhiteSpace(postText))
                    return postText;
                if (!string.IsNullOrWhiteSpace(postUrl))
                    return postUrl;

                if (TypePost == PostModelType.LinkPost || TypePost == PostModelType.YoutubePost)
                    return "Shared a link";

                return string.Empty;
            }
            catch
            {
                return string.Empty;
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

        private bool TryGetShareMediaAttachment(out string attachmentUrl, out string fileName)
        {
            attachmentUrl = string.Empty;
            fileName = string.Empty;

            try
            {
                var candidateUrl = GetShareMediaCandidateUrl();

                if (string.IsNullOrWhiteSpace(candidateUrl))
                    return false;

                attachmentUrl = candidateUrl;
                fileName = BuildSafeFileName(candidateUrl);
                return !string.IsNullOrWhiteSpace(fileName);
            }
            catch
            {
                return false;
            }
        }

        private string GetShareMediaCandidateUrl()
        {
            try
            {
                var candidates = new List<string>
                {
                    DataPost?.PostSticker,
                    DataPost?.PostFileFull,
                    DataPost?.PostFile,
                    DataPost?.PostFileThumb,
                    DataPost?.PhotoMulti?.FirstOrDefault()?.Image,
                    DataPost?.PhotoAlbum?.FirstOrDefault()?.Image,
                    ReadNestedProductImage(DataPost),
                };

                // Shared posts often keep media in nested SharedInfo payload fields.
                AppendSharedMediaCandidates(candidates, ReadSharedInfoPayload(DataPost));

                return candidates.FirstOrDefault(url => !string.IsNullOrWhiteSpace(url)) ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void AppendSharedMediaCandidates(List<string> candidates, object sharedInfo)
        {
            try
            {
                if (candidates == null || sharedInfo == null)
                    return;

                foreach (var field in new[] { "PostSticker", "PostFileFull", "PostFile", "PostFileThumb", "PostLink", "Url" })
                {
                    candidates.Add(ReadStringProperty(sharedInfo, field));
                }

                candidates.Add(ReadFirstImageProperty(sharedInfo, "PhotoMulti"));
                candidates.Add(ReadFirstImageProperty(sharedInfo, "PhotoAlbum"));
                candidates.Add(ReadNestedProductImage(sharedInfo));
            }
            catch
            {
                // Ignore reflection failures and continue with primary post candidates.
            }
        }

        private static string ReadStringProperty(object target, string propertyName)
        {
            try
            {
                if (target == null || string.IsNullOrWhiteSpace(propertyName))
                    return string.Empty;

                var property = target.GetType().GetProperty(propertyName);
                if (property == null)
                    return string.Empty;

                return property.GetValue(target)?.ToString() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ReadFirstImageProperty(object target, string propertyName)
        {
            try
            {
                var property = target?.GetType().GetProperty(propertyName);
                if (property?.GetValue(target) is System.Collections.IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        var image = ReadStringProperty(item, "Image");
                        if (!string.IsNullOrWhiteSpace(image))
                            return image;
                    }
                }

                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ReadNestedProductImage(object target)
        {
            try
            {
                var product = target?.GetType().GetProperty("Product")?.GetValue(target);
                var value = product?.GetType().GetProperty("Value")?.GetValue(product);
                var productClass = value?.GetType().GetProperty("ProductClass")?.GetValue(value);
                return ReadFirstImageProperty(productClass, "Images");
            }
            catch
            {
                return string.Empty;
            }
        }

        private static object ReadSharedInfoPayload(object target)
        {
            try
            {
                var sharedInfo = target?.GetType().GetProperty("SharedInfo")?.GetValue(target);
                if (sharedInfo == null)
                    return null;

                return sharedInfo.GetType().GetProperty("SharedInfoClass")?.GetValue(sharedInfo);
            }
            catch
            {
                return null;
            }
        }

        private string BuildSafeFileName(string url)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url))
                    return string.Empty;

                var parsed = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null;
                var rawName = parsed != null
                    ? System.IO.Path.GetFileName(parsed.LocalPath)
                    : System.IO.Path.GetFileName(url.Split('?')[0].Split('#')[0]);

                if (string.IsNullOrWhiteSpace(rawName))
                    rawName = "shared_media";

                foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
                    rawName = rawName.Replace(invalidChar, '_');

                return rawName.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task ShareRemoteAttachmentOrTextAsync(string title)
        {
            if (TryGetShareMediaAttachment(out var attachmentUrl, out var fileName))
            {
                await ShareFileImplementation.ShareRemoteFile(Activity, DataPost.Url, attachmentUrl, fileName, title);
                return;
            }

            await CrossShare.Current.Share(new ShareMessage
            {
                Title = string.Empty,
                Text = BuildShareText()
            });
        }

        #endregion

        #region Events

        //ShareToPage
        private void SharePageLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    List<PageDataObject> listPageClass = ListUtils.MyPageList.ToList();

                    if (listPageClass.Count > 0)
                    {
                        Intent intent = new Intent(Context, typeof(SharePageActivity));
                        intent.PutExtra("Pages", JsonConvert.SerializeObject(listPageClass));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost));
                        StartActivity(intent);
                    }
                    else
                        ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_NoPageManaged), ToastLength.Short);

                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private async void ShareOptionsLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Log.Info(ShareTag, $"ShareOptionsLayoutOnClick type={TypePost} hasPost={DataPost != null}");
                if (DataPost == null)
                {
                    ShareWithNativeChooser(Context.GetText(Resource.String.Lbl_Send_to), "");
                    Dismiss();
                    return;
                }

                if (!CrossShare.IsSupported)
                {
                    ShareWithNativeChooser(Context.GetText(Resource.String.Lbl_Send_to), BuildShareText());
                    Dismiss();
                    return;
                }

                switch (TypePost)
                {
                    case PostModelType.EventPost:
                        {
                            if (DataPost.Event?.EventClass != null)
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = Methods.FunString.DecodeString(DataPost.Event.Value.EventClass.Name),
                                    Text = Methods.FunString.DecodeString(DataPost.Event.Value.EventClass.Description),
                                    Url = DataPost.Event.Value.EventClass.Url,
                                });
                            break;
                        }
                    case PostModelType.ImagePost:
                    case PostModelType.StickerPost:
                        {
                            await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));
                            break;
                        }
                    case PostModelType.MapPost:
                    case PostModelType.MultiImage2:
                    case PostModelType.MultiImage3:
                    case PostModelType.MultiImage4:
                    case PostModelType.MultiImage5:
                    case PostModelType.MultiImage6:
                    case PostModelType.MultiImage7:
                    case PostModelType.MultiImage8:
                    case PostModelType.MultiImage9:
                    case PostModelType.MultiImage10:
                    case PostModelType.MultiImages:
                        {
                            await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));
                            break;
                        }
                    case PostModelType.LinkPost:
                    case PostModelType.YoutubePost:
                        {
                            ShareFileImplementation.ShareText(Activity, BuildShareText(), Context.GetText(Resource.String.Lbl_Send_to));
                            break;
                        }
                    case PostModelType.VideoPost:
                        {
                            await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));
                            break;
                        }
                    case PostModelType.FilePost:
                        {
                            await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));
                            break;
                        }
                    case PostModelType.ProductPost:
                        {
                            if (DataPost.Product != null)
                            {
                                var shareOptions = new ShareOptions { ShareExternalUrl = true };
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = Methods.FunString.DecodeString(DataPost.Product.Value.ProductClass.Name),
                                    Text = Methods.FunString.DecodeString(DataPost.Product.Value.ProductClass.Description),
                                    Url = DataPost.Product.Value.ProductClass.Url,
                                }, shareOptions);
                            }
                            break;
                        }
                    case PostModelType.BlogPost:
                        if (DataPost.Blog != null)
                        {
                            var shareOptions = new ShareOptions { ShareExternalUrl = true };
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = Methods.FunString.DecodeString(DataPost.Blog.Value.BlogClass.Title),
                                Text = Methods.FunString.DecodeString(DataPost.Blog.Value.BlogClass.Description),
                                Url = DataPost.Blog.Value.BlogClass.Url,
                            }, shareOptions);
                        }
                        break;
                    case PostModelType.AdsPost:
                        if (DataPost.Blog != null)
                        {
                            await CrossShare.Current.Share(new ShareMessage
                            {
                                Title = "",
                                Text = DataPost.Url,
                                Url = DataPost.Url,
                            });
                        }
                        break;
                    default:
                        {
                            if (DataPost.Blog != null)
                            {
                                await CrossShare.Current.Share(new ShareMessage
                                {
                                    Title = Methods.FunString.DecodeString(DataPost.Blog.Value.BlogClass.Title),
                                    Text = Methods.FunString.DecodeString(DataPost.Blog.Value.BlogClass.Description),
                                    Url = DataPost.Blog.Value.BlogClass.Url,
                                });
                            }
                            else switch (string.IsNullOrEmpty(DataPost.PostSticker))
                                {
                                    case false:
                                        {
                                            await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));

                                            break;
                                        }
                                    default:
                                        {
                                            switch (string.IsNullOrEmpty(DataPost.PostFileFull ?? DataPost.PostFile))
                                            {
                                                case false:
                                                    {
                                                        await ShareRemoteAttachmentOrTextAsync(Context.GetText(Resource.String.Lbl_Send_to));

                                                        break;
                                                    }
                                                default:
                                                    await CrossShare.Current.Share(new ShareMessage
                                                    {
                                                        Title = "",
                                                        Text = Methods.FunString.DecodeString(DataPost.PostText),
                                                        Url = DataPost.Url
                                                    });
                                                    break;
                                            }

                                            break;
                                        }
                                }

                            break;
                        }
                }

                Dismiss();
            }
            catch (Exception exception)
            {
                ShareWithNativeChooser(Context.GetText(Resource.String.Lbl_Send_to), BuildShareText());
                Methods.DisplayReportResultTrack(exception);
            }
        }

        //ShareToGroup
        private void ShareGroupLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                if (Methods.CheckConnectivity())
                {
                    List<GroupDataObject> listGroupClass = ListUtils.MyGroupList.ToList();

                    if (listGroupClass.Count > 0)
                    {
                        Intent intent = new Intent(Context, typeof(ShareGroupActivity));
                        intent.PutExtra("Groups", JsonConvert.SerializeObject(listGroupClass));
                        intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost));
                        StartActivity(intent);
                    }
                    else
                        ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_NoGroupManaged), ToastLength.Short);
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        private void ShareTimelineLayoutOnClick(object sender, EventArgs e)
        {
            try
            {
                Log.Info(ShareTag, $"ShareTimelineLayoutOnClick type={TypePost} postId={DataPost?.PostId} id={DataPost?.Id}");
                if (Methods.CheckConnectivity())
                {
                    TypeDialog = "ShareToMyTimeline";

                    var dialog = new MaterialAlertDialogBuilder(Context);

                    dialog.SetTitle(Resource.String.Lbl_Share);
                    dialog.SetMessage(Context.GetText(Resource.String.Lbl_ShareToMyTimeline));
                    dialog.SetPositiveButton(Context.GetText(Resource.String.Lbl_Yes), (o, args) =>
                    {
                        try
                        {
                            Intent intent = new Intent(Context, typeof(SharePostActivity));
                            intent.PutExtra("ShareToType", "MyTimeline");
                            //intent.PutExtra("ShareToMyTimeline", "");  
                            intent.PutExtra("PostObject", JsonConvert.SerializeObject(DataPost)); //PostDataObject
                            Context.StartActivity(intent);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception); 
                        }
                    });
                    dialog.SetNegativeButton(Context.GetText(Resource.String.Lbl_No), new MaterialDialogUtils());
                   
                    dialog.Show();
                }
                else
                {
                    ToastUtils.ShowToast(Activity, Context.GetText(Resource.String.Lbl_CheckYourInternetConnection), ToastLength.Short);
                }
            }
            catch (Exception exception)
            {
                Methods.DisplayReportResultTrack(exception);
            }
        }

        #endregion 
    }
}