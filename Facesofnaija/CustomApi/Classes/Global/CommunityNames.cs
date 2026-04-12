using System.Collections.Generic;
using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CommunityNames
    {
        public CommunityNames()
        {

        }

        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public long? Status { get; set; }
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }
    }
    public class CommunityRequst
    {
        public CommunityRequst()
        {

        }

        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public long? Status { get; set; }
        [JsonProperty("community_data", NullValueHandling = NullValueHandling.Ignore)]
        public string Data { get; set; }
    }
}