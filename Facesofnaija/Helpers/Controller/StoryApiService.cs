using Newtonsoft.Json;
using static Facesofnaija.AppSettings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Facesofnaija.Helpers.Model;
using Facesofnaija.Helpers.Utils;
using WoWonderClient;
using WoWonderClient.Classes.Story;
using WoWonderClient.Requests;

namespace Facesofnaija.Helpers.Controller
{
    public static class StoryApiService
    {
        private static string ResolveAccessToken()
        {
            var token = Current.AccessToken;

            if (string.IsNullOrWhiteSpace(token))
                token = UserDetails.AccessToken;

            if (!string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(Current.AccessToken))
                Current.AccessToken = token;

            return token ?? string.Empty;
        }

        private static GetUserStoriesObject NormalizeStoriesResponse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;

            try
            {
                var result = JsonConvert.DeserializeObject<GetUserStoriesObject>(json);
                if (result != null && result.Status == 200 && result.Stories?.Any(s => s.Stories?.Count > 0) == true)
                    return result;
            }
            catch (Exception)
            {
                // Fallback JSON shape handling is below.
            }

            try
            {
                var obj = JObject.Parse(json);
                var apiStatusToken = obj["api_status"];
                var storiesToken = obj["stories"];
                if (storiesToken != null && int.TryParse(apiStatusToken?.ToString(), out var normalizedStatus) && normalizedStatus == 200)
                {
                    static StoryDataObject BuildGroupFromRows(IEnumerable<JObject> rows, string fallbackUserId = "")
                    {
                        var rowList = rows?.ToList() ?? new List<JObject>();
                        if (rowList.Count == 0)
                            return null;

                        var firstRow = rowList.First();
                        var firstUser = firstRow["user_data"] as JObject;
                        var userId = firstRow["user_id"]?.ToString() ?? firstUser?["user_id"]?.ToString() ?? fallbackUserId ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(userId))
                            return null;

                        var group = new StoryDataObject
                        {
                            UserId = userId,
                            Username = firstUser?["username"]?.ToString() ?? string.Empty,
                            Avatar = firstUser?["avatar"]?.ToString() ?? string.Empty,
                            FirstName = firstUser?["first_name"]?.ToString() ?? string.Empty,
                            LastName = firstUser?["last_name"]?.ToString() ?? string.Empty,
                            Stories = new List<StoryDataObject.Story>(),
                            DurationsList = new List<long>()
                        };

                        var seenStoryIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var row in rowList)
                        {
                            var storyId = row["id"]?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(storyId) && seenStoryIds.Contains(storyId))
                                continue;

                            if (string.IsNullOrWhiteSpace(group.Avatar))
                                group.Avatar = row["user_data"]?["avatar"]?.ToString() ?? string.Empty;

                            var images = new List<StoryDataObject.Image>();
                            if (row["images"] is JArray imagesArray)
                            {
                                foreach (var imageToken in imagesArray)
                                {
                                    var filename = imageToken?["filename"]?.ToString() ?? string.Empty;
                                    if (string.IsNullOrWhiteSpace(filename))
                                        continue;
                                    images.Add(new StoryDataObject.Image { Filename = filename });
                                }
                            }

                            var videos = new List<StoryDataObject.Video>();
                            if (row["videos"] is JArray videosArray)
                            {
                                foreach (var videoToken in videosArray)
                                {
                                    var filename = videoToken?["filename"]?.ToString() ?? string.Empty;
                                    if (string.IsNullOrWhiteSpace(filename))
                                        continue;

                                    videos.Add(new StoryDataObject.Video { Filename = filename });
                                }
                            }

                            group.Stories.Add(new StoryDataObject.Story
                            {
                                Id = storyId,
                                UserId = row["user_id"]?.ToString() ?? group.UserId,
                                Title = row["title"]?.ToString() ?? string.Empty,
                                Description = row["description"]?.ToString() ?? string.Empty,
                                Posted = row["posted"]?.ToString() ?? string.Empty,
                                Expire = row["expire"]?.ToString() ?? string.Empty,
                                Thumbnail = row["thumbnail"]?.ToString() ?? group.Avatar,
                                Videos = videos,
                                Images = images.Count > 0 ? images : new List<StoryDataObject.Image>(),
                                ViewCount = row["view_count"]?.ToString() ?? "0",
                                TypeView = videos.Count > 0 ? "Video" : ("Image")
                            });

                            if (!string.IsNullOrWhiteSpace(storyId))
                                seenStoryIds.Add(storyId);
                        }

                        return group.Stories.Count > 0 ? group : null;
                    }

                    // Shape A: grouped stories [{ user_id, avatar, stories:[...] }]
                    var normalizedStories = storiesToken.ToObject<List<StoryDataObject>>() ?? new List<StoryDataObject>();

                    // Shape B: flat rows [{ id, user_id, thumbnail, user_data, videos, ... }]
                    // Shape A may produce non-empty count but with no Stories sub-items (flat rows misidentified as grouped).
                    if (normalizedStories.All(s => s.Stories == null || s.Stories.Count == 0) && storiesToken is JArray rawRows && rawRows.Count > 0)
                    {
                        var groupedStories = new Dictionary<string, StoryDataObject>();
                        var seenStoryIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                        foreach (var rowToken in rawRows)
                        {
                            if (rowToken is not JObject row)
                                continue;

                            var storyId = row["id"]?.ToString() ?? string.Empty;
                            if (!string.IsNullOrWhiteSpace(storyId) && seenStoryIds.Contains(storyId))
                                continue;

                            var userData = row["user_data"] as JObject;
                            var userId = row["user_id"]?.ToString() ?? userData?["user_id"]?.ToString() ?? string.Empty;
                            if (string.IsNullOrWhiteSpace(userId))
                                continue;

                            if (!groupedStories.TryGetValue(userId, out var userGroup))
                            {
                                userGroup = new StoryDataObject
                                {
                                    UserId = userId,
                                    Username = userData?["username"]?.ToString() ?? string.Empty,
                                    Avatar = userData?["avatar"]?.ToString() ?? string.Empty,
                                    FirstName = userData?["first_name"]?.ToString() ?? string.Empty,
                                    LastName = userData?["last_name"]?.ToString() ?? string.Empty,
                                    Stories = new List<StoryDataObject.Story>(),
                                    DurationsList = new List<long>()
                                };

                                groupedStories[userId] = userGroup;
                            }

                            var images = new List<StoryDataObject.Image>();
                            if (row["images"] is JArray imagesArray)
                            {
                                foreach (var imageToken in imagesArray)
                                {
                                    var filename = imageToken?["filename"]?.ToString() ?? string.Empty;
                                    if (string.IsNullOrWhiteSpace(filename))
                                        continue;
                                    images.Add(new StoryDataObject.Image { Filename = filename });
                                }
                            }

                            var videos = new List<StoryDataObject.Video>();
                            if (row["videos"] is JArray videosArray)
                            {
                                foreach (var videoToken in videosArray)
                                {
                                    var filename = videoToken?["filename"]?.ToString() ?? string.Empty;
                                    if (string.IsNullOrWhiteSpace(filename))
                                        continue;

                                    videos.Add(new StoryDataObject.Video
                                    {
                                        Filename = filename
                                    });
                                }
                            }

                            var story = new StoryDataObject.Story
                            {
                                Id = storyId,
                                UserId = userId,
                                Title = row["title"]?.ToString() ?? string.Empty,
                                Description = row["description"]?.ToString() ?? string.Empty,
                                Posted = row["posted"]?.ToString() ?? string.Empty,
                                Expire = row["expire"]?.ToString() ?? string.Empty,
                                Thumbnail = row["thumbnail"]?.ToString() ?? userGroup.Avatar,
                                Videos = videos,
                                Images = images.Count > 0 ? images : new List<StoryDataObject.Image>(),
                                ViewCount = row["view_count"]?.ToString() ?? "0",
                                TypeView = videos.Count > 0 ? "Video" : ("Image")
                            };

                            userGroup.Stories.Add(story);

                            if (!string.IsNullOrWhiteSpace(storyId))
                                seenStoryIds.Add(storyId);
                        }

                        normalizedStories = groupedStories.Values.Where(a => a.Stories?.Count > 0).ToList();

                        // Populate default durations so the progress bar works in the viewer.
                        foreach (var g in normalizedStories)
                        {
                            if (g.DurationsList == null || g.DurationsList.Count == 0)
                                g.DurationsList = Enumerable.Repeat(5000L, g.Stories.Count).ToList();
                        }

                        System.Diagnostics.Debug.WriteLine($"[StoryApiService] Normalized flat stories rows={rawRows.Count} groups={normalizedStories.Count}");
                                            foreach (var g in normalizedStories)
                                                Android.Util.Log.Warn("FON_STORY_PARSE", $"group userId={g.UserId} avatar={g.Avatar ?? "NULL"} storiesCount={g.Stories?.Count} thumb0={g.Stories?.FirstOrDefault()?.Thumbnail ?? "NULL"}");
                    }

                    // Shape C: object map { "123": {user_id, stories:[...]}, ... } or { "123": [rows...] }
                    if (normalizedStories.All(s => s.Stories == null || s.Stories.Count == 0) && storiesToken is JObject storiesObject && storiesObject.HasValues)
                    {
                        var groups = new List<StoryDataObject>();

                        foreach (var property in storiesObject.Properties())
                        {
                            if (property.Value is JObject groupObject)
                            {
                                if (groupObject["stories"] != null)
                                {
                                    var parsedGroup = groupObject.ToObject<StoryDataObject>();
                                    if (parsedGroup?.Stories?.Count > 0)
                                    {
                                        if (string.IsNullOrWhiteSpace(parsedGroup.UserId))
                                            parsedGroup.UserId = property.Name;
                                        groups.Add(parsedGroup);
                                        continue;
                                    }
                                }

                                var singleGroup = BuildGroupFromRows(new[] { groupObject }, property.Name);
                                if (singleGroup != null)
                                    groups.Add(singleGroup);
                            }
                            else if (property.Value is JArray rowsArray)
                            {
                                var rowObjects = rowsArray.OfType<JObject>().ToList();
                                var arrayGroup = BuildGroupFromRows(rowObjects, property.Name);
                                if (arrayGroup != null)
                                    groups.Add(arrayGroup);
                            }
                        }

                        normalizedStories = groups.Where(g => g?.Stories?.Count > 0).ToList();
                        System.Diagnostics.Debug.WriteLine($"[StoryApiService] Normalized object-map stories keys={storiesObject.Count} groups={normalizedStories.Count}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[StoryApiService] NormalizeStoriesResponse status={normalizedStatus} stories_count={normalizedStories.Count}");

                    return new GetUserStoriesObject
                    {
                        Status = 200,
                        Stories = normalizedStories
                    };
                }
            }
            catch (Exception)
            {
                // Ignore shape normalization errors.
            }

            return null;
        }

        private static List<string> BuildBaseUrls()
        {
            var bases = new List<string>();

            // Origin host fallback: currently serves story APIs while public host returns 404 for story routes.
            bases.Add(AppSettings.SiteUrl);

            var configBase = InitializeWoWonder.WebsiteUrl?.Trim()?.TrimEnd('/');
            if (!string.IsNullOrWhiteSpace(configBase) && !bases.Contains(configBase))
                bases.Add(configBase);

            return bases;
        }

        private static List<string> BuildStoryUrls(string type)
        {
            var token = ResolveAccessToken();

            var urls = new List<string>();

            void AddUrl(string url)
            {
                if (!string.IsNullOrWhiteSpace(url) && !urls.Contains(url))
                    urls.Add(url);
            }

            foreach (var baseUrl in BuildBaseUrls())
            {
                // Common WoWonder variants depending on how WebsiteUrl is configured.
                AddUrl($"{baseUrl}/api/v2/?type={type}&access_token={token}");
                AddUrl($"{baseUrl}/api/?type={type}&access_token={token}");

                if (baseUrl.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    AddUrl($"{baseUrl}/v2/?type={type}&access_token={token}");

                if (baseUrl.EndsWith("/api/v2", StringComparison.OrdinalIgnoreCase))
                {
                    AddUrl($"{baseUrl}/?type={type}&access_token={token}");
                    AddUrl($"{baseUrl.Replace("/api/v2", "/api")}/?type={type}&access_token={token}");
                }

                // Also try underscore style as a fallback for some backend customizations.
                if (type.Contains('-'))
                {
                    var underscoreType = type.Replace("-", "_");
                    AddUrl($"{baseUrl}/api/v2/?type={underscoreType}&access_token={token}");
                    AddUrl($"{baseUrl}/api/?type={underscoreType}&access_token={token}");
                }
            }

            return urls.Distinct().ToList();
        }

        private static List<string> BuildLegacyStoryUrls()
        {
            var urls = new List<string>();

            foreach (var baseUrl in BuildBaseUrls())
            {
                var cleanBase = baseUrl.TrimEnd('/');
                urls.Add($"{cleanBase}/app_api.php?application=phone&type=get_stories");
                urls.Add($"{cleanBase}/app_api.php?type=get_stories&application=phone");
            }

            return urls.Distinct().ToList();
        }

        public static async Task<(int apiStatus, dynamic response)> GetUserStoriesAsync(string limit = "15", string offset = "0")
        {
            try
            {
                int ScoreStories(GetUserStoriesObject storiesObject)
                {
                    if (storiesObject?.Stories == null || storiesObject.Stories.Count == 0)
                        return 0;

                    var users = storiesObject.Stories
                        .Where(s => !string.IsNullOrWhiteSpace(s?.UserId))
                        .Select(s => s.UserId)
                        .Distinct(StringComparer.Ordinal)
                        .ToList();

                    var otherUsers = users.Count(id => !string.Equals(id, UserDetails.UserId, StringComparison.Ordinal));
                    // Prioritize other users, then total unique users.
                    return (otherUsers * 1000) + users.Count;
                }

                var resolvedToken = ResolveAccessToken();
                Android.Util.Log.Warn("FON_STORY_API", $"ResolveAccessToken returned '{(resolvedToken?.Length > 10 ? resolvedToken.Substring(0, 10) + "..." : resolvedToken ?? "NULL")}'");
                if (string.IsNullOrEmpty(resolvedToken))
                    return (400, "Access token is missing");

                GetUserStoriesObject bestStoriesResponse = null;
                var bestStoriesScore = -1;

                // First try SDK route mapping (usually matches server-specific API wiring).
                var (sdkStatusInitial, sdkResponseInitial) = await RequestsAsync.Story.GetUserStoriesAsync(limit, offset).ConfigureAwait(false);
                if (sdkStatusInitial == 200)
                {
                    if (sdkResponseInitial is GetUserStoriesObject sdkStories && sdkStories.Stories?.Count > 0)
                    {
                        bestStoriesResponse = sdkStories;
                        bestStoriesScore = ScoreStories(sdkStories);
                        Android.Util.Log.Warn("FON_STORY_API", $"SDK initial stories_count={sdkStories.Stories.Count} score={bestStoriesScore}");
                    }

                    System.Diagnostics.Debug.WriteLine("[StoryApiService] SDK initial checked; trying endpoint fallbacks for richer dataset.");
                }

                using var client = new HttpClient();
                var urls = new List<string>();
                urls.AddRange(BuildStoryUrls("get-user-stories"));
                urls.AddRange(BuildStoryUrls("get-stories"));
                urls = urls.Distinct().ToList();
                var token = ResolveAccessToken();
                var userId = UserDetails.UserId ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"[StoryApiService] WebsiteUrl='{InitializeWoWonder.WebsiteUrl}', StoryUrls='{string.Join(" | ", urls)}'");

                string lastResponse = "Invalid story response";
                GetUserStoriesObject emptyStoriesResponse = null;
                foreach (var url in urls)
                {
                    var requestUrls = new List<(string label, string value)>
                    {
                        ("POST", url),
                        ("GET", $"{url}&limit={Uri.EscapeDataString(limit ?? "15")}&offset={Uri.EscapeDataString(offset ?? "0")}&user_id={Uri.EscapeDataString(userId)}&s={Uri.EscapeDataString(token)}&access_token={Uri.EscapeDataString(token)}")
                    };

                    foreach (var requestUrl in requestUrls)
                    {
                        HttpResponseMessage response;
                        if (requestUrl.label == "POST")
                        {
                            using var content = new FormUrlEncodedContent(new[]
                            {
                                new KeyValuePair<string, string>("limit", limit ?? "15"),
                                new KeyValuePair<string, string>("offset", offset ?? "0"),
                                new KeyValuePair<string, string>("user_id", userId),
                                new KeyValuePair<string, string>("s", token),
                                new KeyValuePair<string, string>("access_token", token),
                                new KeyValuePair<string, string>("my_offset", offset ?? "0"),
                            });

                            response = await client.PostAsync(requestUrl.value, content).ConfigureAwait(false);
                        }
                        else
                        {
                            response = await client.GetAsync(requestUrl.value).ConfigureAwait(false);
                        }

                        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var preview = json?.Length > 300 ? json.Substring(0, 300) : json;
                        System.Diagnostics.Debug.WriteLine($"[StoryApiService] get-stories {requestUrl.label} URL={requestUrl.value} HTTP={response.StatusCode} raw={preview}");

                        if (!response.IsSuccessStatusCode)
                        {
                            lastResponse = string.IsNullOrWhiteSpace(json) ? response.StatusCode.ToString() : json;
                            continue;
                        }

                        var normalized = NormalizeStoriesResponse(json);
                        if (normalized != null && normalized.Status == 200)
                        {
                            System.Diagnostics.Debug.WriteLine($"[StoryApiService] Parsed {requestUrl.label} URL={requestUrl.value} stories_count={normalized.Stories?.Count ?? 0}");
                            if (normalized.Stories?.Count > 0)
                            {
                                var score = ScoreStories(normalized);
                                Android.Util.Log.Warn("FON_STORY_API", $"Route {requestUrl.label} stories_count={normalized.Stories.Count} score={score} url={requestUrl.value}");
                                if (score > bestStoriesScore)
                                {
                                    bestStoriesScore = score;
                                    bestStoriesResponse = normalized;
                                }
                            }

                            emptyStoriesResponse ??= normalized;
                            System.Diagnostics.Debug.WriteLine($"[StoryApiService] {requestUrl.label} URL={requestUrl.value} returned empty stories; trying next endpoint.");
                            continue;
                        }

                        lastResponse = string.IsNullOrWhiteSpace(json) ? "Invalid story response" : json;
                    }
                }

                // Legacy fallback for installations where v2 returns empty while old app API still returns stories.
                var legacyUrls = BuildLegacyStoryUrls();
                foreach (var legacyUrl in legacyUrls)
                {
                    using var legacyContent = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId ?? string.Empty),
                        new KeyValuePair<string, string>("s", ResolveAccessToken()),
                        new KeyValuePair<string, string>("offset", offset ?? "0"),
                        new KeyValuePair<string, string>("limit", limit ?? "15"),
                        new KeyValuePair<string, string>("my_offset", offset ?? "0"),
                        new KeyValuePair<string, string>("access_token", ResolveAccessToken()),
                    });

                    var legacyResponse = await client.PostAsync(legacyUrl, legacyContent).ConfigureAwait(false);
                    var legacyJson = await legacyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var legacyPreview = legacyJson?.Length > 300 ? legacyJson.Substring(0, 300) : legacyJson;
                    System.Diagnostics.Debug.WriteLine($"[StoryApiService] legacy-stories URL={legacyUrl} HTTP={legacyResponse.StatusCode} raw={legacyPreview}");
                    try { if (legacyJson?.Length > 0) { var jo = Newtonsoft.Json.Linq.JObject.Parse(legacyJson); var st = jo["stories"] as Newtonsoft.Json.Linq.JArray; if (st != null) { foreach (var s in st) { var imgs = s["images"] as Newtonsoft.Json.Linq.JArray; Android.Util.Log.Warn("FON_STORY_API", $"Story id={s["id"]} images_count={(imgs?.Count ?? 0)} videos={((s["videos"] as Newtonsoft.Json.Linq.JArray)?.Count ?? 0)} thumb={s["thumbnail"]}"); if (imgs != null) { foreach (var img in imgs) Android.Util.Log.Warn("FON_STORY_API", $"  Image filename={img["filename"]}"); } } } } } catch { }

                    if (!legacyResponse.IsSuccessStatusCode)
                    {
                        lastResponse = string.IsNullOrWhiteSpace(legacyJson) ? legacyResponse.StatusCode.ToString() : legacyJson;
                        continue;
                    }

                    var normalizedLegacy = NormalizeStoriesResponse(legacyJson);
                    if (normalizedLegacy != null && normalizedLegacy.Status == 200)
                    {
                        if (normalizedLegacy.Stories?.Count > 0)
                        {
                            var score = ScoreStories(normalizedLegacy);
                            Android.Util.Log.Warn("FON_STORY_API", $"Legacy stories_count={normalizedLegacy.Stories.Count} score={score} url={legacyUrl}");
                            if (score > bestStoriesScore)
                            {
                                bestStoriesScore = score;
                                bestStoriesResponse = normalizedLegacy;
                            }
                        }

                        emptyStoriesResponse ??= normalizedLegacy;
                    }
                }

                // Final SDK retry in case manual route attempts are blocked by edge config.
                var (sdkStatus, sdkResponse) = await RequestsAsync.Story.GetUserStoriesAsync(limit, offset).ConfigureAwait(false);
                if (sdkStatus == 200)
                {
                    if (sdkResponse is GetUserStoriesObject sdkStories && sdkStories.Stories?.Count > 0)
                    {
                        var score = ScoreStories(sdkStories);
                        Android.Util.Log.Warn("FON_STORY_API", $"SDK retry stories_count={sdkStories.Stories.Count} score={score}");
                        if (score > bestStoriesScore)
                        {
                            bestStoriesScore = score;
                            bestStoriesResponse = sdkStories;
                        }
                    }
                }

                if (bestStoriesResponse?.Stories?.Count > 0)
                {
                    Android.Util.Log.Warn("FON_STORY_API", $"Returning best stories_count={bestStoriesResponse.Stories.Count} score={bestStoriesScore}");
                    return (200, bestStoriesResponse);
                }

                if (emptyStoriesResponse != null)
                    return (200, emptyStoriesResponse);

                return (400, lastResponse);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (404, e.Message);
            }
        }

        public static async Task<(int apiStatus, dynamic response)> CreateStoryAsync(string storyTitle, string storyDescription, string filePath, string fileType, string thumbnail)
        {
            try
            {
                var token = ResolveAccessToken();
                if (string.IsNullOrEmpty(token))
                    return (400, "Access token is missing");

                if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[StoryApiService] File missing: '{filePath}' exists={File.Exists(filePath ?? "")}");
                    return (400, "Story file is missing");
                }

                var fileInfo = new FileInfo(filePath);
                System.Diagnostics.Debug.WriteLine($"[StoryApiService] File: '{filePath}' size={fileInfo.Length} type={fileType}");

                // First try SDK route mapping (usually matches server-specific API wiring).
                var (sdkStatusInitial, sdkResponseInitial) = await RequestsAsync.Story.CreateStoryAsync(storyTitle, storyDescription, filePath, fileType, thumbnail).ConfigureAwait(false);
                if (sdkStatusInitial == 200)
                {
                    if (sdkResponseInitial is CreateStoryObject sdkCreated && !string.IsNullOrWhiteSpace(sdkCreated.StoryId))
                        return (sdkStatusInitial, sdkCreated);
                }

                using var client = new HttpClient();
                var urls = BuildStoryUrls("create-story");
                System.Diagnostics.Debug.WriteLine($"[StoryApiService] WebsiteUrl='{InitializeWoWonder.WebsiteUrl}', StoryUrls='{string.Join(" | ", urls)}'");

                string lastResponse = "Invalid story create response";
                string TryExtractStoryId(string json)
                {
                    try
                    {
                        var obj = JObject.Parse(json);
                        return obj["story_id"]?.ToString()
                               ?? obj["storyId"]?.ToString()
                               ?? obj["data"]?["story_id"]?.ToString()
                               ?? obj["data"]?["id"]?.ToString()
                               ?? obj["story"]?["id"]?.ToString()
                               ?? string.Empty;
                    }
                    catch
                    {
                        return string.Empty;
                    }
                }

                foreach (var url in urls)
                {
                    using var form = new MultipartFormDataContent();

                    form.Add(new StringContent(storyTitle ?? string.Empty), "story_title");
                    form.Add(new StringContent(storyDescription ?? string.Empty), "story_description");
                    form.Add(new StringContent(fileType ?? "image"), "file_type");

                    using var fileStream = File.OpenRead(filePath);
                    using var fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath.Split('.').Length > 0 ? filePath.Split('.')[^1] : "jpg"));
                    form.Add(fileContent, "file", Path.GetFileName(filePath));

                    var isVideo = (fileType ?? string.Empty).Equals("video", StringComparison.OrdinalIgnoreCase);
                    Stream coverStream = null;
                    StreamContent coverContent = null;
                    try
                    {
                        if (isVideo && !string.IsNullOrEmpty(thumbnail) && File.Exists(thumbnail) && !thumbnail.Contains("avatar"))
                        {
                            coverStream = File.OpenRead(thumbnail);
                            coverContent = new StreamContent(coverStream);
                            coverContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(thumbnail.Split('.').Length > 0 ? thumbnail.Split('.')[^1] : "jpg"));
                            form.Add(coverContent, "cover", Path.GetFileName(thumbnail));
                        }

                        var response = await client.PostAsync(url, form).ConfigureAwait(false);
                        var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                        var preview = json?.Length > 600 ? json.Substring(0, 600) : json;
                        System.Diagnostics.Debug.WriteLine($"[StoryApiService] create-story URL={url} HTTP={response.StatusCode} raw: {preview}");

                        if (!response.IsSuccessStatusCode)
                        {
                            lastResponse = string.IsNullOrWhiteSpace(json) ? response.StatusCode.ToString() : json;
                            continue;
                        }

                        try
                        {
                            var result = JsonConvert.DeserializeObject<CreateStoryObject>(json);
                            if (result != null && result.Status == 200)
                            {
                                if (!string.IsNullOrWhiteSpace(result.StoryId))
                                    return (200, result);

                                var extractedStoryId = TryExtractStoryId(json);
                                if (!string.IsNullOrWhiteSpace(extractedStoryId))
                                    return (200, new CreateStoryObject { Status = 200, StoryId = extractedStoryId });
                            }
                        }
                        catch (JsonException)
                        {
                            // JSON shape mismatch — fall through to raw response handling
                        }

                        // Fallback parse for APIs that return { api_status: "200", story_id: "..." }.
                        try
                        {
                            var obj = JObject.Parse(json);
                            var apiStatusToken = obj["api_status"];
                            var storyIdToken = obj["story_id"];
                            if (int.TryParse(apiStatusToken?.ToString(), out var normalizedStatus) && normalizedStatus == 200)
                            {
                                var storyId = storyIdToken?.ToString() ?? TryExtractStoryId(json);
                                if (string.IsNullOrWhiteSpace(storyId))
                                {
                                    lastResponse = "Create story succeeded but server did not return a story id";
                                    continue;
                                }

                                return (200, new CreateStoryObject
                                {
                                    Status = 200,
                                    StoryId = storyId
                                });
                            }
                        }
                        catch (Exception)
                        {
                            // Ignore shape normalization errors and continue trying routes.
                        }

                        lastResponse = string.IsNullOrWhiteSpace(json) ? "Invalid story create response" : json;
                    }
                    finally
                    {
                        coverContent?.Dispose();
                        coverStream?.Dispose();
                    }
                }

                // Legacy phone API fallback for servers where v2 route accepts upload but doesn't persist.
                var legacyCreateUrls = BuildBaseUrls()
                    .SelectMany(baseUrl => new[]
                    {
                        $"{baseUrl.TrimEnd('/')}/app_api.php?application=phone&type=create_story&s={Uri.EscapeDataString(token)}",
                        $"{baseUrl.TrimEnd('/')}/app_api.php?type=create_story&application=phone&s={Uri.EscapeDataString(token)}"
                    })
                    .Distinct()
                    .ToList();

                foreach (var legacyUrl in legacyCreateUrls)
                {
                    foreach (var fileFieldName in new[] { "file", "story_file", "story", "image", "video" })
                    {
                        using var legacyForm = new MultipartFormDataContent();
                        legacyForm.Add(new StringContent(InitializeWoWonder.ServerKey ?? string.Empty), "server_key");
                        legacyForm.Add(new StringContent(UserDetails.UserId ?? string.Empty), "user_id");
                        legacyForm.Add(new StringContent(token), "s");
                        legacyForm.Add(new StringContent(storyTitle ?? string.Empty), "story_title");
                        legacyForm.Add(new StringContent(storyDescription ?? string.Empty), "story_description");
                        legacyForm.Add(new StringContent(storyDescription ?? string.Empty), "description");
                        legacyForm.Add(new StringContent(fileType ?? "image"), "file_type");
                        legacyForm.Add(new StringContent(fileType ?? "image"), "type");

                        using var uploadStream = File.OpenRead(filePath);
                        using var uploadContent = new StreamContent(uploadStream);
                        uploadContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(filePath.Split('.').Length > 0 ? filePath.Split('.')[^1] : "jpg"));
                        legacyForm.Add(uploadContent, fileFieldName, Path.GetFileName(filePath));

                        if (!string.IsNullOrEmpty(thumbnail) && File.Exists(thumbnail) && !thumbnail.Contains("avatar"))
                        {
                            using var coverStream = File.OpenRead(thumbnail);
                            using var coverContent = new StreamContent(coverStream);
                            coverContent.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(thumbnail.Split('.').Length > 0 ? thumbnail.Split('.')[^1] : "jpg"));
                            legacyForm.Add(coverContent, "cover", Path.GetFileName(thumbnail));
                        }

                        var legacyResponse = await client.PostAsync(legacyUrl, legacyForm).ConfigureAwait(false);
                        var legacyJson = await legacyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var legacyPreview = legacyJson?.Length > 600 ? legacyJson.Substring(0, 600) : legacyJson;
                        System.Diagnostics.Debug.WriteLine($"[StoryApiService] legacy-create_story URL={legacyUrl} field={fileFieldName} HTTP={legacyResponse.StatusCode} raw: {legacyPreview}");

                        if (!legacyResponse.IsSuccessStatusCode)
                        {
                            lastResponse = string.IsNullOrWhiteSpace(legacyJson) ? legacyResponse.StatusCode.ToString() : legacyJson;
                            continue;
                        }

                        var extractedLegacyId = TryExtractStoryId(legacyJson);
                        if (!string.IsNullOrWhiteSpace(extractedLegacyId))
                            return (200, new CreateStoryObject { Status = 200, StoryId = extractedLegacyId });

                        try
                        {
                            var legacyObj = JObject.Parse(legacyJson);
                            var legacyApiStatus = legacyObj["api_status"]?.ToString() ?? legacyObj["status"]?.ToString();
                            if (legacyApiStatus is "200" or "201" or "1")
                            {
                                lastResponse = "Create story returned success without story id";
                            }
                        }
                        catch
                        {
                            // ignore parse errors and continue candidate routes
                        }
                    }
                }

                // Fallback to SDK endpoint implementation (obfuscated internal routes).
                var (sdkStatus, sdkResponse) = await RequestsAsync.Story.CreateStoryAsync(storyTitle, storyDescription, filePath, fileType, thumbnail).ConfigureAwait(false);
                if (sdkStatus == 200)
                {
                    if (sdkResponse is CreateStoryObject sdkCreated && !string.IsNullOrWhiteSpace(sdkCreated.StoryId))
                        return (sdkStatus, sdkCreated);
                }

                return (400, lastResponse);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (404, e.Message);
            }
        }

        public static async Task TrackStoryViewAsync(string storyId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storyId))
                    return;

                var token = ResolveAccessToken();
                if (string.IsNullOrWhiteSpace(token))
                    return;

                var userId = UserDetails.UserId ?? string.Empty;

                foreach (var baseUrl in BuildBaseUrls())
                {
                    var cleanBase = baseUrl.TrimEnd('/');
                    var url = $"{cleanBase}/app_api.php?application=phone&type=update_story_view";

                    using var client = new HttpClient();
                    using var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("user_id", userId),
                        new KeyValuePair<string, string>("s", token),
                        new KeyValuePair<string, string>("story_id", storyId),
                    });

                    var response = await client.PostAsync(url, content).ConfigureAwait(false);
                    if (response.IsSuccessStatusCode)
                    {
                        Android.Util.Log.Warn("FON_STORY_FLOW", $"TrackStoryView storyId={storyId} HTTP={response.StatusCode}");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
            }
        }
    }
}
