using WoWonderClient.Classes.User;
using Newtonsoft.Json;
using System.Collections.Generic;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.CustomApi.Classes.Search
{
    internal class GetCustomSearchObject : GetSearchObject
    {
        [JsonProperty("communities", NullValueHandling = NullValueHandling.Ignore)]
        public List<CommunityDataObject> Communities { get; set; }
    }
}