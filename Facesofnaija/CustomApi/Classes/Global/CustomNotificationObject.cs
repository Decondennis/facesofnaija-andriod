using WoWonderClient.Classes.Global;
using Newtonsoft.Json;
using WoWonderClient.Classes.Event;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CustomNotificationObject : NotificationObject
    {
        [JsonProperty("community_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityId { get; set; }
    }
}