using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Teams.Samples.HelloWorld.Web.Model;


using Microsoft.Bot.Connector;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Helper
{
    public static class O365CardHelper
    {
        public static Attachment GetListofFlights(IEnumerable<AirCraftInfo> aircraftDetails)
        {
            var listCard = new ListCard();
            listCard.content = new Content();
            //listCard.content.title = "The following flights are avilibile at Base Location " + JourneyDate.ToShortDateString();
            var list = new List<Item>();

            foreach (var aircraft in aircraftDetails)
            {
                //DateTime journeydate = TimeZoneInfo.ConvertTimeToUtc(JourneyDate.Date);
                //DateTime dabasedate = TimeZoneInfo.ConvertTimeToUtc(flight.JourneyDate.Date);
                //if (journeydate.Date>DateTime.Now.Date)
                //{
                listCard.content.title = "The following aircrafts are available at " + aircraft.BaseLocation;

                var item = new Item();
                item.icon = "https://airlinebaggage.azurewebsites.net/resources/Flight.png";
                item.type = "resultItem";
                item.id = aircraft.FlightNumber;
                item.title = "Aircraft ID: " + aircraft.AircraftId + "   |" + " Type: " + aircraft.FlightType;
                item.subtitle = "Model: " + aircraft.Model + "   |" + " Capacity: " + aircraft.Capacity;

                item.tap = new Tap()
                {
                    type = "imBack",
                    title = "Aircraft",
                    value = "Show Aircraft by Id " + " (" + aircraft.AircraftId + ")"
                };
                list.Add(item);
                //}

            }

            listCard.content.items = list.ToArray();

            Attachment attachment = new Attachment();
            attachment.ContentType = listCard.contentType;
            attachment.Content = listCard.content;
            return attachment;
        }
        public static O365ConnectorCard GetO365ConnectorCardResult(AirCraftInfo flight)
        {
            var section = new O365ConnectorCardSection
            {
                ActivityTitle = $"Aircraft Id: **{flight.AircraftId}**",
                ActivitySubtitle = $"Model: **{flight.Model}**",
                ActivityImage = "https://airlinebaggage.azurewebsites.net/resources/Flight.png",
                Facts = new List<O365ConnectorCardFact>
                    {

                        new O365ConnectorCardFact("Base location", flight.BaseLocation),
                        new O365ConnectorCardFact("Capacity", flight.Capacity),
                        new O365ConnectorCardFact("Flight type",flight.FlightType),
                        //new O365ConnectorCardFact("Flight Number",flight.FlightNumber)

                    }
            };
            //var MarkGrounded = new O365ConnectorCardActionCard(
            //    O365ConnectorCardActionCard.Type,
            //    "Mark as grounded",
            //    "FlightNumber",
            //    new List<O365ConnectorCardInputBase>
            //    {
            //        new O365ConnectorCardTextInput(
            //            O365ConnectorCardTextInput.Type,
            //            "flightNumberInput",
            //            true,
            //            "Enter Aircraft Id",
            //            null,
            //            false,
            //            null)

            //    },
            //    new List<O365ConnectorCardActionBase>
            //    {
            //        new O365ConnectorCardHttpPOST(
            //            O365ConnectorCardHttpPOST.Type,
            //            "Mark as grounded",
            //            Constants.MarkGrounded,
            //            @"{""Value"":""{{flightNumberInput.value}}""}")
            //           //@"{""PNRValue"":""{{pnrNumberInput.value}}"", ""NewFlightValue"":""{{flightNumberInput.value}}""}"),

            //         //@"{""CardsType"":""{{CardsType.value}}"", ""Teams"":""{{Teams.value}}"", ""Apps"":""{{Apps.value}}"", ""OfficeProduct"":""{{OfficeProduct.value}}""}")


            //    });

            O365ConnectorCard card = new O365ConnectorCard()
            {
                Title = "Assign an Aircraft",
                ThemeColor = "#E67A9E",
                Sections = new List<O365ConnectorCardSection> { section },
                PotentialAction = new List<O365ConnectorCardActionBase>
                {
                     new O365ConnectorCardHttpPOST(O365ConnectorCardHttpPOST.Type,"Assign the new aircraft",Constants.Assignaircraft,$"{{'FlightNumber':'{flight.FlightNumber}','AircraftId':'{flight.AircraftId}'}}"),
                     new O365ConnectorCardHttpPOST(O365ConnectorCardHttpPOST.Type,"Mark as grounded",Constants.MarkGrounded,$"{{'Value':'{flight.AircraftId}'}}"),
                    // @"{""FlightNumber"":""{{flightNumberInput.value}}"", ""BaseLocation"":""{{baselocationInput.value}}""}")
                }


            };
            return card;
        }
    }





    public class O365BodyValue
    {
        public string Value { get; set; }
    }

    public class AirCraftDetails
    {
        public string FlightNumber { get; set; }
        public string BaseLocation { get; set; }

        public string AircraftId { get; set; }
    }

}