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
          new PublicHoliday() {Date = new DateTime(2018, 10, 02), Title = "Gandhi Jayanti" },
            new PublicHoliday() {Date = new DateTime(2018, 10, 19), Title = "Dussehra" },
            new PublicHoliday() {Date = new DateTime(2018, 11, 01), Title = "Kannada Rajyotsava" },
            new PublicHoliday() {Date = new DateTime(2018, 11, 08), Title = "Diwali" },
            new PublicHoliday() {Date = new DateTime(2018, 12, 25), Title = "Christmas" }
        };
    }

    public class PublicHoliday
    {
        public DateTime Date { get; set; }

        public string Title { get; set; }
    }
}