using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{

    public class Employee
    {
        [JsonIgnore]
        public const string TYPE = "Employee";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = TYPE;

        [JsonProperty(PropertyName = "id")]
        public string EmailId { get; set; }

        [JsonProperty(PropertyName = "userUniqueId")]
        public string UserUniqueId { get; set; }

        [JsonProperty(PropertyName = "tenantId")]
        public string TenantId { get; set; }

        [JsonProperty(PropertyName = "photoPath")]
        public string PhotoPath { get; set; }

        [JsonProperty(PropertyName = "ManagerEmailId")]
        public string ManagerEmailId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "demoManagerEmailId")]
        public string DemoManagerEmailId { get; set; }

        [JsonProperty(PropertyName = "leaveBalance")]
        public LeaveBalance LeaveBalance { get; set; }

    }

    public class LeaveBalance
    {
        public int PersonalLeave { get; set; }
        public int SickLeave { get; set; }
        public int OptionalLeave { get; set; }
        // Add if needed
    }

    //public abstract class DocumentModel
    //{
    //    [JsonProperty(PropertyName = "type")]
    //    public abstract string Type { get; set; }
    //}
}