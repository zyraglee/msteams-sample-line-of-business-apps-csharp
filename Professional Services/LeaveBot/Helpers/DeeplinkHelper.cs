using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Helpers
{
    public static class DeeplinkHelper
    {
        public static string LeaveBoardDeeplink { get; set; } =
            $"https://teams.microsoft.com/l/entity/{ApplicationSettings.AppId}/com.contoso.LeaveBot.leaveboard?webUrl={HttpUtility.UrlEncode(ApplicationSettings.BaseUrl)}&label=Leave%20Board";

        public static string PublicHolidaysDeeplink { get; set; } =
            $"https://teams.microsoft.com/l/entity/{ApplicationSettings.AppId}/com.contoso.LeaveBot.holidays?webUrl={HttpUtility.UrlEncode(ApplicationSettings.BaseUrl+"/first")}&label=Public%20Holidays";

        public static string HelpDeeplink { get; set; } =
            $"https://teams.microsoft.com/l/entity/{ApplicationSettings.AppId}/com.contoso.LeaveBot.help?webUrl={HttpUtility.UrlEncode(ApplicationSettings.BaseUrl+"/second")}&label=Help";
    }
}