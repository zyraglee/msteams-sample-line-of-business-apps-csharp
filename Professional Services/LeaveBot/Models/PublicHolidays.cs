using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using System;
using System.Collections.Generic;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{
    // Static class
    public static class PublicHolidaysList
    {
        public static List<PublicHoliday> HolidayList { get; set; } = new List<PublicHoliday>()
        {
          new PublicHoliday() {Date = new DateTime(2018, 10, 02), Title = "Gandhi Jayanti",ImagePath = null,OptionalHoliday = null,CelebrationText = null },
            new PublicHoliday() {Date = new DateTime(2018, 10,22 ), Title = "Dussehra",ImagePath = null,OptionalHoliday = "Optional",CelebrationText = null },
            new PublicHoliday() {Date = new DateTime(2018, 11, 01), Title = "Kannada Rajyotsava",ImagePath = null,OptionalHoliday = null,CelebrationText = null },
            new PublicHoliday() {Date = new DateTime(2018, 11, 08), Title = "Diwali",ImagePath = ApplicationSettings.BaseUrl + "/Resources/Diwali.PNG",OptionalHoliday = null,CelebrationText = "Office Celebrations on a day before" },
            new PublicHoliday() {Date = new DateTime(2018, 12, 25), Title = "Christmas",ImagePath = null,OptionalHoliday = null,CelebrationText = null }
        };
    }

    public class PublicHoliday
    {
        public DateTime Date { get; set; }

        public string Title { get; set; }

        public string ImagePath { get; set; }

        public string OptionalHoliday { get; set; }

        public string CelebrationText { get; set; }
    }
}