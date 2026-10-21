using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main(string[] args)
    {
        var json = File.ReadAllText(@"sample_general_data.json");
        var jObj = JObject.Parse(json);

        Console.WriteLine("=== Testing JObject properties ===");
        var notifsToken = jObj["notifications"];
        Console.WriteLine($"notifications type: {notifsToken?.Type}, is array: {notifsToken is JArray}");
        if (notifsToken is JArray arr)
        {
            Console.WriteLine($"Count: {arr.Count}");
            foreach (var item in arr)
            {
                Console.WriteLine($"Item id: {item["id"]}, type: {item["type"]}, text: {item["type_text"]}, url: {item["url"]}");
            }
        }

        Console.WriteLine("=== Testing WoWonderClient.GetGeneralDataObject deserialization ===");
        try
        {
            var asm = System.Reflection.Assembly.LoadFrom(@"C:\Users\Dell\source\repos\workspace\Facesofnaija-andriod\API\WoWonderClient.dll");
            var type = asm.GetType("WoWonderClient.Classes.Global.GetGeneralDataObject");
            Console.WriteLine($"Found type: {type?.FullName}");
            var obj = JsonConvert.DeserializeObject(json, type);
            Console.WriteLine($"Deserialized GetGeneralDataObject successfully! Object: {obj}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FAILED GetGeneralDataObject deserialization: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  InnerException: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
        }
    }

    public class TestNotificationObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("notifier_id")]
        public string NotifierId { get; set; }

        [JsonProperty("recipient_id")]
        public string RecipientId { get; set; }

        [JsonProperty("post_id")]
        public string PostId { get; set; }

        [JsonProperty("page_id")]
        public string PageId { get; set; }

        [JsonProperty("group_id")]
        public string GroupId { get; set; }

        [JsonProperty("event_id")]
        public string EventId { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("type2")]
        public string Type2 { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("full_link")]
        public string FullLink { get; set; }

        [JsonProperty("seen")]
        public string Seen { get; set; }

        [JsonProperty("time")]
        public string Time { get; set; }

        [JsonProperty("ajax_url")]
        public string AjaxUrl { get; set; }

        [JsonProperty("type_text")]
        public string TypeText { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("time_text_string")]
        public string TimeTextString { get; set; }

        [JsonProperty("time_text")]
        public string TimeText { get; set; }

        [JsonProperty("community_id")]
        public string CommunityId { get; set; }

        [JsonProperty("comment_id")]
        public string CommentId { get; set; }

        [JsonProperty("reply_id")]
        public string ReplyId { get; set; }

        [JsonProperty("story_id")]
        public string StoryId { get; set; }

        [JsonProperty("blog_id")]
        public string BlogId { get; set; }

        [JsonProperty("notifier")]
        public TestUserDataObject Notifier { get; set; }
    }

    public class TestUserDataObject
    {
        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("avatar")]
        public string Avatar { get; set; }
    }
}