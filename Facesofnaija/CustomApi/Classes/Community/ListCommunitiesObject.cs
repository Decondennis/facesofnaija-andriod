using System.Collections.Generic;
using Newtonsoft.Json;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.CustomApi.Classes.Community
{
    public class ListCommunitiesObject
    {
        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public long? Status { get; set; }
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public List<CommunityDataObject> Data { get; set; }
    }
}