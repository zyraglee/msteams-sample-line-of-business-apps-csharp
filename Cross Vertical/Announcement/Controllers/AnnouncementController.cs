using CrossVertical.Announcement.Repository;
using System.Web.Mvc;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using CrossVertical.Announcement.Dialogs;
using Microsoft.Bot.Connector;
using CrossVertical.Announcement.Models;

namespace CrossVertical.Announcement.Controllers
{
    public class AnnouncementController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            // Landing page
            return View();
        }

        [Route("history")]
        public async Task<ActionResult> History(string tid)
        {
            if (string.IsNullOrEmpty(tid))
            {
                return HttpNotFound();
            }
            var tenatInfo = await Cache.Tenants.GetItemAsync(tid);
            var myTenantAnnouncements = new List<Campaign>();

            foreach (var announcementId in tenatInfo.Announcements)
            {
                var announcement = await Cache.Announcements.GetItemAsync(announcementId);
                if (announcement != null)
                    myTenantAnnouncements.Add(announcement);
            }

            List<PostDetails> postDetails = new List<PostDetails>();
            foreach (var announcement in myTenantAnnouncements)
            {
                announcement.GetPreviewCard();
                //var campaign = announcement.Post as Campaign;
                PostDetails post = new PostDetails();
                post.Id = announcement.Id;
                post.Title = announcement.Title;
                post.Date = announcement.CreatedTime;
                post.Status = announcement.Status;
                post.MessageSensitivity = announcement.Sensitivity;

                var recipientCount = 0;
                var groupsNames = new List<string>();
                var channelNames = new List<string>();

                // Create anew class
                foreach (var group in announcement.Recipients.Groups)
                {
                    var groupname = await Cache.Groups.GetItemAsync(group.GroupId);
                    if (groupname == null)
                        continue;

                    groupsNames.Add(groupname.Name);
                    recipientCount += group.Users.Count;
                    post.LikeCount += group.Users.Sum(u => u.LikeCount);
                    post.AckCount += group.Users.Where(u => u.IsAcknoledged).Count();
                }
                foreach (var team in announcement.Recipients.Channels)
                {
                    var teamname = await Cache.Teams.GetItemAsync(team.TeamId);
                    if (teamname == null)
                        continue;
                    channelNames.Add(teamname.Name);
                    post.LikeCount += team.Channel.LikeCount;
                }
                if (recipientCount == 0 && announcement.Recipients != null && announcement.Recipients.Channels != null)
                    recipientCount = announcement.Recipients.Channels.Count;

                var maxChar = 40;
                var recipientNames = string.Empty;
                var recipientChannelNames = string.Empty;
                for (int i = 0; i < groupsNames.Count; i++)
                {
                    if (i != 0)
                        recipientNames += ", ";
                    recipientNames += groupsNames[i];

                    if (recipientNames.Length >= maxChar)
                    {
                        // Check the actual count
                        recipientNames += " +" + (groupsNames.Count - i);
                        break;
                    }

                }
                for (int i = 0; i < channelNames.Count; i++)
                {
                    if (i != 0)
                        recipientNames += ", ";
                    recipientChannelNames += channelNames[i];

                    if (recipientNames.Length >= maxChar)
                    {
                        // Check the actual count
                        recipientChannelNames += " +" + (channelNames.Count - i);
                        break;
                    }

                }
                post.RecipientCount = $"{recipientCount}";
                post.Recipients = $"{recipientNames}";
                post.RecipientChannelNames = $"{recipientChannelNames}";
                postDetails.Add(post);
            }

            return View(postDetails);
        }
        [Route("tabinfo")]
        public async Task<ActionResult> TabInfo()
        {
            return View();
        }
        [Route("create")]
        public async Task<ActionResult> Create(string Emailid)
        {
            User user1 = new User() { BotConversationId = "id1", EmailId = "user@microsoft.com", Name = "User 1" };
            User user2 = new User() { BotConversationId = "id2", EmailId = "user2@microsoft.com", Name = "User 2" };
            User user3 = new User() { BotConversationId = "id3", EmailId = "user3@microsoft.com", Name = "User 3" };
            Group group1 = new Group()
            {
                Id = "GroupId1",
                Name = "Group 1",
                Users = new List<string>() { user1.EmailId, user2.EmailId }
            };
            Group group2 = new Group()
            {
                Id = "GroupId2",
                Name = "Group 2",
                Users = new List<string>() { user2.EmailId, user3.EmailId }
            };

            Team team1 = new Team()
            {
                Id = "Team1",
                Name = "Team1",
                Channels = new List<Channel>()
                {
                    new Channel() { Id = "channel1", Name = "Channel 1" },
                    new Channel() { Id = "channel2", Name = "Channel 2" },
                    new Channel() { Id = "channel3", Name = "Channel 3" },
                }
            };

            Team team2 = new Team()
            {
                Id = "Team2",
                Name = "Team2",
                Channels = new List<Channel>()
                {
                    new Channel() { Id = "channel1", Name = "Channel 1" },
                    new Channel() { Id = "channel2", Name = "Channel 2" },
                    new Channel() { Id = "channel3", Name = "Channel 3" },
                }
            };

            Tenant tenant = new Tenant()
            {
                Id = "Tenant1",
                Groups = new List<string>() { group1.Id, group2.Id },
                Users = new List<string>() { user1.EmailId, user2.EmailId, user3.EmailId },
                Teams = new List<string>() { team1.Id, team2.Id }
            };

            Campaign campaignAnnouncement = new Campaign()
            {
                IsAcknowledgementRequested = true,
                IsContactAllowed = true,
                Title = "Giving Campaign 2018 is here",
                SubTitle = "Have you contributed to the mission?",
                Author = new Author()
                {
                    Name = "John Doe",
                    Role = "Director of Corporate Communications",
                },
                Preview = "The 2018 Employee Giving Campaign is officially underway! Our incredibly generous culture of employee giving is unique to Contoso, and has a long history going back to our founder and his family’s core belief and value in philanthropy. Individually and collectively, we can have an incredible impact no matter how we choose to give. We are all very fortunate and 2018 has been a good year for the company which we are all participating in. Having us live in a community with a strong social safety net benefits us all so lets reflect our participation in this year's success with participation in Give.",
                Body = "The 2018 Employee Giving Campaign is officially underway! Our incredibly generous culture of employee giving is unique to Contoso, and has a long history going back to our founder and his family’s core belief and value in philanthropy. Individually and collectively, we can have an incredible impact no matter how we choose to give. We are all very fortunate and 2018 has been a good year for the company which we are all participating in. Having us live in a community with a strong social safety net benefits us all so lets reflect our participation in this year's success with participation in Give. I hope you will take advantage of some of the fun and impactful opportunities that our giving team has put together and I’d like to thank our VPALs John Doe and Jason Natie for all the hard work they've put into planning these events for our team. To find out more about these opportunities, look for details in Give 2018",
                ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSG-vkjeuIlD-up_-VHCKgcREhFGp27lDErFkveBLQBoPZOHwMbjw",
                Sensitivity = MessageSensitivity.Information
            };

            campaignAnnouncement.Id = "Announcement3";
            campaignAnnouncement.TenantId = tenant.Id;

            var recipients = new RecipientInfo();
            recipients.Channels.Add(new ChannelRecipient()
            {
                TeamId = team1.Id,
                Channel = new RecipientDetails()
                {
                    Id = "channel1",
                }
            });

            recipients.Channels.Add(new ChannelRecipient()
            {
                TeamId = team2.Id,
                Channel = new RecipientDetails()
                {
                    Id = "channel2",
                }
            });

            recipients.Groups.Add(new GroupRecipient()
            {
                GroupId = group1.Id,
                Users = new List<RecipientDetails>() {
                    new RecipientDetails()
                    {
                        Id = user1.EmailId,
                    },
                    new RecipientDetails()
                    {
                        Id = user2.EmailId,
                    },
                }
            });
            campaignAnnouncement.Recipients = recipients;
            campaignAnnouncement.Status = Status.Draft;

            // Insert
            //await DocumentDBRepository.CreateItemAsync(user1);
            //await DocumentDBRepository.CreateItemAsync(user2);

            // Udpate
            //user1.Name += "Updated";
            //await DocumentDBRepository.UpdateItemAsync(user1.EmailId, user1);

            //await DocumentDBRepository.CreateItemAsync(group1);
            //await DocumentDBRepository.CreateItemAsync(group2);

            //await DocumentDBRepository.CreateItemAsync(team1);
            //await DocumentDBRepository.CreateItemAsync(team2);

            //await DocumentDBRepository.CreateItemAsync(tenant);

            await DocumentDBRepository.CreateItemAsync(campaignAnnouncement);

            // Update announcements.
            tenant.Announcements.Add(campaignAnnouncement.Id);
            await DocumentDBRepository.UpdateItemAsync(campaignAnnouncement.Id, campaignAnnouncement);

            var allGroups = await DocumentDBRepository.GetItemsAsync<Group>(u => u.Type == nameof(Group));
            var allTeam = await DocumentDBRepository.GetItemsAsync<Team>(u => u.Type == nameof(Team));
            var allTenants = await DocumentDBRepository.GetItemsAsync<Tenant>(u => u.Type == nameof(Tenant));
            var allAnnouncements = await DocumentDBRepository.GetItemsAsync<Campaign>(u => u.Type == nameof(Campaign));
            var myTenantAnnouncements = allAnnouncements.Where(a => a.TenantId == tenant.Id);
            return View();
        }
    }
}
