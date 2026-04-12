using Newtonsoft.Json;
using WoWonderClient.Classes.Posts;
using Facesofnaija.CustomApi.Classes.Global;

namespace Facesofnaija.CustomApi.Classes.Posts
{
    internal class CustomPostDataObject : PostDataObject
    {
        [JsonProperty("community_recipient", NullValueHandling = NullValueHandling.Ignore)]
        public CommunityDataObject CommunityRecipient { get; set; }
    }
}