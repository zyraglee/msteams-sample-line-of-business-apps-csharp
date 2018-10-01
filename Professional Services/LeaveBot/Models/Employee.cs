using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Models
{
    public class Employee
    {
        public string EmailId { get; set; }

        public string UserUniqueId { get; set; }

        public string TenantId { get; set; }

        public string PhotoPath { get; set; }

        public string ManagerEmailId { get; set; }

        public string Name { get; set; }

        public string DemoManagerEmailId { get; set; }

        public LeaveBalance LeaveBalance { get; set; }
    }

    public class LeaveBalance
    {
        public int PersonalLeave { get; set; }
        public int SickLeave { get; set; }
        public int OptionalLeave { get; set; }
        // Add if needed
    }
}