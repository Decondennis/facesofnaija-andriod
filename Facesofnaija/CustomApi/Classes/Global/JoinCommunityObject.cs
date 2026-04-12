using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class JoinCommunityObject
    {
        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public int Status { get; set; }
        [JsonProperty("join_status", NullValueHandling = NullValueHandling.Ignore)]
        public string JoinStatus { get; set; }
    }
}