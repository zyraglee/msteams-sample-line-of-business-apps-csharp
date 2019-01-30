using CrossVertical.Announcement.Helpers;
using CrossVertical.Announcement.Models;
using CrossVertical.Announcement.Repository;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace CrossVertical.Announcement
{
    public class MessageExtension
    {
        public async static Task<ComposeExtensionResponse> HandleMessageExtensionQuery(ConnectorClient connector, Activity activity, string tid, string emailId)
        {

            var query = activity.GetComposeExtensionQueryData();

            if (query == null || query.CommandId != "getMyMessages")
            {
                // We only process the 'getMyMessages' queries with this message extension
                return null;
            }

            var searchString = string.Empty;

            var titleParam = query.Parameters?.FirstOrDefault(p => p.Name == "title");

            if (titleParam != null)
            {
                searchString = titleParam.Value.ToString().ToLower();
            }
            var attachments = new List<ComposeExtensionAttachment>();
            var announcements = await GetMyAnnouncements(tid, emailId, searchString);
            foreach (var item in announcements.OrderByDescending(a => a.CreatedTime))
            {
                var attachment2 = GetAttachment(item);
                attachments.Add(attachment2);
            }
            var response = new ComposeExtensionResponse(new ComposeExtensionResult("list", "result"));
            response.ComposeExtension.Attachments = attachments.ToList();
            return response;

        }
        public static async Task<List<Campaign>> GetMyAnnouncements(string tid, string emailId, string searchString)
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
                    if (!announcement.Title.ToLower().Contains(searchString)
                        && !announcement.SubTitle.ToLower().Contains(searchString)
                        && (announcement.Author != null && !announcement.Author.Name.ToLower().Contains(searchString)))
                    {
                        continue;
                    }
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

        private static ComposeExtensionAttachment GetAttachment(Campaign campaign)
        {
            var previewCard = new ThumbnailCard
            {
                Title = campaign.Title/*!string.IsNullOrWhiteSpace(title) ? title : Faker.Lorem.Sentence()*/,
                Text = campaign.SubTitle,
            };
            previewCard.Images = new List<CardImage>() {
                new CardImage(Uri.IsWellFormedUriString(campaign.Author?.ProfilePhoto, UriKind.Absolute) ?
                campaign.Author?.ProfilePhoto : null ) };
            campaign.ShowAllDetailsButton = false;
            var card = campaign.GetPreviewCard().ToAttachment();
            campaign.ShowAllDetailsButton = true;
            return card
                .ToComposeExtensionAttachment(previewCard.ToAttachment());
        }
    }

}