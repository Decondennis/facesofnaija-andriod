using Newtonsoft.Json;
using WoWonderClient.Classes.Global;
namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CommunityDataObject
    {
        public CommunityDataObject()
        {

        }
        [JsonProperty("is_community_joined", NullValueHandling = NullValueHandling.Ignore)]
        public long? IsCommunityJoined { get; set; }
        [JsonProperty("members", NullValueHandling = NullValueHandling.Ignore)]
        public long Members { get; set; }
        [JsonProperty("community_sub_category", NullValueHandling = NullValueHandling.Ignore)]
        public string GroupSubCategory { get; set; }
        [JsonProperty("username", NullValueHandling = NullValueHandling.Ignore)]
        public string Username { get; set; }
        [JsonProperty("category_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CategoryId { get; set; }
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty("is_joined", NullValueHandling = NullValueHandling.Ignore)]
        public IsJoined IsJoined { get; set; }
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string Url { get; set; }
        //[JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        //public FieldsUnion Fields { get; set; }
        [JsonProperty("members_count", NullValueHandling = NullValueHandling.Ignore)]
        public long? MembersCount { get; set; }
        [JsonProperty("community_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityId { get; set; }
        [JsonProperty("active", NullValueHandling = NullValueHandling.Ignore)]
        public string Active { get; set; }
        [JsonProperty("join_privacy", NullValueHandling = NullValueHandling.Ignore)]
        public string JoinPrivacy { get; set; }
        [JsonProperty("privacy", NullValueHandling = NullValueHandling.Ignore)]
        public string Privacy { get; set; }
        [JsonProperty("sub_category", NullValueHandling = NullValueHandling.Ignore)]
        public string SubCategory { get; set; }
        [JsonProperty("category", NullValueHandling = NullValueHandling.Ignore)]
        public string Category { get; set; }
        [JsonProperty("about", NullValueHandling = NullValueHandling.Ignore)]
        public string About { get; set; }
        [JsonProperty("cover", NullValueHandling = NullValueHandling.Ignore)]
        public string Cover { get; set; }
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public string Avatar { get; set; }
        [JsonProperty("community_title", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityTitle { get; set; }
        [JsonProperty("communit_name", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityName { get; set; }
        [JsonProperty("user_id", NullValueHandling = NullValueHandling.Ignore)]
        public string UserId { get; set; }
        [JsonProperty("registered", NullValueHandling = NullValueHandling.Ignore)]
        public string Registered { get; set; }
        [JsonProperty("is_reported", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsReported { get; set; }
    }
}