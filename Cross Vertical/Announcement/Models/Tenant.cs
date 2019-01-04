using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossVertical.Announcement.Models
{
    public class Tenant
    {
        [JsonProperty("type")]
        public string Type { get; set; } = nameof(Tenant);

        public string Id { get; set; }

        public bool IsAdminConsented { get; set; }

        public List<string> Teams { get; set; } = new List<string>();

        public List<string> Users { get; set; } = new List<string>();

        public List<string> Groups { get; set; } = new List<string>();

        public List<string> Announcements { get; set; } = new List<string>();
    }
}