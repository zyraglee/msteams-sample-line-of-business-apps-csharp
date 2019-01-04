using Newtonsoft.Json;
using System.Collections.Generic;

namespace CrossVertical.Announcement.Models
{
    public class Team
    {
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(Team);

        public string Id { get; set; }

        public string Name { get; set; }

        public List<Channel> Channels { get; set; } = new List<Channel>();
    }
}