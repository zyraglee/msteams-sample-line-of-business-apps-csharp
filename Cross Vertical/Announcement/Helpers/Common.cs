using CrossVertical.Announcement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CrossVertical.Announcement.Helpers
{
    public class Common
    {
        public static Role GetUserRole(string emailId, Tenant tenatInfo)
        {
            var role = Role.User;
            if (tenatInfo.Moderators.Contains(emailId))
                role = Role.Moderator;
            if (tenatInfo.Admin == emailId)
                role = Role.Admin;
            return role;
        }
    }
}