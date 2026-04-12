using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class GetCommunityDataObject
    {
        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }
        [JsonProperty("community_data", NullValueHandling = NullValueHandling.Ignore)]
        public CommunityDataObject CommunityData { get; set; }
    }
}