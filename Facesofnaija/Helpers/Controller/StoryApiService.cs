using Newtonsoft.Json;
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
                if (result != null && result.Status == 200)
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
                                Images = new List<StoryDataObject.Image>(),
                                ViewCount = row["view_count"]?.ToString() ?? "0",
                                TypeView = videos.Count > 0 ? "Video" : "Image"
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

                            var images = new List<StoryDataObject.Image>();

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
                                Images = images,
                                ViewCount = row["view_count"]?.ToString() ?? "0",
                                TypeView = videos.Count > 0 ? "Video" : "Image"
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
            bases.Add("http://172.236.19.52");

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
                if (string.IsNullOrEmpty(ResolveAccessToken()))
                    return (400, "Access token is missing");

                // First try SDK route mapping (usually matches server-specific API wiring).
                var (sdkStatusInitial, sdkResponseInitial) = await RequestsAsync.Story.GetUserStoriesAsync(limit, offset).ConfigureAwait(false);
                if (sdkStatusInitial == 200)
                {
                    if (sdkResponseInitial is GetUserStoriesObject sdkStories && sdkStories.Stories?.Count > 0)
                        return (200, sdkStories);

                    System.Diagnostics.Debug.WriteLine("[StoryApiService] SDK returned empty stories; trying endpoint fallbacks.");
                }

                using var client = new HttpClient();
                var urls = new List<string>();
                urls.AddRange(BuildStoryUrls("get-user-stories"));
                urls.AddRange(BuildStoryUrls("get-stories"));
                urls = urls.Distinct().ToList();
                System.Diagnostics.Debug.WriteLine($"[StoryApiService] WebsiteUrl='{InitializeWoWonder.WebsiteUrl}', StoryUrls='{string.Join(" | ", urls)}'");

                string lastResponse = "Invalid story response";
                GetUserStoriesObject emptyStoriesResponse = null;
                foreach (var url in urls)
                {
                    var requestUrls = new List<(string label, string value)>
                    {
                        ("POST", url),
                        ("GET", $"{url}&limit={Uri.EscapeDataString(limit ?? "15")}&offset={Uri.EscapeDataString(offset ?? "0")}")
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
                                return (200, normalized);

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
                        new KeyValuePair<string, string>("my_offset", offset ?? "0"),
                        new KeyValuePair<string, string>("access_token", ResolveAccessToken()),
                    });

                    var legacyResponse = await client.PostAsync(legacyUrl, legacyContent).ConfigureAwait(false);
                    var legacyJson = await legacyResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var legacyPreview = legacyJson?.Length > 300 ? legacyJson.Substring(0, 300) : legacyJson;
                    System.Diagnostics.Debug.WriteLine($"[StoryApiService] legacy-stories URL={legacyUrl} HTTP={legacyResponse.StatusCode} raw={legacyPreview}");

                    if (!legacyResponse.IsSuccessStatusCode)
                    {
                        lastResponse = string.IsNullOrWhiteSpace(legacyJson) ? legacyResponse.StatusCode.ToString() : legacyJson;
                        continue;
                    }

                    var normalizedLegacy = NormalizeStoriesResponse(legacyJson);
                    if (normalizedLegacy != null && normalizedLegacy.Status == 200)
                    {
                        if (normalizedLegacy.Stories?.Count > 0)
                            return (200, normalizedLegacy);

                        emptyStoriesResponse ??= normalizedLegacy;
                    }
                }

                // Final SDK retry in case manual route attempts are blocked by edge config.
                var (sdkStatus, sdkResponse) = await RequestsAsync.Story.GetUserStoriesAsync(limit, offset).ConfigureAwait(false);
                if (sdkStatus == 200)
                {
                    if (sdkResponse is GetUserStoriesObject sdkStories && sdkStories.Stories?.Count > 0)
                        return (200, sdkStories);
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
                if (string.IsNullOrEmpty(Current.AccessToken))
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
                    return (sdkStatusInitial, sdkResponseInitial);

                using var client = new HttpClient();
                var urls = BuildStoryUrls("create-story");
                System.Diagnostics.Debug.WriteLine($"[StoryApiService] WebsiteUrl='{InitializeWoWonder.WebsiteUrl}', StoryUrls='{string.Join(" | ", urls)}'");

                string lastResponse = "Invalid story create response";
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
                                return (200, result);
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
                                return (200, new CreateStoryObject
                                {
                                    Status = 200,
                                    StoryId = storyIdToken?.ToString() ?? string.Empty
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

                // Fallback to SDK endpoint implementation (obfuscated internal routes).
                var (sdkStatus, sdkResponse) = await RequestsAsync.Story.CreateStoryAsync(storyTitle, storyDescription, filePath, fileType, thumbnail).ConfigureAwait(false);
                if (sdkStatus == 200)
                    return (sdkStatus, sdkResponse);

                return (400, lastResponse);
            }
            catch (Exception e)
            {
                Methods.DisplayReportResultTrack(e);
                return (404, e.Message);
            }
        }
    }
}
