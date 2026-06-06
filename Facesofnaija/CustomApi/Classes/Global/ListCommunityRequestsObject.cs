using System.Collections.Generic;
using Newtonsoft.Json;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class ListCommunityRequestsObject
    {
        [JsonProperty("api_status", NullValueHandling = NullValueHandling.Ignore)]
        public long? Status { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public List<CommunityRequestDataObject> Data { get; set; }
    }
}
