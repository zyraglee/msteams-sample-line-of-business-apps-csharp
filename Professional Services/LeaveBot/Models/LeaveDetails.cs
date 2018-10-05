using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{
    public class LeaveDetails
    {
        [JsonIgnore]
        public const string TYPE = "LeaveDetails";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; } = TYPE;

        [JsonProperty(PropertyName = "id")]
        public string LeaveId { get; set; }

        [JsonProperty(PropertyName = "appliedByEmailId")]
        public string AppliedByEmailId { get; set; }

        [JsonProperty(PropertyName = "managerEmailId")]
        public string ManagerEmailId { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public LeaveDate StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public LeaveDate EndDate { get; set; }

        [JsonProperty(PropertyName = "leaveType")]
        public LeaveType LeaveType { get; set; }

        [JsonProperty(PropertyName = "employeeComment")]
        public string EmployeeComment { get; set; }

        [JsonProperty(PropertyName = "managerComment")]
        public string ManagerComment { get; set; }

        [JsonProperty(PropertyName = "status")]
        public LeaveStatus Status { get; set; }

        [JsonProperty(PropertyName = "leaveCategory")]
        public LeaveCategory LeaveCategory { get; set; }

    }

    public class LeaveDate
    {
        [JsonProperty(PropertyName = "date")]
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "type")]
        public DayType Type { get; set; }
    }

    public enum DayType
    {
        FullDay,
        HalfDay
    }

    public enum LeaveCategory
    {
        Vacation,
        Sickness,
        Personal,
        Other
    }
    

    public enum LeaveType
    {
        PaidLeave,
        SickLeave,
        OptionalLeave,
        CarriedLeave,
        MaternityLeave,
        PaternityLeave,
        Caregiver,
        // Arun - Add types
    }

    public enum LeaveStatus
    {
        Pending,
        Approved,
        Rejected,
        Withdrawn
        // Arun - Add types
    }

    public class LeaveExtended : LeaveDetails
    {
        public int DaysDiff { get; set; }

        public string startDay { get; set; }
        public string EndDay { get; set; }
        public string StartDateval { get; set; }

        public string EndDateVal { get; set; }
        public List<LeaveDetails> leavesData { get; set; }

        public int Totalleaves { get; set; }

        public string lastUsed { get; set; }

        public string BaseUri { get; set; }
    }
}