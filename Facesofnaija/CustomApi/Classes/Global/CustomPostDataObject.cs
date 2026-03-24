using WoWonderClient.Classes.Posts;
using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CustomPostDataObject : PostDataObject
    {
        [JsonProperty("community_recipient", NullValueHandling = NullValueHandling.Ignore)]
        public CommunityDataObject CommunityRecipient { get; set; }
        [JsonProperty("community_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityId { get; set; }
    }
}