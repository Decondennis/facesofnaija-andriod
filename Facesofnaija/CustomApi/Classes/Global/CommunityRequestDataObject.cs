using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CommunityRequestDataObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("community_id")]
        public string CommunityId { get; set; }

        [JsonProperty("community_name")]
        public string CommunityName { get; set; }

        [JsonProperty("community_title")]
        public string CommunityTitle { get; set; }

        [JsonProperty("about")]
        public string About { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("privacy")]
        public string Privacy { get; set; }

        [JsonProperty("request_status")]
        public string RequestStatus { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }

        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty("members_count")]
        public string MembersCount { get; set; }
    }
}
