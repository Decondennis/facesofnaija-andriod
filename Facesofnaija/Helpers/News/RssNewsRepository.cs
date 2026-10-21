using Android.App;
using Android.Content;
using Android.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.Helpers.News
{
    public class NewsItem
    {
        [JsonProperty("id")]
        public object Id { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; } = string.Empty;

        [JsonProperty("title")]
        public string Title { get; set; } = string.Empty;

        [JsonProperty("text")]
        public string Text { get; set; } = string.Empty;

        [JsonProperty("link")]
        public string Link { get; set; } = string.Empty;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("time")]
        public long Time { get; set; }

        [JsonProperty("pubDateMillis")]
        public long PubDateMillis { get; set; }

        [JsonProperty("time_text")]
        public string TimeText { get; set; } = string.Empty;
    }

    public class NewsTickerApiResponse
    {
        [JsonProperty("api_status")]
        public int ApiStatus { get; set; }

        [JsonProperty("api_text")]
        public string ApiText { get; set; }

        [JsonProperty("ticker_text")]
        public string TickerText { get; set; }

        [JsonProperty("data")]
        public List<NewsItem> Data { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public static class RssNewsRepository
    {
        private const string PrefsName = "rss_ticker_prefs";
        private const string CacheKey = "cached_headlines_json";
        private const string CacheTimeKey = "cached_timestamp_ms";
        private const long CacheDurationMs = 15 * 60 * 1000L; // 15 minutes, matching PDF spec

        private static readonly (string Source, string Url)[] Feeds = new[]
        {
            ("PunchNG", "https://rss.punchng.com/v1/category/latest_news"),
            ("Vanguard", "https://www.vanguardngr.com/feed/"),
            ("PremiumTimes", "https://www.premiumtimesng.com/feed"),
            ("ChannelsTV", "https://www.channelstv.com/feed/"),
            ("BBCWorld", "https://feeds.bbci.co.uk/news/world/rss.xml"),
            ("AlJazeera", "https://www.aljazeera.com/xml/rss/all.xml")
        };

        private static readonly object LockObj = new object();
        private static List<NewsItem> MemoryCache = new List<NewsItem>();
        private static long MemoryCacheTime = 0;

        public static string GetCachedTickerString()
        {
            try
            {
                var cached = LoadFromCache(ignoreExpiry: true);
                if (cached != null && cached.Count > 0)
                {
                    return FormatTicker(cached);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static async Task<string> GetTickerStringAsync(bool forceRefresh = false)
        {
            try
            {
                var headlines = await GetHeadlinesAsync(forceRefresh);
                if (headlines != null && headlines.Count > 0)
                {
                    return FormatTicker(headlines);
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return string.Empty;
        }

        public static async Task<List<NewsItem>> GetHeadlinesAsync(bool forceRefresh = false)
        {
            try
            {
                if (!forceRefresh)
                {
                    var cached = LoadFromCache(ignoreExpiry: false);
                    if (cached != null && cached.Count > 0)
                    {
                        return cached;
                    }
                }

                // Tier 1: Try server API endpoint (the web app as source of truth)
                var serverNews = await FetchFromServerApiAsync();
                if (serverNews != null && serverNews.Count > 0)
                {
                    SaveToCache(serverNews);
                    return serverNews;
                }

                // Tier 2: Directly fetch and parse the 6 RSS feeds in parallel (as per PDF Part 3)
                var directNews = await FetchDirectFeedsAsync();
                if (directNews != null && directNews.Count > 0)
                {
                    SaveToCache(directNews);
                    return directNews;
                }

                // Tier 3: Fallback to expired cache
                var expired = LoadFromCache(ignoreExpiry: true);
                if (expired != null && expired.Count > 0)
                {
                    return expired;
                }

                // Tier 4: Fallback seed headlines so marquee ticker is never empty
                return GetSeedHeadlines();
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return LoadFromCache(ignoreExpiry: true) ?? GetSeedHeadlines();
            }
        }

        private static async Task<List<NewsItem>> FetchFromServerApiAsync()
        {
            try
            {
                var baseUrl = AppSettings.SiteUrl?.TrimEnd('/');
                if (string.IsNullOrWhiteSpace(baseUrl))
                    baseUrl = "https://www.facesofnaija.ng";

                var endpoint = $"{baseUrl}/api-v2.php?type=get_news_ticker";
                using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler())
                {
                    Timeout = TimeSpan.FromSeconds(8)
                };
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36");
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");

                var response = await client.GetAsync(endpoint);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(json))
                    {
                        var apiRes = JsonConvert.DeserializeObject<NewsTickerApiResponse>(json);
                        if (apiRes?.ApiStatus == 200 && apiRes.Data != null && apiRes.Data.Count > 0)
                        {
                            foreach (var item in apiRes.Data)
                            {
                                if (string.IsNullOrWhiteSpace(item.Text) && !string.IsNullOrWhiteSpace(item.Title))
                                    item.Text = item.Title;
                                if (string.IsNullOrWhiteSpace(item.Url) && !string.IsNullOrWhiteSpace(item.Link))
                                    item.Url = item.Link;
                            }
                            return apiRes.Data;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RssNewsRepository] Server API fetch notice: {ex.Message}");
            }
            return null;
        }

        private static async Task<List<NewsItem>> FetchDirectFeedsAsync()
        {
            try
            {
                var tasks = Feeds.Select(feed => FetchSingleFeedAsync(feed.Source, feed.Url, limit: 4)).ToList();
                var results = await Task.WhenAll(tasks);

                var combined = results
                    .Where(list => list != null)
                    .SelectMany(list => list)
                    .OrderByDescending(item => item.PubDateMillis)
                    .ToList();

                return combined;
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
                return null;
            }
        }

        private static async Task<List<NewsItem>> FetchSingleFeedAsync(string source, string urlString, int limit = 4)
        {
            var items = new List<NewsItem>();
            try
            {
                using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler())
                {
                    Timeout = TimeSpan.FromSeconds(6)
                };
                client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Mobile Safari/537.36");

                var xml = await client.GetStringAsync(urlString);
                if (string.IsNullOrWhiteSpace(xml))
                    return items;

                var doc = XDocument.Parse(xml);
                var itemElements = doc.Descendants("item").Take(limit);

                foreach (var el in itemElements)
                {
                    var rawTitle = el.Element("title")?.Value ?? string.Empty;
                    var rawLink = el.Element("link")?.Value ?? string.Empty;
                    var rawPubDate = el.Element("pubDate")?.Value ?? string.Empty;

                    var cleanTitle = CleanHtmlText(rawTitle);
                    var cleanLink = rawLink.Trim();

                    long pubMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (DateTimeOffset.TryParse(rawPubDate, out var parsedDate))
                    {
                        pubMillis = parsedDate.ToUnixTimeMilliseconds();
                    }

                    if (!string.IsNullOrWhiteSpace(cleanTitle) && !string.IsNullOrWhiteSpace(cleanLink))
                    {
                        items.Add(new NewsItem
                        {
                            Source = source,
                            Title = cleanTitle,
                            Text = cleanTitle,
                            Link = cleanLink,
                            Url = cleanLink,
                            PubDateMillis = pubMillis,
                            Time = pubMillis / 1000L,
                            TimeText = "Recent"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RssNewsRepository] Feed {source} fetch error: {ex.Message}");
            }
            return items;
        }

        private static string CleanHtmlText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            try
            {
                var spanned = Html.FromHtml(input.Trim(), FromHtmlOptions.ModeLegacy);
                return spanned?.ToString()?.Trim() ?? input.Trim();
            }
            catch
            {
                return input.Replace("&amp;", "&").Replace("&quot;", "\"").Replace("&#039;", "'").Trim();
            }
        }

        private static string FormatTicker(List<NewsItem> items)
        {
            if (items == null || items.Count == 0)
                return string.Empty;

            var parts = new List<string>();
            foreach (var item in items)
            {
                var src = !string.IsNullOrWhiteSpace(item.Source) ? item.Source.ToUpperInvariant() : "NEWS";
                var title = !string.IsNullOrWhiteSpace(item.Title) ? item.Title : item.Text;
                if (!string.IsNullOrWhiteSpace(title))
                {
                    parts.Add($"[{src}] {title.Trim()}");
                }
            }
            return string.Join("   •   ", parts);
        }

        private static void SaveToCache(List<NewsItem> items)
        {
            try
            {
                lock (LockObj)
                {
                    MemoryCache = items;
                    MemoryCacheTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }

                var context = Application.Context;
                var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
                var editor = prefs?.Edit();
                if (editor != null)
                {
                    var json = JsonConvert.SerializeObject(items);
                    editor.PutString(CacheKey, json);
                    editor.PutLong(CacheTimeKey, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                    editor.Apply();
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
        }

        private static List<NewsItem> LoadFromCache(bool ignoreExpiry = false)
        {
            try
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                lock (LockObj)
                {
                    if (MemoryCache != null && MemoryCache.Count > 0)
                    {
                        if (ignoreExpiry || (now - MemoryCacheTime <= CacheDurationMs))
                        {
                            return MemoryCache;
                        }
                    }
                }

                var context = Application.Context;
                var prefs = context.GetSharedPreferences(PrefsName, FileCreationMode.Private);
                if (prefs == null)
                    return new List<NewsItem>();

                var lastSaved = prefs.GetLong(CacheTimeKey, 0L);
                if (!ignoreExpiry && (now - lastSaved > CacheDurationMs))
                {
                    return new List<NewsItem>();
                }

                var json = prefs.GetString(CacheKey, null);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<NewsItem>();

                var items = JsonConvert.DeserializeObject<List<NewsItem>>(json);
                if (items != null && items.Count > 0)
                {
                    lock (LockObj)
                    {
                        MemoryCache = items;
                        MemoryCacheTime = lastSaved;
                    }
                    return items;
                }
            }
            catch (Exception ex)
            {
                Methods.DisplayReportResultTrack(ex);
            }
            return new List<NewsItem>();
        }

        private static List<NewsItem> GetSeedHeadlines()
        {
            return new List<NewsItem>
            {
                new NewsItem { Source = "PunchNG", Title = "Ooni, Omisore mourn Afenifere leader Fasoranti", Text = "Ooni, Omisore mourn Afenifere leader Fasoranti", Link = "https://punchng.com", Url = "https://punchng.com" },
                new NewsItem { Source = "Vanguard", Title = "Ebola: Senate urges FG to make screening mandatory at all entry points", Text = "Ebola: Senate urges FG to make screening mandatory at all entry points", Link = "https://www.vanguardngr.com", Url = "https://www.vanguardngr.com" },
                new NewsItem { Source = "ChannelsTV", Title = "Gov Otu Presents N987.8bn 2027 Budget For Capital Projects", Text = "Gov Otu Presents N987.8bn 2027 Budget For Capital Projects", Link = "https://www.channelstv.com", Url = "https://www.channelstv.com" },
                new NewsItem { Source = "PremiumTimes", Title = "WHO pledges support as Nigeria records first imported Ebola case", Text = "WHO pledges support as Nigeria records first imported Ebola case", Link = "https://www.premiumtimesng.com", Url = "https://www.premiumtimesng.com" },
                new NewsItem { Source = "BBCWorld", Title = "Trump and Zelensky at loggerheads despite new optimism", Text = "Trump and Zelensky at loggerheads despite new optimism", Link = "https://www.bbc.co.uk/news", Url = "https://www.bbc.co.uk/news" },
                new NewsItem { Source = "AlJazeera", Title = "France's Macron says protesters won't be allowed to cause chaos", Text = "France's Macron says protesters won't be allowed to cause chaos", Link = "https://www.aljazeera.com", Url = "https://www.aljazeera.com" }
            };
        }
    }
}
