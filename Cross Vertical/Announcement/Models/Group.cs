using Newtonsoft.Json;
using System.Collections.Generic;

namespace CrossVertical.Announcement.Models
{
    public class Group
    {
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(Group);

        public string Id { get; set; }

        public string Name { get; set; }

        public List<string> Users { get; set; }
    }
}