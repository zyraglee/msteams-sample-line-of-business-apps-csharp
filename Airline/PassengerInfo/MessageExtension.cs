using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ContosoAirline.Helper;
using ContosoAirline.Model;
using ContosoAirline.Repository;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;

namespace Airline.PassengerInfo.Web
{
    public class MessageExtension
    {
        public async static Task<ComposeExtensionResponse> HandleMessageExtensionQuery(Activity activity)
        {
            var query = activity.GetComposeExtensionQueryData();
            if (query == null || query.CommandId != "search")
            {
                // We only process the 'getRandomText' queries with this message extension
                return null;
            }

            var title = "";
            var titleParam = query.Parameters?.FirstOrDefault(p => p.Name == "searchText");
            if (titleParam != null)
            {
                title = titleParam.Value.ToString();
            }

            var attachments = new List<ComposeExtensionAttachment>();

            var passengers = await DocumentDBRepository<Passenger>.GetItemsAsync(d => d != null && (d.Name.Contains(title) || d.Seat.Contains(title)));


            foreach (var passenger in passengers)
            {
                var card = O365CardHelper.GetO365ConnectorCard(passenger);
                var preview = O365CardHelper.GetPreviewCard(passenger);

                attachments.Add(card
                .ToAttachment()
                    .ToComposeExtensionAttachment(preview.ToAttachment()));
            }

            var response = new ComposeExtensionResponse(new ComposeExtensionResult("list", "result"));
            response.ComposeExtension.Attachments = attachments.ToList();

            return response;


        }

        //private static ComposeExtensionAttachment GetAttachment(string title = null)
        //{
        //    var section = new O365ConnectorCardSection
        //    {
        //        Title = "Passenger Details",
        //        ActivityTitle = "Mr. Vishal Akkalkote",
        //        ActivitySubtitle = "Seat No **29F**",
        //        ActivityText = "Special Instruction: None",
        //        ActivityImage = "https://contosoairline.azurewebsites.net/public/resources/male.jpg",
        //        Facts = new List<O365ConnectorCardFact>
        //            {
        //                new O365ConnectorCardFact("From", "Bengaluru"),
        //                new O365ConnectorCardFact("To", "Hyderabad"),
        //                new O365ConnectorCardFact("Gate", "2"),
        //                new O365ConnectorCardFact("Date", "25 April 18"),
        //                new O365ConnectorCardFact("Flight No", "241"),
        //                new O365ConnectorCardFact("Class", "P"),
        //                new O365ConnectorCardFact("Board Time", "16:05"),
        //                new O365ConnectorCardFact("Departure Time", "16:45"),
        //                new O365ConnectorCardFact("PNR", "DBW6WK"),
        //            }
        //    };

        //    O365ConnectorCard card = new O365ConnectorCard()
        //    {
        //        ThemeColor = "#E67A9E",
        //        Sections = new List<O365ConnectorCardSection> { section },
        //    };

        //    var preview = new ThumbnailCard
        //    {
        //        Title = "Vishal Akkalkote",
        //        Text = "29F",
        //        Images = { new CardImage("https://contosoairline.azurewebsites.net/public/resources/male.jpg") }
        //    };

        //    return card
        //        .ToAttachment()
        //        .ToComposeExtensionAttachment(preview.ToAttachment());
        //}
    }
}
