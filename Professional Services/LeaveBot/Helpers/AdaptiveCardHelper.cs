using AdaptiveCards;
using Microsoft.Bot.Connector;
using System;
using System.IO;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Helpers
{
    /// <summary>
    ///  Helper class which posts to the saved channel every 20 seconds.
    /// </summary>
    public static class AdaptiveCardHelper
    {

        public static Attachment GetAdaptiveCard()
        {
            // Parse the JSON 
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(GetAdaptiveCardJson());

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = result.Card

            };
        }

        public static Attachment GetAdaptiveCardFromJosn(string json)
        {
            // Parse the JSON 
            AdaptiveCardParseResult result = AdaptiveCard.FromJson(json);

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = result.Card

            };
        }

        public static String GetAdaptiveCardJson()
        {
            var path = System.Web.Hosting.HostingEnvironment.MapPath(@"~\Cards\AdaptiveCard.json");
            return File.ReadAllText(path);
        }

    }
}