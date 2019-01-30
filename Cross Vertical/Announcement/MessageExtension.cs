using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;using CrossVertical.Announcement.Helpers;
using CrossVertical.Announcement.Models;
using Faker;
using CrossVertical.Announcement.Repository;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using System.Threading.Tasks;
using System.Web.Mvc;
namespace CrossVertical.Announcement
{
    public class MessageExtension
    {
        public async static Task<ComposeExtensionResponse> HandleMessageExtensionQuery(ConnectorClient connector, Activity activity,string tid,string emailId)

        {

            var query = activity.GetComposeExtensionQueryData();

            if (query == null || query.CommandId != "getMyAnnouncements")

            {

                // We only process the 'getRandomText' queries with this message extension

                return null;

            }



            var title = "";

            var titleParam = query.Parameters?.FirstOrDefault(p => p.Name == "cardTitle");

            if (titleParam != null)

            {

                title = titleParam.Value.ToString();

            }
            var attachments = new List<ComposeExtensionAttachment>();
           var announcements = await GetMyAnnouncements(tid, emailId);
           foreach (var item in announcements)
            {
                var attachment2 = GetAttachment(item);
                attachments.Add(attachment2);
            }
            var response = new ComposeExtensionResponse(new ComposeExtensionResult("list", "result"));
            response.ComposeExtension.Attachments = attachments.ToList();
            return response;

        }
        public static async Task<List<Campaign>> GetMyAnnouncements(string tid,string emailId)
        {
            
            var tenatInfo = await Cache.Tenants.GetItemAsync(tid);
            var myTenantAnnouncements = new List<Campaign>();
            emailId = emailId.ToLower();
            var myAnnouncements = tenatInfo.Announcements;
            Role role = Common.GetUserRole(emailId, tenatInfo);
            foreach (var announcementId in myAnnouncements)
            {
                var announcement = await Cache.Announcements.GetItemAsync(announcementId);
                if (announcement != null)
                {
                    if (role == Role.Moderator || role == Role.Admin)
                    {
                        myTenantAnnouncements.Add(announcement);
                    }
                    else if (announcement.Recipients.Channels.Any(c => c.Members.Contains(emailId))
                          || announcement.Recipients.Groups.Any(g => g.Users.Any(u => u.Id == emailId)))
                    {
                        // Validate if user is part of this announcement.
                        myTenantAnnouncements.Add(announcement);
                    }

                }
            }
            return myTenantAnnouncements;

        }



        private static ComposeExtensionAttachment GetAttachment(Campaign item)

        {

            var card = new ThumbnailCard

            {

                Title = item.Title/*!string.IsNullOrWhiteSpace(title) ? title : Faker.Lorem.Sentence()*/,

                Text = item.SubTitle,

             };
           // card.Images = new List<CardImage>() { new CardImage(item.ImageUrl) };
            card.Images = new List<CardImage>() { new CardImage("http://lorempixel.com/640/480?rand=" + DateTime.Now.Ticks.ToString()) };


            return card

                .ToAttachment()

                .ToComposeExtensionAttachment();

        }

    }

}