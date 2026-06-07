using System.Threading.Tasks;
using System.Net.Http;
using Android.Util;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WoWonderClient;
using Facesofnaija.CustomApi.Classes.Community;
using Facesofnaija.CustomApi.Classes.Global;
using Facesofnaija.Helpers.Model;
using System.Collections.Generic;
using System.Linq;
using WoWonderClient.Requests;
using WoWonderClient.Classes.Global;
using WoWonderClient.Classes.Posts;
using WoWonderClient.Classes.Comments;
using System.Collections.ObjectModel;
using System.IO;
using Facesofnaija.CustomApi.Classes.Search;
using System.Net.Http.Headers;
using Facesofnaija.Helpers.Utils;

namespace Facesofnaija.CustomApi.Requests
{
    public class CustomRequests
    {
        public static class Community
        {
            public static string WebsiteUrl { get; set; }
            public static string UserId { get; set; }
            public static string AccessToken { get; set; }

            internal static async Task<(long?, dynamic)> GetCommunitiesByFetchAsync(string fetch)
            {
                try
                {
                    Console.WriteLine("FON_COMM " + fetch + " start");
                    using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(30) };
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    var url = "http://172.236.19.52/api/get-community?access_token=" + Uri.EscapeDataString(UserDetails.AccessToken ?? "");
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", fetch),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId ?? "")
                    });

                    var response = await client.PostAsync(url, content);
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("FON_COMM " + fetch + " done");

                    if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
                    {
                        var list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                        if (list != null)
                            return (list.Status, list);
                    }

                    Console.WriteLine("FON_COMM " + fetch + " FAILED response=" + (int)response.StatusCode + " json=" + (json ?? "null"));
                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FON_COMM " + fetch + " EXCEPTION=" + e.Message);
                    return (404, e.Message);
                }
            }

            public static Task<(long?, dynamic)> GetJoinedCommunitiesAsync()//string userId, string offset, string limit
            {
                Console.WriteLine("This is GetJoinedCommunitiesAsync");
                return GetCommunitiesByFetchAsync("joined_communities");
            }

            public static async Task<(long?, dynamic)> GetMyCommunitiesAsync(string offset, string limit)
            {
                try
                {
                    Console.WriteLine("This is the joined communities");
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_communities"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetRecommendedCommunitiesAsync(string limit, string offset)
            {
                Console.WriteLine("This is GetRecommendedCommunitiesAsync");
                return await GetCommunitiesByFetchAsync("random_communities");
            }

            public static async Task<(long?, dynamic)> GetSuggestedCommunitiesAsync(string limit = "20", string offset = "")
            {
                return await GetCommunitiesByFetchAsync("random_communities");
            }

            public static async Task<(long?, dynamic)> GetRequestedCommunitiesAsync()
            {
                try
                {
                    Console.WriteLine("FON_COMM requested_communities start");
                    using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(30) };
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    var url = "http://172.236.19.52/api/get-community?access_token=" + Uri.EscapeDataString(UserDetails.AccessToken ?? "");
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "requested_communities"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId ?? "")
                    });

                    var response = await client.PostAsync(url, content);
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("FON_COMM requested_communities done");

                    if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
                    {
                        var list = JsonConvert.DeserializeObject<ListCommunityRequestsObject>(json);
                        if (list != null)
                            return (list.Status, list);
                    }

                    Console.WriteLine("FON_COMM requested_communities FAILED response=" + (int)response.StatusCode + " json=" + (json ?? "null"));
                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FON_COMM requested_communities EXCEPTION=" + e.Message);
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetAllCommunitiesAsync()
            {
                return await GetCommunitiesByFetchAsync("random_communities");
            }

            public static async Task<(long?, dynamic)> GetCommunityDataAsync(string communityId)
            {
                Console.WriteLine("This is GetCommunityDataAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("community_id", communityId),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community-data?access_token=" + UserDetails.AccessToken, content);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    GetCommunityDataObject list = JsonConvert.DeserializeObject<GetCommunityDataObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetCommunityJoinRequestsAsync(string communityId, string limit = "20", string offset = "")
            {
                Console.WriteLine("This is GetCommunityJoinRequestsAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    GetCommunityDataObject list = JsonConvert.DeserializeObject<GetCommunityDataObject>(json);
                    //GetCommunityJoinRequestsObject list = JsonConvert.DeserializeObject<GetCommunityJoinRequestsObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetCommunityMembersAsync(string communityId, string limit = "20", string offset = "")
            {
                Console.WriteLine("This is GetCommunityMembersAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetCommunitiesByCategoryAsync(string category, string limit, string offset)
            {
                Console.WriteLine("This is GetCommunitiesByCategoryAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetNotInCommunityMembersAsync(string communityId)
            {
                Console.WriteLine("This is GetNotInCommunityMembersAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> JoinCommunityAsync(string communityId)
            {
                Console.WriteLine("FON_COMM join start id=" + communityId);
                try
                {
                    using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(30) };
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    var url = "http://172.236.19.52/api/join-community?access_token=" + Uri.EscapeDataString(UserDetails.AccessToken ?? "");
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey ?? ""),
                        new KeyValuePair<string, string>("community_id", communityId ?? "")
                    });
                    var response = await client.PostAsync(url, content);
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("FON_COMM join done json=" + json);

                    if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
                    {
                        JoinCommunityObject list = JsonConvert.DeserializeObject<JoinCommunityObject>(json);
                        if (list != null)
                            return (list.Status, list);
                    }

                    Console.WriteLine("FON_COMM join FAILED response=" + (int)response.StatusCode + " json=" + (json ?? "null"));
                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    Console.WriteLine("FON_COMM join EXCEPTION=" + e.Message);
                    return (404, e.Message);
                }
            }

            public static Task<(long?, dynamic)> LeaveCommunityAsync(string communityId)
            {
                return JoinCommunityAsync(communityId);
            }

            public static Task<(long?, dynamic)> CancelCommunityRequestAsync(string communityId)
            {
                return JoinCommunityAsync(communityId);
            }

            public static async Task<(long?, dynamic)> JoinRequestActionAsync(string communityId, string userId, bool requestAction)
            {
                Console.WriteLine("This is JoinRequestActionAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }
                    /*var list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list.Data);
                    }*/

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }
            public static async Task<(long?, dynamic)> ReportCommunityAsync(string communityId, string text)
            {
                Console.WriteLine("This is ReportCommunityAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> UpdateCommunityAvatarAsync(string CommunityId, string filePath)
            {

                Console.WriteLine("This is UpdateCommunityAvatarAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> UpdateCommuityCoverAsync(string communityId, string filePath)
            {
                Console.WriteLine("This is UpdateCommuityCoverAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> RemoveCommunityMembersAsync(string communityId, string userId)
            {
                Console.WriteLine("This is RemoveCommunityMembersAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> CommunityAddAsync(string communityId, string userId)
            {
                Console.WriteLine("This is CommunityAddAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> UpdateCommunityDataAsync(string communityId, Dictionary<string, string> communitySettings)
            {
                Console.WriteLine("This is UpdateCommunityDataAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "joined_groups"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }

            public static async Task<(long?, dynamic)> GetRandomCommunitiesAsync()//string userId, string offset, string limit
            {
                Console.WriteLine("This is GetRandomCommunitiesAsync");
                try
                {
                    Console.WriteLine("This is the random communities");
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("fetch", "random_communities"),
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken, content);
                    Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/get-community?access_token=" + UserDetails.AccessToken);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                    ListCommunitiesObject list = JsonConvert.DeserializeObject<ListCommunitiesObject>(json);
                    if (list != null)
                    {
                        return (list.Status, list);
                    }

                    var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                    return (400, error);
                }
                catch (Exception e)
                {
                    return (404, e.Message);
                }
            }
        }

        public static async Task<(long?, dynamic)> GetCutomSearchAsync(Dictionary<string, string> dictionary)//string userId, string offset, string limit
        {
            Console.WriteLine("This is GetCutomSearchAsync");
            try
            {
                dictionary.Add("server_key", InitializeWoWonder.ServerKey);

                var client = new HttpClient();
                var content = new FormUrlEncodedContent(dictionary);
                var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/search?access_token=" + UserDetails.AccessToken, content);
                Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/search?access_token=" + UserDetails.AccessToken);
                string json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json);
                GetCustomSearchObject list = JsonConvert.DeserializeObject<GetCustomSearchObject>(json);
                if (list != null)
                {
                    return (list.Status, list);
                }

                var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                return (400, error);
            }
            catch (Exception e)
            {
                return (404, e.Message);
            }
        }

        public static async Task<(long?, dynamic)> GetCommunityNames()
        {
            try
            {
                foreach (var baseUrl in GetApiBaseCandidates())
                {
                    var endpoint = $"{baseUrl}/api/communities-custom?access_token={UserDetails.AccessToken}";
                    try
                    {
                        using var client = new HttpClient();
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                            new KeyValuePair<string, string>("user_id", UserDetails.UserId)
                        });

                        var response = await client.PostAsync(endpoint, content);
                        var json = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(endpoint);

                        var token = JObject.Parse(json);
                        var status = token["api_status"]?.Value<long?>() ?? 0;
                        var data = token["data"]?.ToString();

                        if (status == 200)
                        {
                            return (200, new CommunityNames
                            {
                                Status = 200,
                                Data = data ?? string.Empty
                            });
                        }
                    }
                    catch
                    {
                        // Try the next base URL.
                    }
                }

                return (404, "communities-custom endpoint failed on all base URLs");
            }
            catch (Exception e)
            {
                return (404, e.Message);
            }
        }

        public static async Task<(long?, dynamic)> RequestCommunity(Dictionary<string, string> dictionary)
        {
            try
            {
                using var client = new HttpClient(new Xamarin.Android.Net.AndroidMessageHandler()) { Timeout = TimeSpan.FromSeconds(30) };
                client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                var url = "http://172.236.19.52/api/request-community?access_token=" + Uri.EscapeDataString(UserDetails.AccessToken ?? "");
                var content = new FormUrlEncodedContent(dictionary);
                Console.WriteLine("FON_COMM request-community url=" + url);
                
                var response = await client.PostAsync(url, content);
                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine("FON_COMM request-community response=" + json);

                if (response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(json))
                {
                    CommunityRequst comRquest = JsonConvert.DeserializeObject<CommunityRequst>(json);
                    if (comRquest != null)
                        return (comRquest.Status, comRquest);
                }

                var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                return (400, error);
            }
            catch (Exception e)
            {
                Console.WriteLine("FON_COMM request-community EXCEPTION=" + e.Message);
                return (404, e.Message);
            }
        }

        public static async Task<string> GetAnnouncement()
        {
            try
            {
                // Try SDK path first.
                var (apiStatus, respond) = await RequestsAsync.Global.GetGeneralDataAsync(true, UserDetails.OnlineUsers, UserDetails.DeviceId, "0", "announcement");
                if (apiStatus == 200)
                {
                    if (respond is GetGeneralDataObject result)
                    {
                        if (!string.IsNullOrEmpty(result.Announcement?.AnnouncementClass?.TextDecode))
                        {
                           return result.Announcement?.AnnouncementClass?.TextDecode;
                        }
                    }
                }

                // Fallback: call the same endpoint style used by the web app and parse raw JSON.
                foreach (var baseUrl in GetApiBaseCandidates())
                {
                    var endpoint = $"{baseUrl}/api/get-general-data?access_token={UserDetails.AccessToken}";
                    try
                    {
                        using var client = new HttpClient();
                        var content = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("fetch", "announcement"),
                            new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey)
                        });

                        var response = await client.PostAsync(endpoint, content);
                        var json = await response.Content.ReadAsStringAsync();

                        var token = JObject.Parse(json);
                        var status = token["api_status"]?.Value<long?>() ?? 0;
                        if (status != 200)
                            continue;

                        var ann = token["announcement"] as JObject;
                        var textDecode = ann?["text_decode"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(textDecode))
                            return textDecode;

                        var text = ann?["text"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                    catch
                    {
                        // Try the next base URL.
                    }
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        private static IEnumerable<string> GetApiBaseCandidates()
        {
            var candidates = new List<string>();

            void Add(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                    return;

                var normalized = value.Trim().TrimEnd('/');
                if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    normalized = "http://" + normalized;

                candidates.Add(normalized);
            }

            Add(InitializeWoWonder.WebsiteUrl);
            Add(Community.WebsiteUrl);
            Add("http://172.236.19.52");

            return candidates.Distinct(StringComparer.OrdinalIgnoreCase);
        }

        // ---------------------------------------------------------------------------
        // Custom Posts � bypasses the SDK's internal HttpClient which points to the
        // wrong domain (facesofnaija.com) and instead uses InitializeWoWonder.WebsiteUrl
        // (the working IP from analytic.xml).
        // ---------------------------------------------------------------------------
        public static class Posts
            {
                private static string GetApiBase()
                {
                    // Posting is currently broken on the domain endpoint but works on the server IP.
                    return "http://172.236.19.52";
                }

                public static async Task<(int, dynamic)> AddNewPostAsync(
                    string userId, string postId, string pagePost, string textContent,
                    string privacy, string feelingType, string feeling, string location,
                    ObservableCollection<Attachments> postAttachments,
                    ObservableCollection<PollAnswers> pollAnswersList,
                    string idColor, string albumName)
                {
                    try
                    {
                        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId))
                        {
                            return (400, "Session is missing. Please sign in again.");
                        }

                        var endpoints = new[]
                        {
                            $"{GetApiBase()}/app_api.php?application=phone&type=new_post",
                            $"{GetApiBase()}/app_api.php?type=new_post"
                        };


                        string lastError = "Bad request.";
                        var hasExplicitApiError = false;
                        foreach (var url in endpoints)
                        {
                            using var form = new MultipartFormDataContent(Guid.NewGuid().ToString());
                            form.Add(new StringContent(InitializeWoWonder.ServerKey ?? string.Empty), "server_key");
                            form.Add(new StringContent(userId ?? string.Empty), "user_id");
                            form.Add(new StringContent(sessionId ?? string.Empty), "s");

                            var normalizedPrivacy = string.IsNullOrWhiteSpace(privacy) ? "0" : privacy;
                            form.Add(new StringContent(textContent ?? string.Empty), "postText");
                            form.Add(new StringContent(normalizedPrivacy), "postPrivacy");
                            form.Add(new StringContent("0"), "community_id");

                            if (!string.IsNullOrEmpty(feelingType))
                                form.Add(new StringContent(feelingType), "feeling_type");
                            if (!string.IsNullOrEmpty(feeling))
                                form.Add(new StringContent(feeling), "feeling");
                            if (!string.IsNullOrEmpty(location))
                                form.Add(new StringContent(location), "postMap");
                            if (!string.IsNullOrEmpty(idColor))
                                form.Add(new StringContent(idColor), "post_color");
                            if (!string.IsNullOrEmpty(albumName))
                                form.Add(new StringContent(albumName), "album_name");

                            if (!string.IsNullOrEmpty(pagePost) && !string.IsNullOrEmpty(postId))
                            {
                                if (pagePost.Contains("SocialGroup", StringComparison.OrdinalIgnoreCase) || pagePost.Contains("Group", StringComparison.OrdinalIgnoreCase))
                                    form.Add(new StringContent(postId), "group_id");
                                else if (pagePost.Contains("SocialPage", StringComparison.OrdinalIgnoreCase) || pagePost.Contains("Page", StringComparison.OrdinalIgnoreCase))
                                    form.Add(new StringContent(postId), "page_id");
                                else if (pagePost.Contains("SocialEvent", StringComparison.OrdinalIgnoreCase) || pagePost.Contains("Event", StringComparison.OrdinalIgnoreCase))
                                    form.Add(new StringContent(postId), "event_id");
                                else if (pagePost.Contains("Normal", StringComparison.OrdinalIgnoreCase))
                                {
                                    if (!string.Equals(userId, postId, StringComparison.OrdinalIgnoreCase))
                                        form.Add(new StringContent(postId), "recipient_id");
                                }
                            }

                            if (pollAnswersList != null)
                            {
                                foreach (var answer in pollAnswersList)
                                {
                                    if (!string.IsNullOrEmpty(answer?.Answer))
                                        form.Add(new StringContent(answer.Answer), "answer[]");
                                }
                            }

                            if (postAttachments != null && postAttachments.Count > 0)
                            {
                                foreach (var att in postAttachments)
                                {
                                    if (string.IsNullOrEmpty(att?.FileUrl)) continue;

                                    var fileName = att.FileName;
                                    if (string.IsNullOrWhiteSpace(fileName))
                                        fileName = Path.GetFileName(att.FileUrl);

                                    if (string.IsNullOrWhiteSpace(fileName))
                                        fileName = string.Equals(att.TypeAttachment, "postVideo", StringComparison.OrdinalIgnoreCase) ? "video.mp4" : "file.bin";

                                    var typeAttachment = att.TypeAttachment;
                                    if (string.Equals(typeAttachment, "postFile", StringComparison.OrdinalIgnoreCase) && IsVideoExtension(fileName))
                                        typeAttachment = "postVideo";

                                    var resolvedMime = GetMimeType(fileName);
                                    if (string.Equals(typeAttachment, "postVideo", StringComparison.OrdinalIgnoreCase) && string.Equals(resolvedMime, "application/octet-stream", StringComparison.OrdinalIgnoreCase))
                                        resolvedMime = "video/mp4";

                                    if (att.FileStream != null)
                                    {
                                        att.FileStream.Position = 0;
                                        using var ms = new MemoryStream();
                                        att.FileStream.CopyTo(ms);
                                        var bytes = ms.ToArray();
                                        var fileContent = new ByteArrayContent(bytes);
                                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(resolvedMime);
                                        form.Add(fileContent, typeAttachment, fileName);

                                        if (att.Thumb?.FileUrl != null && File.Exists(att.Thumb.FileUrl))
                                        {
                                            var thumbBytes = File.ReadAllBytes(att.Thumb.FileUrl);
                                            var thumbName = Path.GetFileName(att.Thumb.FileUrl);
                                            var thumbContent = new ByteArrayContent(thumbBytes);
                                            thumbContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(thumbName));
                                            form.Add(thumbContent, att.Thumb.TypeAttachment, thumbName);
                                        }
                                    }
                                    else if (File.Exists(att.FileUrl))
                                    {
                                        var bytes = File.ReadAllBytes(att.FileUrl);
                                        var fileContent = new ByteArrayContent(bytes);
                                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(resolvedMime);
                                        form.Add(fileContent, typeAttachment, fileName);

                                        if (att.Thumb?.FileUrl != null && File.Exists(att.Thumb.FileUrl))
                                        {
                                            var thumbBytes = File.ReadAllBytes(att.Thumb.FileUrl);
                                            var thumbName = Path.GetFileName(att.Thumb.FileUrl);
                                            var thumbContent = new ByteArrayContent(thumbBytes);
                                            thumbContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(thumbName));
                                            form.Add(thumbContent, att.Thumb.TypeAttachment, thumbName);
                                        }
                                    }
                                    else if (att.FileUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                                    {
                                        form.Add(new StringContent(att.FileUrl), "postSticker");
                                    }
                                }
                            }

                            var response = await client.PostAsync(url, form);
                            var json = await response.Content.ReadAsStringAsync();

                            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                            {
                                if (!hasExplicitApiError)
                                    lastError = $"Unexpected server response (HTTP {(int)response.StatusCode})";
                                continue;
                            }

                            var token = JObject.Parse(json);
                            var apiStatus = token["api_status"]?.ToString();
                            if (string.Equals(apiStatus, "200", StringComparison.OrdinalIgnoreCase))
                            {
                                var result = token.ToObject<AddPostObject>();
                                if (result != null)
                                    return (200, result);

                                return (200, json);
                            }

                            var errorText = token["errors"]?["error_text"]?.ToString();
                            if (!string.IsNullOrWhiteSpace(errorText))
                            {
                                lastError = errorText;
                                hasExplicitApiError = true;
                            }
                            else
                                lastError = json;
                        }

                        return (400, lastError);
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                public static async Task<(int, dynamic)> PostActionFallbackAsync(string postId, string action, string reaction = "", string postText = "", string privacy = "")
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(postId))
                            return (400, "Missing session or post id");

                        if (string.Equals(action, "delete", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(action, "delete_post", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(action, "remove_post", StringComparison.OrdinalIgnoreCase))
                        {
                            var (quickDeleteStatus, quickDeleteRespond) = await ExecuteDeletePostFallbackQuickAsync(sessionId, postId);
                            return (quickDeleteStatus, quickDeleteRespond);
                        }

                        // Skip phone API (app_api.php) — it has no handler for post_actions/like/share
                        // and returns HTTP 200 with empty body, causing false success.
                        // Go directly to the web API (api-v2.php → post-actions.php).

                        var (webStatus, webRespond) = await ExecuteWebApiPostActionAsync(sessionId, postId, action, reaction);
                        if (webStatus == 200)
                            return (webStatus, webRespond);

                        return (400, "Post action fallback failed");
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                private static async Task<(int, dynamic)> ExecuteDeletePostFallbackQuickAsync(string sessionId, string postId)
                {
                    var requestCandidates = new[]
                    {
                        (Type: "delete_post", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete" }
                        }),
                        (Type: "delete_post", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete" },
                            { "do", "delete" }
                        }),
                        (Type: "delete_post", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete_post" },
                            { "type", "delete_post" }
                        }),
                        (Type: "delete_post", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "delete_post", "1" },
                            { "action", "delete_post" },
                            { "type", "delete_post" }
                        }),
                        (Type: "delete_post", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "do", "delete_post" },
                            { "action", "delete_post" }
                        }),
                        (Type: "post_actions", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete" },
                            { "type", "delete_post" }
                        }),
                        (Type: "post_actions", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete_post" },
                            { "type", "delete_post" }
                        }),
                        (Type: "posts_action", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete" },
                            { "type", "delete_post" }
                        }),
                        (Type: "post_action", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "action", "delete" },
                            { "type", "delete_post" }
                        }),
                        (Type: "delete", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "type", "delete" },
                            { "action", "delete" }
                        }),
                        (Type: "delete", Payload: new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "postId", postId },
                            { "id", postId },
                            { "type", "delete" },
                            { "do", "delete" }
                        })
                    };

                    string lastError = "Delete fallback failed";
                    foreach (var payload in requestCandidates.Select(r => r.Payload))
                    {
                        if (!payload.ContainsKey("access_token"))
                            payload["access_token"] = sessionId;
                        if (!payload.ContainsKey("accessToken"))
                            payload["accessToken"] = sessionId;
                    }

                    foreach (var request in requestCandidates)
                    {
                        Log.Info("WoFallback", $"Delete fallback candidate type={request.Type} postId={postId} keys={string.Join(",", request.Payload.Keys.OrderBy(k => k))}");
                        var (apiStatus, respond) = await ExecutePhoneApiFormAsync(request.Type, request.Payload, 8);
                        if (apiStatus == 200)
                            return (apiStatus, respond);

                        lastError = respond?.ToString() ?? lastError;
                    }

                    return (400, lastError);
                }

                private static async Task<(int, dynamic)> ExecutePhoneApiFormSingleEndpointAsync(string type, Dictionary<string, string> fields, int timeoutSeconds = 5)
                {
                    using var handler = new HttpClientHandler();
                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds < 4 ? 4 : timeoutSeconds) };
                    client.DefaultRequestHeaders.ConnectionClose = true;

                    fields.TryGetValue("s", out var sessionId);
                    sessionId ??= string.Empty;

                    var endpoint = $"{GetApiBase()}/app_api.php?application=phone&type={type}&s={Uri.EscapeDataString(sessionId)}";
                    Log.Info("WoFallback", $"HttpForm request type={type} endpoint={endpoint} timeout={timeoutSeconds} keys={string.Join(",", fields.Keys.OrderBy(k => k))}");

                    try
                    {
                        var postFields = new Dictionary<string, string>(fields);
                        if (postFields.ContainsKey("s") && !postFields.ContainsKey("session"))
                            postFields["session"] = postFields["s"];
                        postFields.Remove("s");
                        using var form = new FormUrlEncodedContent(postFields.Where(a => !string.IsNullOrEmpty(a.Key)));
                        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                        {
                            Content = form
                        };
                        request.Headers.ConnectionClose = true;

                        var response = await client.SendAsync(request);
                        var json = await response.Content.ReadAsStringAsync();
                        var normalizedJson = (json ?? string.Empty).TrimStart('\uFEFF', ' ', '\t', '\r', '\n');

                        if (string.IsNullOrWhiteSpace(normalizedJson) || !normalizedJson.StartsWith("{"))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK
                                && string.IsNullOrWhiteSpace(normalizedJson)
                                && type.IndexOf("delete", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                Log.Info("WoFallback", $"HttpForm success: delete type={type} endpoint={endpoint} empty-body-treated-as-success");
                                return (200, $"type={type}; endpoint={endpoint}; http=200; empty-body-treated-as-success");
                            }

                            var shortBody = normalizedJson.Replace("\r", " ").Replace("\n", " ");
                            if (shortBody.Length > 180)
                                shortBody = shortBody.Substring(0, 180);
                            Log.Warn("WoFallback", $"HttpForm non-JSON response: type={type} endpoint={endpoint} status={(int)response.StatusCode} body={shortBody}");
                            return (400, $"type={type}; endpoint={endpoint}; http={(int)response.StatusCode}; nonjson={shortBody}");
                        }

                        var token = JObject.Parse(normalizedJson);
                        var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                        if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                            return (200, token);

                        var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? token["message"]?.ToString() ?? token.ToString(Formatting.None);
                        if (!string.IsNullOrEmpty(errorText) && errorText.Length > 220)
                            errorText = errorText.Substring(0, 220);

                        return (400, $"type={type}; endpoint={endpoint}; http={(int)response.StatusCode}; api={apiStatus}; error={errorText}");
                    }
                    catch (Exception ex)
                    {
                        var err = ex.Message ?? string.Empty;
                        if (err.Length > 220)
                            err = err.Substring(0, 220);
                        return (400, $"type={type}; endpoint={endpoint}; ex={err}");
                    }
                }

                public static async Task<(int, dynamic)> CreateCommentFastPhoneApiAsync(string postOrCommentId, string text, string imagePath = "", string voicePath = "", string imageUrl = "", string mode = "")
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(postOrCommentId))
                            return (400, "Missing session or target id");

                        var isReply = string.Equals(mode, "create_reply", StringComparison.OrdinalIgnoreCase);
                        var typeCandidates = isReply
                            ? new[] { "create_post_comment", "create_reply", "reply_comment" }
                            : new[] { "create_post_comment", "create_comment", "post_comment" };

                        var payloads = new[]
                        {
                            new Dictionary<string, string>
                            {
                                { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                                { "user_id", UserDetails.UserId ?? string.Empty },
                                { "s", sessionId },
                                { "text", text ?? string.Empty },
                                { "post_id", postOrCommentId },
                                { "id", postOrCommentId }
                            },
                            new Dictionary<string, string>
                            {
                                { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                                { "s", sessionId },
                                { "comment_text", text ?? string.Empty },
                                { "post_id", postOrCommentId },
                                { "type", "create_comment" }
                            }
                        };

                        if (isReply)
                        {
                            foreach (var p in payloads)
                            {
                                p.Remove("post_id");
                                p.Remove("id");
                                p["comment_id"] = postOrCommentId;
                                p["mode"] = "create_reply";
                            }
                        }

                        foreach (var payload in payloads)
                        {
                            foreach (var type in typeCandidates)
                            {
                                var (status, resp) = await ExecutePhoneApiFormAsync(type, payload, 6);
                                if (status == 200)
                                {
                                    Log.Info("WoFallback", $"CreateCommentFastPhoneApi succeeded type={type}");
                                    if (resp is JObject token)
                                    {
                                        // Wrap under "data" key to match SDK format expected by call sites
                                        var wrapped = new JObject
                                        {
                                            ["api_status"] = "200",
                                            ["data"] = token
                                        };
                                        return (200, wrapped);
                                    }
                                    return (200, resp);
                                }
                            }
                        }
                        return (400, "Fast phone API comment creation failed");
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                public static async Task<(int, dynamic)> CreateCommentFallbackAsync(string postOrCommentId, string text, string imagePath = "", string voicePath = "", string imageUrl = "", string mode = "")
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(postOrCommentId))
                            return (400, "Missing session or target id");

                        var basePayload = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "text", text ?? string.Empty },
                            { "comment_text", text ?? string.Empty },
                            { "comment", text ?? string.Empty },
                            { "message", text ?? string.Empty },
                            { "c_file", imagePath ?? string.Empty },
                            { "record", voicePath ?? string.Empty },
                            { "image", imageUrl ?? string.Empty }
                        };

                        var isReply = string.Equals(mode, "create_reply", StringComparison.OrdinalIgnoreCase);
                        var payloadCandidates = new List<Dictionary<string, string>>();

                        // Minimal payload variants first (most compatible with legacy app_api routes).
                        var minimalPayload1 = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "text", text ?? string.Empty }
                        };

                        var minimalPayload2 = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "s", sessionId },
                            { "comment_text", text ?? string.Empty }
                        };

                        var payload1 = new Dictionary<string, string>(basePayload);
                        var payload2 = new Dictionary<string, string>(basePayload);
                        var payload3 = new Dictionary<string, string>(basePayload);

                        if (string.Equals(mode, "create_reply", StringComparison.OrdinalIgnoreCase))
                        {
                            minimalPayload1["comment_id"] = postOrCommentId;
                            minimalPayload1["mode"] = "create_reply";
                            minimalPayload2["comment_id"] = postOrCommentId;
                            minimalPayload2["type"] = "create_reply";

                            payload1["mode"] = "create_reply";
                            payload1["type"] = "create_reply";
                            payload1["comment_id"] = postOrCommentId;
                            payload1["id"] = postOrCommentId;

                            payload2["type"] = "reply_comment";
                            payload2["commentId"] = postOrCommentId;
                            payload2["parent_comment_id"] = postOrCommentId;
                            payload2["id"] = postOrCommentId;

                            payload3["do"] = "create_reply";
                            payload3["comment_id"] = postOrCommentId;
                            payload3["commentId"] = postOrCommentId;
                        }
                        else
                        {
                            minimalPayload1["post_id"] = postOrCommentId;
                            minimalPayload2["post_id"] = postOrCommentId;
                            minimalPayload2["postId"] = postOrCommentId;

                            payload1["post_id"] = postOrCommentId;
                            payload1["id"] = postOrCommentId;

                            payload2["postId"] = postOrCommentId;
                            payload2["post_id"] = postOrCommentId;
                            payload2["action"] = "create_comment";

                            payload3["do"] = "comment";
                            payload3["post_id"] = postOrCommentId;
                        }

                        payloadCandidates.Add(minimalPayload1);
                        payloadCandidates.Add(payload1);
                        payloadCandidates.Add(payload2);

                        var typeCandidates = isReply
                            ? new[] { "create_post_comment", "create_reply", "reply_comment" }
                            : new[] { "create_post_comment", "create_comment", "post_comment" };

                        string lastError = "Create comment fallback failed";
                        Log.Info("WoFallback", $"CreateCommentFallbackAsync start targetId={postOrCommentId} isReply={isReply} textLength={text?.Length ?? 0} image={!string.IsNullOrEmpty(imagePath)} voice={!string.IsNullOrEmpty(voicePath)} imageUrl={!string.IsNullOrEmpty(imageUrl)}");

                        foreach (var payload in payloadCandidates)
                        {
                            foreach (var type in typeCandidates)
                            {
                                Log.Info("WoFallback", $"Create comment fallback candidate type={type} keys={string.Join(",", payload.Keys.OrderBy(k => k))}");
                                var (apiStatus, respond) = await ExecutePhoneApiFormAsync(type, payload, 8);
                                if (apiStatus == 200)
                                {
                                    return (200, respond);
                                }

                                lastError = respond?.ToString() ?? lastError;
                            }
                        }
                        Log.Warn("WoFallback", $"CreateCommentFallbackAsync final fail targetId={postOrCommentId} error={lastError}");
                        return (400, lastError);
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                public static async Task<(int, dynamic)> SharePostFallbackAsync(string postId, string targetId, string shareMode, string text)
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(postId))
                            return (400, "Missing session or post id");

                        // Use phone API's new_post endpoint with parent_id to create a share post.
                        // The web API has no share-post/share endpoint; post-actions doesn't handle 'share'.
                        // The phone API's new_post DOES handle parent_id for sharing.
                        // Note: Wo_RegisterPost rejects posts with empty postText and no media,
                        // so ensure postText has at least some content.
                        var shareText = text ?? string.Empty;
                        if (string.IsNullOrWhiteSpace(shareText))
                            shareText = "shared";

                        Log.Info("FON_SHARE", $"SharePostFallback postId={postId} targetId={targetId} mode={shareMode} textLen={text?.Length}");

                        var payload = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "parent_id", postId },
                            { "postText", shareText },
                        };

                        if (!string.IsNullOrWhiteSpace(targetId))
                        {
                            if (string.Equals(shareMode, "share_post_on_group", StringComparison.OrdinalIgnoreCase))
                            {
                                payload["user_id"] = targetId;
                                payload["group_id"] = targetId;
                            }
                            else if (string.Equals(shareMode, "share_post_on_page", StringComparison.OrdinalIgnoreCase))
                            {
                                payload["user_id"] = "0";
                                payload["page_id"] = targetId;
                            }
                            else
                            {
                                var currentUserId = UserDetails.UserId ?? string.Empty;
                                if (!string.Equals(targetId, currentUserId, StringComparison.OrdinalIgnoreCase))
                                {
                                    payload["recipient_id"] = targetId;
                                }
                            }
                        }

                        Log.Info("FON_SHARE", $"Payload keys={string.Join(",", payload.Keys)}");
                        var result = await ExecutePhoneApiFormAsync("new_post", payload, 20);
                        Log.Info("FON_SHARE", $"Result status={result.Item1} response={result.Item2?.ToString()?.Substring(0, Math.Min(200, result.Item2?.ToString()?.Length ?? 0))}");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                private static async Task<(int, dynamic)> ExecutePhoneApiFormAsync(string type, Dictionary<string, string> fields, int timeoutSeconds = 45)
                {
                    using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(timeoutSeconds < 4 ? 4 : timeoutSeconds) };
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    fields.TryGetValue("s", out var sessionId);
                    sessionId ??= string.Empty;
                    // rename s to session in POST body to work around server-side filter
                    var postFields = new Dictionary<string, string>(fields);
                    if (postFields.ContainsKey("s") && !postFields.ContainsKey("session"))
                        postFields["session"] = postFields["s"];
                    postFields.Remove("s");
                    var endpoints = new[]
                    {
                        $"{GetApiBase()}/app_api.php?application=phone&type={type}&s={Uri.EscapeDataString(sessionId)}",
                        $"{GetApiBase()}/app_api.php?type={type}&s={Uri.EscapeDataString(sessionId)}",
                        $"{GetApiBase()}/app_api.php?application=phone&type={type}",
                        $"{GetApiBase()}/app_api.php?type={type}"
                    };

                    string lastError = "Bad request";
                    var maxAttempts = timeoutSeconds <= 8 ? 1 : 2;
                    foreach (var endpoint in endpoints)
                    {
                        for (var attempt = 1; attempt <= maxAttempts; attempt++)
                        {
                            Log.Info("WoFallback", $"ExecutePhoneApiFormAsync start type={type} endpoint={endpoint} attempt={attempt} timeout={timeoutSeconds}");
                            try
                            {
                                using var form = new FormUrlEncodedContent(postFields.Where(a => !string.IsNullOrEmpty(a.Key)));
                                using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                                {
                                    Content = form
                                };
                                request.Headers.ConnectionClose = true;

                                var response = await client.SendAsync(request);
                                var json = await response.Content.ReadAsStringAsync();
                                var trimmed = json?.Trim() ?? string.Empty;

                                if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                                {
                                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                                    {
                                        if (string.IsNullOrWhiteSpace(trimmed)
                                            || string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase)
                                            || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
                                        {
                                            var typeKey = type ?? string.Empty;
                                            if (typeKey.IndexOf("comment", StringComparison.OrdinalIgnoreCase) >= 0
                                                || typeKey.IndexOf("share", StringComparison.OrdinalIgnoreCase) >= 0
                                                || typeKey.IndexOf("post", StringComparison.OrdinalIgnoreCase) >= 0)
                                            {
                                                Log.Info("WoFallback", $"ExecutePhoneApiFormAsync empty-or-simple-success treated as success type={type} endpoint={endpoint} body={trimmed}");
                                                return (200, $"type={type}; endpoint={endpoint}; http=200; simple-success-response={trimmed}");
                                            }
                                        }
                                    }

                                    var shortBody = trimmed.Replace("\r", " ").Replace("\n", " ");
                                    if (shortBody.Length > 180)
                                        shortBody = shortBody.Substring(0, 180);
                                    lastError = $"type={type}; endpoint={endpoint}; http={(int)response.StatusCode}; nonjson={shortBody}";
                                    continue;
                                }

                                var token = JObject.Parse(json);
                                var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                                if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                                {
                                    Log.Info("WoFallback", $"ExecutePhoneApiFormAsync success type={type} endpoint={endpoint} apiStatus={apiStatus}");
                                    return (200, token);
                                }

                                var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? token["message"]?.ToString() ?? token.ToString(Formatting.None);
                                if (!string.IsNullOrEmpty(errorText) && errorText.Length > 220)
                                    errorText = errorText.Substring(0, 220);
                                lastError = $"type={type}; endpoint={endpoint}; http={(int)response.StatusCode}; api={apiStatus}; error={errorText}";
                                Log.Warn("WoFallback", lastError);
                            }
                            catch (Exception ex)
                            {
                                var err = ex.Message ?? string.Empty;
                                if (err.Length > 220)
                                    err = err.Substring(0, 220);
                                lastError = $"type={type}; endpoint={endpoint}; attempt={attempt}; ex={err}";
                                Log.Warn("WoFallback", lastError);

                                // Retry once on transient transport errors that commonly appear on Android/OkHttp.
                                if (attempt >= 2 || err.IndexOf("unexpected end of stream", StringComparison.OrdinalIgnoreCase) < 0)
                                    break;
                            }
                        }
                    }

                    return (400, lastError);
                }

                private static async Task<(int, dynamic)> ExecuteWebApiPostActionAsync(string sessionId, string postId, string action, string reaction, string shareType = null)
                {
                    using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                    using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(45) };
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
                    var endpointCandidates = new[]
                    {
                        $"{GetApiBase()}/api/post-actions?access_token={sessionId}",
                        $"{GetApiBase()}/api/post-actions"
                    };

                    Log.Info("WoFallback", $"WebApiPostAction: action={action} postId={postId} shareType={shareType} sessionId={(string.IsNullOrWhiteSpace(sessionId) ? "<none>" : "<present>")}");
                    foreach (var endpoint in endpointCandidates)
                    {
                        Log.Info("WoFallback", $"WebApiPostAction trying endpoint={endpoint}");
                        var payload = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "post_id", postId },
                            { "shared_post_id", postId },
                            { "postId", postId },
                            { "action", action ?? string.Empty },
                            { "reaction", reaction ?? string.Empty },
                            { "reaction_type", reaction ?? string.Empty },
                            { "share_type", shareType ?? string.Empty },
                            { "s", sessionId }
                        };

                        if (!string.IsNullOrWhiteSpace(shareType))
                            payload["type"] = shareType;

                        using var form = new FormUrlEncodedContent(payload.Where(a => !string.IsNullOrWhiteSpace(a.Key)));
                        var response = await client.PostAsync(endpoint, form);
                        var json = await response.Content.ReadAsStringAsync();
                        var trimmed = json?.Trim() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK
                                && (string.IsNullOrWhiteSpace(trimmed)
                                    || string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase)))
                            {
                                Log.Info("WoFallback", $"WebApiPostAction empty-or-simple-success treated as success endpoint={endpoint} body={trimmed}");
                                return (200, $"endpoint={endpoint}; http=200; simple-success-response={trimmed}");
                            }

                            Log.Warn("WoFallback", $"WebApiPostAction skipped non-JSON or empty response from endpoint={endpoint}");
                            continue;
                        }

                        var token = JObject.Parse(json);
                        var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                        if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                            return (200, token);
                    }

                    return (400, "Web post action fallback failed");
                }

                private static async Task<(int, dynamic)> ExecuteSdkStyleCreateCommentAsync(string sessionId, string targetId, string text, string imageUrl, bool isReply, string imagePath, string voicePath)
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                    var endpoint = $"{GetApiBase()}/api/comment?access_token={sessionId}";
                    try
                    {
                        var payload = new Dictionary<string, string>
                        {
                            { "access_token", sessionId },
                            { "type", isReply ? "create_reply" : "create" },
                            { "text", text ?? string.Empty },
                            { "image_url", imageUrl ?? string.Empty }
                        };

                        if (isReply)
                            payload["comment_id"] = targetId;
                        else
                            payload["post_id"] = targetId;

                        var hasImageFile = !string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath);
                        var hasVoiceFile = !string.IsNullOrEmpty(voicePath) && System.IO.File.Exists(voicePath);

                        if (hasImageFile || hasVoiceFile)
                        {
                            using var form = new MultipartFormDataContent();
                            foreach (var kv in payload.Where(a => !string.IsNullOrWhiteSpace(a.Value)))
                                form.Add(new StringContent(kv.Value), kv.Key);

                            if (hasImageFile)
                            {
                                var imageContent = new StreamContent(System.IO.File.OpenRead(imagePath));
                                imageContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(imagePath));
                                form.Add(imageContent, "image", System.IO.Path.GetFileName(imagePath));
                            }

                            if (hasVoiceFile)
                            {
                                var voiceContent = new StreamContent(System.IO.File.OpenRead(voicePath));
                                voiceContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(voicePath));
                                form.Add(voiceContent, "audio", System.IO.Path.GetFileName(voicePath));
                            }

                            var response = await client.PostAsync(endpoint, form);
                            var json = await response.Content.ReadAsStringAsync();
                            Log.Info("WoFallback", $"SdkStyleComment multipart endpoint={endpoint} http={(int)response.StatusCode} bodyLen={json?.Length ?? 0}");
                            return await ProcessCommentWebResponseAsync(json, endpoint);
                        }
                        else
                        {
                            using var form = new FormUrlEncodedContent(payload.Where(a => !string.IsNullOrWhiteSpace(a.Value)));
                            var response = await client.PostAsync(endpoint, form);
                            var json = await response.Content.ReadAsStringAsync();
                            Log.Info("WoFallback", $"SdkStyleComment form endpoint={endpoint} http={(int)response.StatusCode} bodyLen={json?.Length ?? 0}");
                            return await ProcessCommentWebResponseAsync(json, endpoint);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("WoFallback", $"SdkStyleComment exception endpoint={endpoint} ex={ex.Message}");
                        return (400, $"SdkStyleComment exception: {ex.Message}");
                    }
                }

                private static async Task<(int, dynamic)> ProcessCommentWebResponseAsync(string json, string endpoint)
                {
                    var trimmed = json?.Trim() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                    {
                        if (string.IsNullOrWhiteSpace(trimmed)
                            || string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase)
                            || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Info("WoFallback", $"ProcessCommentWebResponse simple-success endpoint={endpoint}");
                            return (200, trimmed);
                        }

                        var shortBody = trimmed.Replace("\r", " ").Replace("\n", " ");
                        if (shortBody.Length > 180) shortBody = shortBody.Substring(0, 180);
                        return (400, $"http=200; nonjson={shortBody}");
                    }

                    var token = JObject.Parse(json);
                    var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                    if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                    {
                        Log.Info("WoFallback", $"ProcessCommentWebResponse success endpoint={endpoint} apiStatus={apiStatus}");
                        return (200, token);
                    }

                    var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? token["message"]?.ToString() ?? token.ToString(Formatting.None);
                    if (!string.IsNullOrEmpty(errorText) && errorText.Length > 220)
                        errorText = errorText.Substring(0, 220);
                    return (400, $"api={apiStatus}; error={errorText}");
                }

                private static async Task<(int, dynamic)> ExecuteWebApiCreateCommentAsync(string sessionId, string targetId, string text, bool isReply)
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
                    var endpointCandidates = new[]
                    {
                        $"{GetApiBase()}/api/create-comment?access_token={sessionId}",
                        $"{GetApiBase()}/api/create-comment",
                        $"{GetApiBase()}/api/comment?access_token={sessionId}",
                        $"{GetApiBase()}/api/comment",
                        $"{GetApiBase()}/api/comments/create?access_token={sessionId}",
                        $"{GetApiBase()}/api/comments/create"
                    };

                    string lastError = "Web comment fallback failed";
                    foreach (var endpoint in endpointCandidates)
                    {
                        var payload = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "access_token", sessionId },
                            { "type", isReply ? "create_reply" : "create" },
                            { "text", text ?? string.Empty },
                            { "comment_text", text ?? string.Empty },
                            { "comment", text ?? string.Empty }
                        };

                        if (isReply)
                            payload["comment_id"] = targetId;
                        else
                            payload["post_id"] = targetId;

                        using var form = new FormUrlEncodedContent(payload.Where(a => !string.IsNullOrWhiteSpace(a.Key)));
                        var response = await client.PostAsync(endpoint, form);
                        var json = await response.Content.ReadAsStringAsync();
                        var trimmed = json?.Trim() ?? string.Empty;

                        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                        {
                            if (response.StatusCode == System.Net.HttpStatusCode.OK
                                && (string.IsNullOrWhiteSpace(trimmed)
                                    || string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(trimmed, "true", StringComparison.OrdinalIgnoreCase)))
                            {
                                Log.Info("WoFallback", $"WebApiCreateComment empty-or-simple-success treated as success endpoint={endpoint} body={trimmed}");
                                return (200, $"endpoint={endpoint}; http=200; simple-success-response={trimmed}");
                            }

                            var shortBody = trimmed.Replace("\r", " ").Replace("\n", " ");
                            if (shortBody.Length > 180)
                                shortBody = shortBody.Substring(0, 180);
                            lastError = $"endpoint={endpoint}; http={(int)response.StatusCode}; nonjson={shortBody}";
                            continue;
                        }

                        var token = JObject.Parse(json);
                        var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                        if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                            return (200, token);

                        var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? token["message"]?.ToString() ?? token.ToString(Formatting.None);
                        if (!string.IsNullOrEmpty(errorText) && errorText.Length > 220)
                            errorText = errorText.Substring(0, 220);
                        lastError = $"endpoint={endpoint}; http={(int)response.StatusCode}; api={apiStatus}; error={errorText}";
                    }

                    return (400, lastError);
                }

                private static string GetMimeType(string fileName)
                {
                    var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                    return ext switch
                    {
                        ".jpg" => "image/jpeg",
                        ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        ".webp" => "image/webp",
                        ".mp4" => "video/mp4",
                        ".mov" => "video/quicktime",
                        ".m4v" => "video/x-m4v",
                        ".webm" => "video/webm",
                        ".flv" => "video/x-flv",
                        ".mpeg" => "video/mpeg",
                        ".mpg" => "video/mpeg",
                        ".mkv" => "video/x-matroska",
                        ".avi" => "video/x-msvideo",
                        ".3gp" => "video/3gpp",
                        ".mp3" => "audio/mpeg",
                        ".wav" => "audio/wav",
                        _ => "application/octet-stream"
                    };
                }

                private static bool IsVideoExtension(string fileName)
                {
                    var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
                    return ext is ".mp4" or ".mov" or ".m4v" or ".webm" or ".flv" or ".mpeg" or ".mpg" or ".mkv" or ".avi" or ".3gp";
                }

                public static async Task<(int, dynamic)> FetchCommentsWebApiAsync(string sessionId, string targetId, string limit = "10", string offset = "0", string type = "fetch_comments")
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                    var isReply = type == "fetch_comments_reply";
                    var idKey = isReply ? "comment_id" : "post_id";
                    var endpoints = new[]
                    {
                        $"{GetApiBase()}/api/comments?access_token={sessionId}&{idKey}={targetId}&limit={limit}&offset={offset}",
                        $"{GetApiBase()}/api/comment?access_token={sessionId}&{idKey}={targetId}&limit={limit}&offset={offset}&type={type}",
                    };

                    foreach (var endpoint in endpoints)
                    {
                        try
                        {
                            Log.Info("WoFallback", $"FetchCommentsWebApi trying endpoint={endpoint}");
                            var response = await client.GetAsync(endpoint);
                            var json = await response.Content.ReadAsStringAsync();

                            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                                continue;

                            var token = JObject.Parse(json);
                            var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                            if (apiStatus == "200" || apiStatus == "201")
                            {
                                var data = token["data"] as JArray ?? token["comment_list"] as JArray ?? token["comments"] as JArray;
                                if (data != null)
                                {
                                    var comments = data.ToObject<List<GetCommentObject>>();
                                    if (comments != null)
                                        return (200, new CommentObject { Status = 200, CommentList = comments });
                                }
                                return (200, token);
                            }

                            var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? json;
                            Log.Warn("WoFallback", $"FetchCommentsWebApi failed endpoint={endpoint} apiStatus={apiStatus} error={errorText}");
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("WoFallback", $"FetchCommentsWebApi exception endpoint={endpoint} ex={ex.Message}");
                        }
                    }

                    return (400, "Comment loading web API fallback failed");
                }

                public static async Task<(int, dynamic)> SharePostWebApiAsync(string sessionId, string postId, string targetId, string shareMode, string text)
                {
                    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
                    var endpoints = new[]
                    {
                        $"{GetApiBase()}/api/share-post?access_token={sessionId}",
                        $"{GetApiBase()}/api/share-post",
                        $"{GetApiBase()}/api/share?access_token={sessionId}",
                        $"{GetApiBase()}/api/share",
                    };

                    foreach (var endpoint in endpoints)
                    {
                        try
                        {
                            var payload = new Dictionary<string, string>
                            {
                                { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                                { "user_id", UserDetails.UserId ?? string.Empty },
                                { "s", sessionId },
                                { "post_id", postId },
                                { "id", postId },
                                { "text", text ?? string.Empty },
                                { "type", shareMode ?? string.Empty },
                                { "share_type", shareMode ?? string.Empty },
                            };
                            if (!string.IsNullOrWhiteSpace(targetId))
                            {
                                payload["recipient_id"] = targetId;
                                payload["target_id"] = targetId;
                                if (shareMode?.IndexOf("group", StringComparison.OrdinalIgnoreCase) >= 0)
                                    payload["group_id"] = targetId;
                                else if (shareMode?.IndexOf("page", StringComparison.OrdinalIgnoreCase) >= 0)
                                    payload["page_id"] = targetId;
                            }

                            using var form = new FormUrlEncodedContent(payload.Where(a => !string.IsNullOrWhiteSpace(a.Key)));
                            var response = await client.PostAsync(endpoint, form);
                            var json = await response.Content.ReadAsStringAsync();
                            var trimmed = json?.Trim() ?? string.Empty;

                            Log.Info("WoFallback", $"SharePostWebApi endpoint={endpoint} status={response.StatusCode}");

                            if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                            {
                                if (response.StatusCode == System.Net.HttpStatusCode.OK
                                    && (string.IsNullOrWhiteSpace(trimmed)
                                        || string.Equals(trimmed, "ok", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(trimmed, "success", StringComparison.OrdinalIgnoreCase)
                                        || string.Equals(trimmed, "1", StringComparison.OrdinalIgnoreCase)))
                                {
                                    Log.Info("WoFallback", $"SharePostWebApi simple-success endpoint={endpoint}");
                                    return (200, trimmed);
                                }
                                continue;
                            }

                            var token = JObject.Parse(json);
                            var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                            if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                                return (200, token);

                            var errorText = token["errors"]?["error_text"]?.ToString() ?? token["error"]?.ToString() ?? json;
                            Log.Warn("WoFallback", $"SharePostWebApi failed endpoint={endpoint} apiStatus={apiStatus} error={errorText}");
                        }
                        catch (Exception ex)
                        {
                            Log.Warn("WoFallback", $"SharePostWebApi exception endpoint={endpoint} ex={ex.Message}");
                        }
                    }

                    return (400, "Share post web API fallback failed");
                }

                public static async Task<(int, dynamic)> GetPostCommentsFallbackAsync(string postId, string limit, string offset)
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId))
                            return (400, "Session missing");

                        var payload = new Dictionary<string, string>
                        {
                            { "server_key", InitializeWoWonder.ServerKey ?? string.Empty },
                            { "user_id", UserDetails.UserId ?? string.Empty },
                            { "s", sessionId },
                            { "post_id", postId },
                            { "id", postId },
                            { "postId", postId },
                            { "limit", limit ?? "10" },
                            { "offset", offset ?? "0" }
                        };

                        return await ExecutePhoneApiFormAsync("fetch_comments", payload);
                    }
                    catch (Exception ex)
                    {
                        return (404, ex.Message);
                    }
                }

                public static async Task<(int, dynamic)> CommentReactionFallbackAsync(string targetId, string reaction, string reactionType)
                {
                    try
                    {
                        var sessionId = string.IsNullOrWhiteSpace(UserDetails.AccessToken) ? Current.AccessToken : UserDetails.AccessToken;
                        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(targetId))
                            return (400, "Missing session or target id");

                        Log.Info("FON_CMT_REACT", $"targetId={targetId} reaction={reaction} type={reactionType}");

                        using var handler = new Xamarin.Android.Net.AndroidMessageHandler();
                        using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(15) };
                        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json, text/plain, */*");
                        client.DefaultRequestHeaders.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                        var endpoint = $"{GetApiBase()}/api/comments?access_token={sessionId}";
                        var payload = new Dictionary<string, string>
                        {
                            { "type", reactionType },
                            { "reaction", reaction },
                        };

                        if (string.Equals(reactionType, "reaction_reply", StringComparison.OrdinalIgnoreCase))
                            payload["reply_id"] = targetId;
                        else
                            payload["comment_id"] = targetId;

                        Log.Info("FON_CMT_REACT", $"POST {endpoint} keys={string.Join(",", payload.Keys)}");
                        using var form = new FormUrlEncodedContent(payload.Where(a => !string.IsNullOrWhiteSpace(a.Key)));
                        var response = await client.PostAsync(endpoint, form);
                        var json = await response.Content.ReadAsStringAsync();
                        Log.Info("FON_CMT_REACT", $"HTTP {response.StatusCode} body={json?.Substring(0, Math.Min(300, json?.Length ?? 0))}");

                        if (string.IsNullOrWhiteSpace(json) || !json.TrimStart().StartsWith("{"))
                            return (400, "Non-JSON response");

                        var token = JObject.Parse(json);
                        var apiStatus = token["api_status"]?.ToString() ?? token["status"]?.ToString();
                        if (apiStatus == "200" || apiStatus == "201" || apiStatus == "1")
                            return (200, token);

                        return (400, token["errors"]?["error_text"]?.ToString() ?? "Comment reaction failed");
                    }
                    catch (Exception ex)
                    {
                        Log.Warn("FON_CMT_REACT", $"Exception: {ex.Message}");
                        return (404, ex.Message);
                    }
                }
            }
        }
    }
