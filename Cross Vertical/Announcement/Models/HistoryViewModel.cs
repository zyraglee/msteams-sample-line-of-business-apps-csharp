using System.Collections.Generic;

namespace CrossVertical.Announcement.Models
{
    public class HistoryViewModel
    {
        public List<PostDetails> Posts { get; set; } = new List<PostDetails>();
        public Role Role { get; set; }
    }
}