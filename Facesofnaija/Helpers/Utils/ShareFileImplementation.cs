using Android.App;
using Android.Content;
using Android.Util;
using Facesofnaija.Helpers.CacheLoaders;
using WoWonder.Helpers.Utils;
using WoWonderClient;
using Facesofnaija.Helpers.Model;
using AndroidX.Core.Content;
using System;
using System.IO;
using System.Net.Http;
using System.Linq;
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
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType(ResolveMimeType(localFilePath?.Path, textImage));
                intent.PutExtra(Intent.ExtraStream, localFilePath);
                intent.ClipData = ClipData.NewRawUri("shared_media", localFilePath);
                if (!string.IsNullOrWhiteSpace(postUrl))
                    intent.PutExtra(Intent.ExtraText, postUrl);
                intent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Log.Info(ShareTag, $"ShareLocalFile uri={localFilePath} hasPostUrl={!string.IsNullOrEmpty(postUrl)} hasTextImage={!string.IsNullOrEmpty(textImage)}");

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent?.AddFlags(ActivityFlags.ClearTop);
                chooserIntent?.AddFlags(ActivityFlags.NewTask);
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
                intent.AddFlags(ActivityFlags.ClearTop);
                intent.AddFlags(ActivityFlags.NewTask);
                intent.SetAction(Intent.ActionSend);
                intent.SetType("text/plain");
                intent.PutExtra(Intent.ExtraText, text);

                var chooserIntent = Intent.CreateChooser(intent, title);
                chooserIntent?.AddFlags(ActivityFlags.ClearTop);
                chooserIntent?.AddFlags(ActivityFlags.NewTask);
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

                activity?.RunOnUiThread(() => ToastUtils.ShowToast(activity, "Unable to share media right now. Please try again.", Android.Widget.ToastLength.Short));
            }
            catch (Exception ex)
            {
                ProgressDialogHelper.Dismiss(activity);
                activity?.RunOnUiThread(() => ToastUtils.ShowToast(activity, "Unable to share media right now. Please try again.", Android.Widget.ToastLength.Short));
                Console.WriteLine("Exception in ShareFile: ShareRemoteFile Exception: {0}", ex.Message);
            }
        }

        public static async Task<Uri> Download(Activity activity, string imageUrl, string fileName)
        {
            try
            {
                if (activity == null || string.IsNullOrEmpty(imageUrl) || string.IsNullOrEmpty(fileName))
                    return null;

                fileName = BuildSafeFileName(fileName, imageUrl);
                if (string.IsNullOrWhiteSpace(fileName))
                    return null;

                Log.Info(ShareTag, $"Download start url={imageUrl} fileName={fileName}");

                string filePath = Path.Combine(activity.CacheDir?.AbsolutePath ?? activity.FilesDir?.AbsolutePath ?? Path.GetTempPath(), "shared_media");
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

                using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true, AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate }) { Timeout = TimeSpan.FromSeconds(60) };
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Linux; Android 14) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Mobile Safari/537.36");
                client.DefaultRequestHeaders.Accept.ParseAdd("*/*");

                HttpResponseMessage response = null;
                foreach (var candidate in BuildDownloadCandidates(imageUrl))
                {
                    try
                    {
                        using var request = new HttpRequestMessage(HttpMethod.Get, candidate);
                        request.Headers.Referrer = BuildReferrerUri();
                        response = await client.SendAsync(request).ConfigureAwait(false);
                        if (response.IsSuccessStatusCode)
                        {
                            Log.Info(ShareTag, $"Download success-url={candidate}");
                            break;
                        }

                        Log.Warn(ShareTag, $"Download candidate failed http={(int)response.StatusCode} url={candidate}");
                    }
                    catch (Exception ex)
                    {
                        Log.Warn(ShareTag, $"Download candidate exception url={candidate} ex={ex.Message}");
                    }
                }

                if (response == null || !response.IsSuccessStatusCode)
                {
                    Log.Warn(ShareTag, $"Download failed for all candidates url={imageUrl}");
                    return null;
                }

                await using (var fs = new FileStream(mediaFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await response.Content.CopyToAsync(fs);
                }

                var downloadedFile = new Java.IO.File(mediaFile);
                var photoUri = FileProvider.GetUriForFile(activity, activity.PackageName + ".fileprovider", downloadedFile);
                Log.Info(ShareTag, $"Download success path={mediaFile}");
                return photoUri;
            }
            catch (TaskCanceledException ex)
            {
                Log.Warn(ShareTag, $"Download timeout for url={imageUrl}");
                Console.WriteLine("Exception in Download timeout: {0}", ex.Message);
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in Download: {0}", ex.Message);
                return null;
            }
            finally
            {
                activity?.RunOnUiThread(() => ProgressDialogHelper.Dismiss(activity));
            }
        }

        private static string[] BuildDownloadCandidates(string imageUrl)
        {
            try
            {
                var candidates = new System.Collections.Generic.List<string>();
                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    candidates.Add(imageUrl);

                    if (System.Uri.TryCreate(imageUrl, System.UriKind.Absolute, out var absoluteUrl) &&
                        (absoluteUrl.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase) || absoluteUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase)))
                    {
                        var altScheme = absoluteUrl.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? "http" : "https";
                        var altBuilder = new System.UriBuilder(absoluteUrl) { Scheme = altScheme };
                        if ((altScheme == "http" && absoluteUrl.Port == 443) || (altScheme == "https" && absoluteUrl.Port == 80))
                            altBuilder.Port = -1;
                        candidates.Add(altBuilder.Uri.ToString());
                    }

                    var normalizedUrl = GlideImageLoader.NormalizeImageUrl(imageUrl);
                    if (!string.IsNullOrWhiteSpace(normalizedUrl))
                        candidates.Add(normalizedUrl);

                    var baseUrl = InitializeWoWonder.WebsiteUrl?.Trim().TrimEnd('/');
                    if (string.IsNullOrWhiteSpace(baseUrl))
                        baseUrl = AppSettings.SiteUrl?.Trim().TrimEnd('/');

                    if (!string.IsNullOrWhiteSpace(baseUrl) &&
                        !baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                        !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    {
                        baseUrl = "http://" + baseUrl;
                    }

                    if (!string.IsNullOrWhiteSpace(baseUrl) &&
                        System.Uri.TryCreate(baseUrl, System.UriKind.Absolute, out var baseUri) &&
                        System.Uri.TryCreate(imageUrl, System.UriKind.Absolute, out var srcUri) &&
                        !string.Equals(baseUri.Authority, srcUri.Authority, StringComparison.OrdinalIgnoreCase))
                    {
                        var builder = new System.UriBuilder(srcUri)
                        {
                            Scheme = baseUri.Scheme,
                            Host = baseUri.Host,
                            Port = baseUri.IsDefaultPort ? -1 : baseUri.Port
                        };
                        candidates.Add(builder.Uri.ToString());
                    }
                }

                var accessToken = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                var userId = UserDetails.UserId;
                if (!string.IsNullOrWhiteSpace(accessToken))
                {
                    foreach (var candidate in candidates.ToList())
                    {
                        if (string.IsNullOrWhiteSpace(candidate) || candidate.Contains("access_token="))
                            continue;

                        var separator = candidate.Contains("?") ? "&" : "?";
                        var withToken = candidate + separator + "access_token=" + System.Uri.EscapeDataString(accessToken);
                        if (!string.IsNullOrWhiteSpace(userId) && !candidate.Contains("user_id="))
                            withToken += "&user_id=" + System.Uri.EscapeDataString(userId);

                        candidates.Add(withToken);
                    }
                }

                return candidates.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
            }
            catch
            {
                return new[] { imageUrl };
            }
        }

        private static System.Uri BuildReferrerUri()
        {
            try
            {
                var baseUrl = InitializeWoWonder.WebsiteUrl?.Trim().TrimEnd('/');
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = AppSettings.SiteUrl?.Trim().TrimEnd('/');

                if (string.IsNullOrWhiteSpace(baseUrl))
                    return null;

                if (!baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !baseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    baseUrl = "http://" + baseUrl;

                return System.Uri.TryCreate(baseUrl, System.UriKind.Absolute, out var uri) ? uri : null;
            }
            catch
            {
                return null;
            }
        }

        private static string BuildSafeFileName(string fileName, string imageUrl)
        {
            try
            {
                var name = fileName ?? string.Empty;

                if (name.Contains("?"))
                    name = name.Split('?')[0];
                if (name.Contains("#"))
                    name = name.Split('#')[0];

                name = Path.GetFileName(name);
                if (string.IsNullOrWhiteSpace(name))
                    name = "shared_media";

                if (Path.GetExtension(name).Length == 0 && System.Uri.TryCreate(imageUrl, System.UriKind.Absolute, out var uriFromUrl))
                {
                    var extFromUrl = Path.GetExtension(uriFromUrl.AbsolutePath);
                    if (!string.IsNullOrWhiteSpace(extFromUrl))
                        name += extFromUrl;
                }

                foreach (var invalid in Path.GetInvalidFileNameChars())
                    name = name.Replace(invalid, '_');

                return name.Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ResolveMimeType(string localPath, string remoteUrl)
        {
            try
            {
                var ext = Path.GetExtension(localPath);
                if (string.IsNullOrWhiteSpace(ext))
                    ext = Path.GetExtension((remoteUrl ?? string.Empty).Split('?')[0].Split('#')[0]);

                if (string.IsNullOrWhiteSpace(ext))
                    return "*/*";

                var normalized = ext.TrimStart('.').ToLowerInvariant();
                var mime = Android.Webkit.MimeTypeMap.Singleton?.GetMimeTypeFromExtension(normalized);
                return string.IsNullOrWhiteSpace(mime) ? "*/*" : mime;
            }
            catch
            {
                return "*/*";
            }
        }
    }
}
