using System.Threading.Tasks;
using System.Net.Http;
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
using System.Collections.ObjectModel;
using System.IO;
using Facesofnaija.CustomApi.Classes.Search;
using System.Net.Http.Headers;

namespace Facesofnaija.CustomApi.Requests
{
    public class CustomRequests
    {
        public static class Community
        {
            public static string WebsiteUrl { get; set; }
            public static string UserId { get; set; }
            public static string AccessToken { get; set; }

            public static async Task<(long?, dynamic)> GetJoinedCommunitiesAsync()//string userId, string offset, string limit
            {
                Console.WriteLine("This is GetJoinedCommunitiesAsync");
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
                Console.WriteLine("This is JoinCommunityAsync");
                try
                {
                    var client = new HttpClient();
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("server_key", InitializeWoWonder.ServerKey),
                        new KeyValuePair<string, string>("community_id", communityId)
                    });
                    var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/join-community?access_token=" + UserDetails.AccessToken, content);
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    JoinCommunityObject list = JsonConvert.DeserializeObject<JoinCommunityObject>(json);
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
            dictionary.Add("server_key", InitializeWoWonder.ServerKey);
            dictionary.Add("user_id", UserDetails.UserId);
            dictionary.Add("type", "request-community");
            
            try
            {
                var client = new HttpClient();
                var content = new FormUrlEncodedContent(dictionary);
                var response = await client.PostAsync(InitializeWoWonder.WebsiteUrl + "/api/communities-custom?access_token=" + UserDetails.AccessToken + "&type=request-community", content);
                Console.WriteLine(InitializeWoWonder.WebsiteUrl + "/api/request-community?access_token=" + UserDetails.AccessToken+ "&type=request-community");
                
                string json = await response.Content.ReadAsStringAsync();

                CommunityRequst comRquest = JsonConvert.DeserializeObject<CommunityRequst>(json);
                if (comRquest != null)
                {
                    return (comRquest.Status, comRquest);
                }

                var error = JsonConvert.DeserializeObject<Helpers.Model.Classes.ExErrorObject>(json);
                return (400, error);
            }
            catch (Exception e)
            {
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
        // Custom Posts — bypasses the SDK's internal HttpClient which points to the
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
                            Android.Util.Log.Warn("FON_POST_CUSTOM", "submit aborted: session id is empty");
                            return (400, "Session is missing. Please sign in again.");
                        }

                        var endpoints = new[]
                        {
                            $"{GetApiBase()}/app_api.php?application=phone&type=new_post",
                            $"{GetApiBase()}/app_api.php?type=new_post"
                        };

                        Android.Util.Log.Info("FON_POST_CUSTOM", $"submit pagePost={pagePost} userId={userId} postId={postId} privacy={privacy ?? "NULL"} textLen={(textContent ?? string.Empty).Length} attachments={postAttachments?.Count ?? 0} serverKeyLen={(InitializeWoWonder.ServerKey ?? string.Empty).Length}");

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
                                    if (att.FileStream != null)
                                    {
                                        att.FileStream.Position = 0;
                                        using var ms = new MemoryStream();
                                        att.FileStream.CopyTo(ms);
                                        var bytes = ms.ToArray();
                                        var fileName = att.FileName ?? "file";
                                        var fileContent = new ByteArrayContent(bytes);
                                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                                        form.Add(fileContent, att.TypeAttachment, fileName);

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
                                        var fileName = att.FileName ?? Path.GetFileName(att.FileUrl);
                                        var fileContent = new ByteArrayContent(bytes);
                                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(GetMimeType(fileName));
                                        form.Add(fileContent, att.TypeAttachment, fileName);

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

                            Android.Util.Log.Info("FON_POST_CUSTOM", $"POST → {url}");
                            var response = await client.PostAsync(url, form);
                            var json = await response.Content.ReadAsStringAsync();
                            Android.Util.Log.Info("FON_POST_CUSTOM", $"Response {(int)response.StatusCode}: {json?.Substring(0, Math.Min(400, json?.Length ?? 0))}");

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
                        Android.Util.Log.Error("FON_POST_CUSTOM", $"Exception: {ex.Message}");
                        return (404, ex.Message);
                    }
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
                        ".3gp" => "video/3gpp",
                        ".mp3" => "audio/mpeg",
                        ".wav" => "audio/wav",
                        _ => "application/octet-stream"
                    };
                }
            }
        }
    }