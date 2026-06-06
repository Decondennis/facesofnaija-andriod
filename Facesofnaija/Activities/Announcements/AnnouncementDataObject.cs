using Newtonsoft.Json;
using System.Collections.Generic;

namespace Facesofnaija.Activities.Announcements
{
    public class AnnouncementsRootObject
    {
        [JsonProperty("api_status")]
        public string ApiStatus { get; set; }

        [JsonProperty("api_text")]
        public string ApiText { get; set; }

        [JsonProperty("api_version")]
        public string ApiVersion { get; set; }

        [JsonProperty("announcements")]
        public List<AnnouncementDataObject> Announcements { get; set; }
    }

    public class AnnouncementDataObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("active")]
        public string Active { get; set; }

        [JsonProperty("text_decode")]
        public string TextDecode { get; set; }

        [JsonProperty("time_text")]
        public string TimeText { get; set; }

        [JsonProperty("is_viewed")]
        public string IsViewed { get; set; }
    }
}
