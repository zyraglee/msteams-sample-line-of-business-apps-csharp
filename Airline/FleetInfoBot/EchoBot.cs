using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using System.Linq;
using System.Configuration;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    [Serializable]
    public class EchoBot: IDialog<object>

    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }
        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {

            var message = await argument;
            if (message.Text != null && message.Text.Contains("Show Aircraft by Id"))
            {

                var reply = context.MakeMessage();
                var flightnumber = System.Text.RegularExpressions.Regex.Match(message.Text, @"\(([^)]*)\)").Groups[1].Value;
                var list = await DocumentDBRepository<AirCraftInfo>.GetItemsAsync(d => d.AircraftId == flightnumber);
                var aircraftInfoCard = O365CardHelper.GetO365ConnectorCardResult(list.FirstOrDefault());

                reply.Attachments.Add(aircraftInfoCard.ToAttachment());
                await context.PostAsync((reply));

            }
            else
            {
                var messageText = message.Text.ToLower();
                var reply = context.MakeMessage();
                
                reply.Attachments.Add(GetCardsInformation());

                await context.PostAsync((reply));
                context.Wait(MessageReceivedAsync);
            }

        }

      

            public static Attachment GetCardsInformation()
        {

            var section = new O365ConnectorCardSection
            {
                ActivityTitle = "Your one stop destination to managing your fleet",
                
            };
            
            
            var AirCraftInfo = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Show me aircraft details",
                "Multiple Choice Card",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardTextInput(
                        O365ConnectorCardTextInput.Type,
                        "flightNumberInput",
                        true,
                        "Enter flight number (Ex:320,777,220)",
                        null,
                        false,
                        null),
                     new O365ConnectorCardTextInput(
                        O365ConnectorCardTextInput.Type,
                        "baselocationInput",
                        true,
                        "Enter Base Location(Ex:Seattle)",
                        null,
                        false,
                        null)

               },
               

            new List<O365ConnectorCardActionBase>
                  {
                   new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Show me aircraft details",
                        Constants.ShowAirCraftDetails,
                        @"{""FlightNumber"":""{{flightNumberInput.value}}"", ""BaseLocation"":""{{baselocationInput.value}}""}")
                 });

            O365ConnectorCard card = new O365ConnectorCard()
            {
                ThemeColor = "#E67A9E",
                Title = "Fleet Management",
                Sections = new List<O365ConnectorCardSection> { section },
                PotentialAction = new List<O365ConnectorCardActionBase>
                {
                    AirCraftInfo
                    
                }
            };
            return card.ToAttachment();

        }
    }
}
