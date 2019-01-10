using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossVertical.Announcement.ViewModels
{
    public class TabListViewModel
    {
        public string SelectedTab { get; set; }
        public ListDetails FirstTab { get; set; }
        public ListDetails SecondTab { get; set; }
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