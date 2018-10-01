using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{
    // Static class
    public static class PublicHolidaysList
    {
        public static List<PublicHoliday> HolidayList { get; set; } = new List<PublicHoliday>()
        {
            new PublicHoliday() {Date = DateTime.Today, Title = "Gandhi Jayanti" },
            new PublicHoliday() {Date = DateTime.Today, Title = "Some holiday" }
        };
    }

    public class PublicHoliday
    {
        public DateTime Date { get; set; }

        public string Title { get; set; }
    }    
}