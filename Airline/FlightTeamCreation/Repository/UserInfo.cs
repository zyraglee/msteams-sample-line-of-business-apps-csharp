using SimpleEchoBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleEchoBot.Repository
{
    public class UserInfoRepository
    {
        public static Dictionary<string, UserInfo> UserList { get; set; } = new Dictionary<string, UserInfo>();


        public static void AddUserInfo(UserInfo userInfo)
        {
            if (UserList.ContainsKey(userInfo.userId))
                UserList[userInfo.userId] = userInfo;
            else
                UserList.Add(userInfo.userId, userInfo);
        }

        public static UserInfo GetUserInfo(string Id)
        {
            if (UserList.ContainsKey(Id))
                return UserList[Id];
            return null;
        }
    }
}