using Newtonsoft.Json;

namespace CrossVertical.Announcement.Models
{
    public class User
    {
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(User);

        [JsonProperty("id")]
        public string EmailId { get; set; }
        public string Name { get; set; }
        public string BotConversationId { get; set; }
    }
}