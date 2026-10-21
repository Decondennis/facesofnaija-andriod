using WoWonderClient.Classes.Global;
using Newtonsoft.Json;
using WoWonderClient.Classes.Event;

namespace Facesofnaija.CustomApi.Classes.Global
{
    public class CustomNotificationObject : NotificationObject
    {
        [JsonProperty("community_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CommunityId { get; set; }

        [JsonProperty("comment_id", NullValueHandling = NullValueHandling.Ignore)]
        public string CommentId { get; set; }

        [JsonProperty("reply_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ReplyId { get; set; }

        [JsonProperty("story_id", NullValueHandling = NullValueHandling.Ignore)]
        public string StoryId { get; set; }

        [JsonProperty("blog_id", NullValueHandling = NullValueHandling.Ignore)]
        public string BlogId { get; set; }
    }
}