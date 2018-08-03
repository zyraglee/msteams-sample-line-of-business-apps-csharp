using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;

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
            if (message.Text != null && message.Text.Contains("Show Baggage by Name"))
            {
                
                var reply = context.MakeMessage();
                var PNRToSearch = System.Text.RegularExpressions.Regex.Match(message.Text, @"\(([^)]*)\)").Groups[1].Value;
                var list = await DocumentDBRepository<Baggage>.GetItemsAsync(d => d.PNR.ToLower() == PNRToSearch.ToLower());
                var BaggagebyPNR = O365CardHelper.GetO365ConnectorCard(list.FirstOrDefault());
                
                reply.Attachments.Add(BaggagebyPNR.ToAttachment());
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
            var section = new O365ConnectorCardSection {
                ActivityTitle= "Track customer belongings",
                Text = "Using this bot you can<ol><li>Check baggage status</li><li>Track current location</li><li>Re-assign baggage</li><li>Report missing</li></ol> Choose one of the options below to retrieve details"
            };
                     
            var PNRNumberCard = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Baggage by PNR",
                "PNR",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardTextInput(
                        O365ConnectorCardTextInput.Type,
                        "pnrNumberInput",
                        true,
                        "Enter PNR number",
                        null,
                        false,
                        null)
                },
                new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Show",
                        Constants.PNR,
                        @"{""Value"":""{{pnrNumberInput.value}}""}")
                });
            var TicketNumnerCard = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Baggage by Ticket#",
                "Ticket Number",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardTextInput(
                        O365ConnectorCardTextInput.Type,
                        "ticketNumberInput",
                        true,
                        "Enter Ticket number",
                        null,
                        false,
                        null)
                },
                new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Show",
                        Constants.TicketNumber,
                        @"{""Value"":""{{ticketNumberInput.value}}""}")
                });
            var NameCard = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Baggage by Passenger Name",
                "Name",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardTextInput(
                        O365ConnectorCardTextInput.Type,
                        "NameInput",
                        true,
                        "Enter Passenger Name",
                        null,
                        false,
                        null)
                },
                new List<O365ConnectorCardActionBase>
                {
                    new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Show",
                        Constants.Name,
                        @"{""Value"":""{{NameInput.value}}""}")
                });

            


            O365ConnectorCard card = new O365ConnectorCard()
            {
                ThemeColor = "#E67A9E",
                Title = "Passenger Baggage Information",
                Sections = new List<O365ConnectorCardSection> { section },
                PotentialAction = new List<O365ConnectorCardActionBase>
                {
                    PNRNumberCard,
                    TicketNumnerCard,
                    NameCard
                }
            };
            return card.ToAttachment();

        }

        
    }
}
