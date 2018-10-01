using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{
    public class LeaveDetails
    {
        public string LeaveId { get; set; }

        public string AppliedByEmailId { get; set; }

        public LeaveDate StartDate { get; set; }

        public LeaveDate EndDate { get; set; }

        public LeaveType LeaveType { get; set; }

        public string EmployeeComment { get; set; }

        public string ManagerComment { get; set; }

        public LeaveStatus Status { get; set; }

    }

    public class LeaveDate
    {
        public DateTime Date { get; set; }
        public DayType LeaveType { get; set; }
    }

    public enum DayType
    {
        FullDay,
        HalfDay
    }

    public enum LeaveType
    {
        PersonalLeave,
        SickLeave
        // Arun - Add types
    }

    public enum LeaveStatus
    {
        PendingApproval,
        Approved,
        Rejected,
        Withdrawn
        // Arun - Add types
    }

}