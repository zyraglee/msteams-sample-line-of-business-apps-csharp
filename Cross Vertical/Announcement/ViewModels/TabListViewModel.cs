using System.Collections.Generic;

namespace CrossVertical.Announcement.ViewModels
{
    public class TabListViewModel
    {
        public bool SelectFirstTab { get; set; }
        public ListDetails FirstTab { get; set; } = new ListDetails();
        public ListDetails SecondTab { get; set; } = new ListDetails();
    }

    public class ListDetails
    {
        public List<Item> Items { get; set; }
        public string Title { get; set; }
    }

    public class Item
    {
        public string Title { get; set; }
        public string SubTitle { get; set; }
        // Add remaining properties.
    }
}