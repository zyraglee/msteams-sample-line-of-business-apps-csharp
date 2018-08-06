using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Model
{
    public class AirCraftInfo
    {
        [JsonProperty(PropertyName = "flightNumber")]
        public string FlightNumber { get; set; }
        [JsonProperty(PropertyName = "model")]
        public string Model { get; set; }
        [JsonProperty(PropertyName = "capacity")]
        public string Capacity { get; set; }
        [JsonProperty(PropertyName = "flightType")]
        public string FlightType { get; set; }
        [JsonProperty(PropertyName = "baseLocation")]
        public string BaseLocation { get; set; }

        [JsonProperty(PropertyName = "aircraftId")]
        public string AircraftId { get; set; }
    }

    
}