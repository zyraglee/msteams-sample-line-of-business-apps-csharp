using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Common.NotificationBot.Models
{
    public class O365ConnectorActionRequest
    {
        public string Value { get; set; }
        public string ActionId { get; set; }
        public string Members { get; set; }
        
    }
}