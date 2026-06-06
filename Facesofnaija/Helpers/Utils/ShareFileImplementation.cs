using Android.App;
using Android.Content;
using Android.Util;
using WoWonder.Helpers.Utils;
using AndroidX.Core.Content;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Console = System.Console;
using File = System.IO.File;
using Path = System.IO.Path;
using Uri = Android.Net.Uri;

namespace Facesofnaija.Helpers.Utils
{
    public static class ShareFileImplementation
    {
        private const string ShareTag = "ShareDebug";

        public static void ShareLocalFile(Activity activity, string postUrl, Uri localFilePath, string textImage, string title)
        {
            try
            {
                if (localFilePath == null || string.IsNullOrWhiteSpace(localFilePath.Path))
                {
                    Console.WriteLine("ShareFile: ShareLocalFile Warning: localFilePath null or empty");
                    return;
                }

                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType("*/*");
                intent.PutExtra(Intent.ExtraStream, localFilePath);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Log.Info(ShareTag, $"ShareLocalFile uri={localFilePath} hasPostUrl={!string.IsNullOrEmpty(postUrl)} hasTextImage={!string.IsNullOrEmpty(textImage)}");

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent?.SetFlags(ActivityFlags.ClearTop);
                chooserIntent?.SetFlags(ActivityFlags.NewTask);
                activity.StartActivity(chooserIntent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in ShareFile: ShareLocalFile Exception: {0}", ex);
            }
        }

        public static void ShareText(Activity activity, string text, string title = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    Console.WriteLine("ShareFile: ShareText Warning: text null or empty");
                    return;
                }

                var intent = new Intent();
                intent.SetFlags(ActivityFlags.ClearTop);
                intent.SetFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType("text/plain");
                intent.PutExtra(Intent.ExtraText, text);

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent?.SetFlags(ActivityFlags.ClearTop);
                chooserIntent?.SetFlags(ActivityFlags.NewTask);
                activity.StartActivity(chooserIntent);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in ShareFile: ShareText Exception: {0}", ex);
            }
        }

        public static async Task ShareRemoteFile(Activity activity, string postUrl, string fileUri, string fileName, string title)
        {
            try
            {
                Log.Info(ShareTag, $"ShareRemoteFile start fileUri={fileUri} fileName={fileName}");

                var localUri = await Download(activity, fileUri, fileName);
                if (localUri != null)
                {
                    ShareLocalFile(activity, postUrl, localUri, fileUri, title);
                    return;
                }

                ToastUtils.ShowToast(activity, "Failed to share file. Please try again.", Android.Widget.ToastLength.Long);
            }
            catch (Exception ex)
            {
                ProgressDialogHelper.Dismiss(activity);
                ToastUtils.ShowToast(activity, "Failed to share file. Please try again.", Android.Widget.ToastLength.Long);
                Console.WriteLine("Exception in ShareFile: ShareRemoteFile Exception: {0}", ex.Message);
            }
        }

        public static async Task<Uri> Download(Activity activity, string imageUrl, string fileName)
        {
            try
            {
                if (activity == null || string.IsNullOrEmpty(imageUrl) || string.IsNullOrEmpty(fileName))
                    return null;

                Log.Info(ShareTag, $"Download start url={imageUrl} fileName={fileName}");

                var getImage = Methods.MultiMedia.GetMediaFrom_Gallery(Methods.Path.FolderDcimImage, fileName);
                if (getImage != "File Dont Exists")
                {
                    var cachedFile = new Java.IO.File(getImage);
                    var cachedUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", cachedFile);
                    Log.Info(ShareTag, $"Download cache-hit path={getImage}");
                    return cachedUri;
                }

                string filePath = Path.Combine(Methods.Path.FolderDcimImage);
                string mediaFile = Path.Combine(filePath, fileName);

                if (!Directory.Exists(filePath))
                    Directory.CreateDirectory(filePath);

                if (File.Exists(mediaFile))
                {
                    var existingFile = new Java.IO.File(mediaFile);
                    var localUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", existingFile);
                    Log.Info(ShareTag, $"Download local-hit path={mediaFile}");
                    return localUri;
                }

                activity.RunOnUiThread(() => ProgressDialogHelper.Show(activity, activity.GetText(Resource.String.Lbl_Loading)));

                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                var response = await client.GetAsync(imageUrl, cancellationTokenSource.Token);
                if (!response.IsSuccessStatusCode)
                {
                    Log.Warn(ShareTag, $"Download failed http={(int)response.StatusCode}");
                    activity.RunOnUiThread(() => ToastUtils.ShowToast(activity, "Failed to download file.", Android.Widget.ToastLength.Long));
                    return null;
                }

                await using (var fs = new FileStream(mediaFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs, cancellationTokenSource.Token);
                }

                var downloadedFile = new Java.IO.File(mediaFile);
                var photoUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", downloadedFile);
                Log.Info(ShareTag, $"Download success path={mediaFile}");
                return photoUri;
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn(ShareTag, $"Download timeout for url={imageUrl}");
                activity?.RunOnUiThread(() => ToastUtils.ShowToast(activity, "Sharing the media is taking too long. Please try again.", Android.Widget.ToastLength.Long));
                Console.WriteLine("Exception in Download timeout: {0}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                activity?.RunOnUiThread(() => ToastUtils.ShowToast(activity, "Error during file download.", Android.Widget.ToastLength.Long));
                Console.WriteLine("Exception in Download: {0}", ex.Message);
                return null;
            }
            finally
            {
                activity?.RunOnUiThread(() => ProgressDialogHelper.Dismiss(activity));
            }
        }
    }
}
