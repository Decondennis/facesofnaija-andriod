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
using Facesofnaija.CustomApi.Classes.Search;

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
    }
}