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
            if (message.Text != null && message.Text.Contains("Show Flights by Number"))
            {

                var reply = context.MakeMessage();
                var flightnumber = System.Text.RegularExpressions.Regex.Match(message.Text, @"\(([^)]*)\)").Groups[1].Value;
                var list = await DocumentDBRepository<FlightInfo>.GetItemsAsync(d => d.FlightNumber == flightnumber);
                var flightresultCard = O365CardHelper.GetO365ConnectorCardResult(list.FirstOrDefault());

                reply.Attachments.Add(flightresultCard.ToAttachment());
                await context.PostAsync((reply));

            }
            else
            {
                var messageText = message.Text.ToLower();
                var reply = context.MakeMessage();
                List<Cities> lst = new List<Cities>();
                Cities obj = new Cities();
                obj.CityName = "Seattle";
                obj.CityCode = "SEA";
                lst.Add(obj);
                Cities obj1 = new Cities();
                obj1.CityName = "Newark";
                obj1.CityCode = "EWR";
                lst.Add(obj1);
                Cities obj3 = new Cities();
                obj3.CityCode = "BWI";
                obj3.CityName = "Washington, DC";
                lst.Add(obj3);
                Cities obj4 = new Cities();
                obj4.CityCode = "BSZ";
                obj4.CityName = "Boston";
                lst.Add(obj4);
                Cities obj5 = new Cities();
                obj5.CityCode = "JFK";
                obj5.CityName = "New York";
                lst.Add(obj5);
                Cities obj6 = new Cities();
                obj6.CityCode = "ORD";
                obj6.CityName = "Chicago";
                lst.Add(obj6);
                var citieslist = lst;
                reply.Attachments.Add(GetCardsInformation(citieslist));

                await context.PostAsync((reply));
                context.Wait(MessageReceivedAsync);
            }

        }

      

            public static Attachment GetCardsInformation(IEnumerable<Cities> citieslist)
        {

            var list = new List<O365ConnectorCardMultichoiceInputChoice>();
            foreach (var city in citieslist)
            {
                list.Add(new O365ConnectorCardMultichoiceInputChoice(city.CityName, city.CityCode));
            }


            var section = new O365ConnectorCardSection
            {
                ActivityTitle = "Your one stop for all flight Details",
                
            };
            
            
            var FlightInfo = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Show Flights",
                "Multiple Choice Card",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "From",
                        true,
                        "From",
                        null,
                        list,
                        "compact",
                        false),
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "To",
                        true,
                        "To",
                        null,
                        list,
                        "compact",
                        false),
                     new O365ConnectorCardDateInput(
                        O365ConnectorCardDateInput.Type,
                        "journeyDate",
                        true,
                        "Journey Date",
                        null,
                        false)
               },
               

            new List<O365ConnectorCardActionBase>
                  {
                   new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Show flights",
                        Constants.ShowFlights,
                        @"{""From"":""{{From.value}}"", ""To"":""{{To.value}}"" , ""JourneyDate"":""{{journeyDate.value}}"" }")
                 });

            var multichoiceCard = new O365ConnectorCardActionCard(
                O365ConnectorCardActionCard.Type,
                "Multiple Choice",
                "Multiple Choice Card",
                new List<O365ConnectorCardInputBase>
                {
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "CardsType",
                        true,
                        "Pick multiple options",
                        null,
                        new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Hero Card", "Hero Card"),
                            new O365ConnectorCardMultichoiceInputChoice("Thumbnail Card", "Thumbnail Card"),
                            new O365ConnectorCardMultichoiceInputChoice("O365 Connector Card", "O365 Connector Card")
                        },
                        "expanded",
                        true),
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "Teams",
                        true,
                        "Pick multiple options",
                        null,
                        new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Bot", "Bot"),
                            new O365ConnectorCardMultichoiceInputChoice("Tab", "Tab"),
                            new O365ConnectorCardMultichoiceInputChoice("Connector", "Connector"),
                            new O365ConnectorCardMultichoiceInputChoice("Compose Extension", "Compose Extension")
                        },
                        "compact",
                        true),
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "Apps",
                        false,
                        "Pick an App",
                        null,
                        new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("VSTS", "VSTS"),
                            new O365ConnectorCardMultichoiceInputChoice("Wiki", "Wiki"),
                            new O365ConnectorCardMultichoiceInputChoice("Github", "Github")
                        },
                        "expanded",
                        false),
                    new O365ConnectorCardMultichoiceInput(
                        O365ConnectorCardMultichoiceInput.Type,
                        "OfficeProduct",
                        false,
                        "Pick an Office Product",
                        null,
                        new List<O365ConnectorCardMultichoiceInputChoice>
                        {
                            new O365ConnectorCardMultichoiceInputChoice("Outlook", "Outlook"),
                            new O365ConnectorCardMultichoiceInputChoice("MS Teams", "MS Teams"),
                            new O365ConnectorCardMultichoiceInputChoice("Skype", "Skype")
                        },
                        "compact",
                        false)
            },

            new List<O365ConnectorCardActionBase>
                  {
                   new O365ConnectorCardHttpPOST(
                        O365ConnectorCardHttpPOST.Type,
                        "Send",
                        "multichoice",
                        @"{""CardsType"":""{{CardsType.value}}"", ""Teams"":""{{Teams.value}}"", ""Apps"":""{{Apps.value}}"", ""OfficeProduct"":""{{OfficeProduct.value}}""}")
                 });

            O365ConnectorCard card = new O365ConnectorCard()
            {
                ThemeColor = "#E67A9E",
                Title = "Flight Information",
                Sections = new List<O365ConnectorCardSection> { section },
                PotentialAction = new List<O365ConnectorCardActionBase>
                {
                    FlightInfo
                    
                }
            };
            return card.ToAttachment();

        }
    }
}
