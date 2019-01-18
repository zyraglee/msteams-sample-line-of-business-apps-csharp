using CrossVertical.Announcement.Dialogs;
using CrossVertical.Announcement.Helpers;
using CrossVertical.Announcement.Models;
using CrossVertical.Announcement.Repository;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CrossVertical.Announcement.Controllers
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        [HttpPost]
        public async Task<HttpResponseMessage> Post([FromBody] Activity activity)
        {
            switch (activity.Type)
            {
                case ActivityTypes.Message:
                    await Conversation.SendAsync(activity, () => new RootDialog());
                    break;

                case ActivityTypes.Invoke:
                    return await HandleInvokeActivity(activity);

                case ActivityTypes.ConversationUpdate:
                    await HandleConversationUpdate(activity);
                    break;

                case ActivityTypes.MessageReaction:
                    await HandleReactions(activity);
                    break;
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// Handle an invoke activity.
        /// </summary>
        private async Task<HttpResponseMessage> HandleInvokeActivity(Activity activity)
        {
            var activityValue = activity.Value.ToString();
            switch (activityValue)
            {
                case "task/fetch":
                    // Handle fetching task module content
                    return await HandleTaskModuleFetchRequest(activity, activityValue);

                case "task/submit":
                    // Handle submission of task module info
                    // Run this on a task so that 
                    new Task(async () =>
                    {
                        var action = JsonConvert.DeserializeObject<TaskModule.BotFrameworkCardValue<ActionDetails>>(activityValue);
                        activity.Name = action.Data.ActionType;
                        await Conversation.SendAsync(activity, () => new RootDialog());
                    }).Start();

                    await Task.Delay(TimeSpan.FromSeconds(2));// Give it some time to start showing output.
                    break;
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        /// <summary>
        /// Handle request to fetch task module content.
        /// </summary>
        private async Task<HttpResponseMessage> HandleTaskModuleFetchRequest(Activity activity, string activityValue)
        {
            var action = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<ActionDetails>>(activityValue);

            var channelData = activity.GetChannelData<TeamsChannelData>();
            var tenantId = channelData.Tenant.Id;

            JObject taskEnvelope = new JObject();
            JObject taskObj = new JObject();
            JObject taskInfo = new JObject();

            // Default to common parameters for task module
            taskObj["type"] = "continue";
            taskObj["value"] = taskInfo;
            taskInfo["height"] = 900;
            taskInfo["width"] = 600;

            // Populate the task module content, based on the kind of dialog requested
            JObject card = null;
            switch (action.Data.Data.ActionType)
            {
                case Constants.CreateOrEditAnnouncement:
                    // Task module to create or edit an announcement
                    taskInfo["title"] = "Create New";
                    card = JObject.FromObject(await AdaptiveCardDesigns.GetCreateNewAnnouncementCard(tenantId));
                    break;

                case Constants.ShowMoreDetails:
                    // Task module to show the details of an announcement
                    taskInfo["title"] = "Details";
                    var showDetails = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<AnnouncementActionDetails>>(activityValue);
                    card = JObject.FromObject(await AdaptiveCardDesigns.GetPreviewAnnouncementCard(showDetails.Data.Data.Id));
                    break;

                case Constants.ShowEditAnnouncementTaskModule:
                    // Task module to edit an announcement
                    taskInfo["title"] = "Edit a message";
                    var editAnnouncement = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<AnnouncementActionDetails>>(activityValue);
                    var campaign = await Cache.Announcements.GetItemAsync(editAnnouncement.Data.Data.Id);
                    if (campaign == null || campaign.Status == Status.Sent)
                    {
                        // The message was already sent, so show an error message in a smaller task module
                        card = JObject.FromObject(AdaptiveCardDesigns.GetUpdateMessageCard("This announcement is already sent and not allowed to edit."));
                        taskInfo["height"] = 100;
                        taskInfo["width"] = 500;
                    }
                    else
                    {
                        card = JObject.FromObject(await AdaptiveCardDesigns.GetEditAnnouncementCard(editAnnouncement.Data.Data.Id, tenantId));
                    }
                    break;

                default:
                    break;
            }
            taskInfo["card"] = card;
            taskEnvelope["task"] = taskObj;

            return Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);
        }


        /// <summary>
        /// Handle conversationUpdate events, which signal membership changes.
        /// </summary>
        private static async Task HandleConversationUpdate(Activity message)
        {
            var channelData = message.GetChannelData<TeamsChannelData>();

            // Ensure that we have an entry for this tenant in the database
            var tenant = await RootDialog.CheckAndAddTenantDetails(channelData.Tenant.Id);

            // Treat 1:1 add/remove events as if they were add/remove of a team member
            if (channelData.EventType == null)
            {
                if (message.MembersAdded != null)
                    channelData.EventType = "teamMemberAdded";
                if(message.MembersRemoved != null)
                    channelData.EventType = "teamMemberRemoved";
            }

            switch (channelData.EventType)
            {
                case "teamMemberAdded":
                    // Team member was added (user or bot)
                    if (message.MembersAdded.Any(m => m.Id.Contains(message.Recipient.Id)))
                    {
                        // Bot was added to a team: send welcome message
                        message.Text = Constants.ShowWelcomeScreen;
                        await Conversation.SendAsync(message, () => new RootDialog());
                        await AddTeamDetails(message, channelData, tenant);
                    }
                    else
                    {
                        // Member was added to a team: update the team member count
                        await UpdateTeamCount(message, channelData, tenant);
                    }
                    break;

                case "teamMemberRemoved":
                    // Add team & channel details 
                    if (message.MembersRemoved.Any(m => m.Id.Contains(message.Recipient.Id)))
                    {
                        // Bot was removed from a team: remove entry for the team in the database
                        await RemoveTeamDetails(channelData, tenant);
                    }
                    else
                    {
                        // Member was removed from a team: update the team member  count
                        await UpdateTeamCount(message, channelData, tenant);
                    }
                    break;

                // Update the team and channel info in the database when the team is rename or when channel are added/removed/renamed

                case "teamRenamed":
                    await RenameTeam(channelData, tenant);
                    break;

                case "channelCreated":
                    await AddNewChannelDetails(channelData, tenant);
                    break;

                case "channelRenamed":
                    await RenameChannel(channelData, tenant);
                    break;

                case "channelDeleted":
                    await DeleteChannel(channelData, tenant);
                    break;
            }
        }

        /// <summary>
        /// Handle messageReaction events, which indicate user liked/unliked a message sent by the bot.
        /// </summary>
        private static async Task HandleReactions(Activity message)
        {
            if (message.ReactionsAdded != null || message.ReactionsRemoved != null)
            {
                // Determine if likes were net added or removed
                var reactionToAdd = message.ReactionsAdded != null ? 1 : -1;

                var channelData = message.GetChannelData<TeamsChannelData>();
                var replyToId = message.ReplyToId;
                if (channelData.Team != null)
                    replyToId = message.Conversation.Id;

                // Look for the announcement that was liked/unliked, and update the reaction count on that announcement
                var tenant = await Cache.Tenants.GetItemAsync(channelData.Tenant.Id);
                bool messageFound = false;
                foreach (var announcementId in tenant.Announcements)
                {
                    var announcement = await Cache.Announcements.GetItemAsync(announcementId);
                    if (announcement?.Recipients == null)
                        continue;

                    if (channelData.Team == null)
                    {
                        foreach (var group in announcement.Recipients.Groups)
                        {
                            var user = group.Users.FirstOrDefault(u => u.MessageId == replyToId);
                            if (user != null)
                            {
                                messageFound = true;
                                user.LikeCount += reactionToAdd;
                                if (user.LikeCount < 0)
                                    user.LikeCount = 0;
                            }
                        }
                    }

                    if (!messageFound && channelData.Team != null)
                    {
                        foreach (var channel in announcement.Recipients.Channels)
                        {
                            if (channel.Channel.MessageId == replyToId)
                            {
                                channel.Channel.LikeCount += reactionToAdd;
                                messageFound = true;
                                if (channel.Channel.LikeCount < 0)
                                    channel.Channel.LikeCount = 0;
                                break;

                            }
                        }
                    }

                    if (messageFound)
                    {
                        await Cache.Announcements.AddOrUpdateItemAsync(announcement.Id, announcement);
                        break;
                    }
                }
            }
        }

        private static async Task DeleteChannel(TeamsChannelData channelData, Tenant tenant)
        {
            var team = await GetTeam(channelData, tenant);
            if (team != null)
            {
                var channel = team.Channels.FirstOrDefault(c => c.Id == channelData.Channel.Id);
                if (channel != null)
                {
                    team.Channels.Remove(channel);
                    await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);
                }
            }
        }

        private static async Task RenameChannel(TeamsChannelData channelData, Tenant tenant)
        {
            var team = await GetTeam(channelData, tenant);
            if (team != null)
            {
                var channel = team.Channels.FirstOrDefault(c => c.Id == channelData.Channel.Id);
                if (channel != null)
                {
                    channel.Name = channelData.Channel.Name;
                    await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);
                }
            }
        }

        private static async Task AddNewChannelDetails(TeamsChannelData channelData, Tenant tenant)
        {
            var team = await GetTeam(channelData, tenant);
            if (team != null)
            {
                team.Channels.Add(new Channel() { Id = channelData.Channel.Id, Name = channelData.Channel.Name });
                await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);
            }

        }

        private static async Task<Team> GetTeam(TeamsChannelData channelData, Tenant tenant)
        {
            var teamId = channelData.Team.Id;
            if (tenant.Teams.Contains(channelData.Team.Id))
            {
                return await Cache.Teams.GetItemAsync(channelData.Team.Id);
            }
            return null;
        }

        private static async Task RenameTeam(TeamsChannelData channelData, Tenant tenant)
        {
            var team = await GetTeam(channelData, tenant);
            if (team != null)
            {
                team.Name = channelData.Team.Name;
                await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);
            }
        }

        private static async Task RemoveTeamDetails(TeamsChannelData channelData, Tenant tenant)
        {
            var team = await GetTeam(channelData, tenant);
            if (team != null)
            {
                await Cache.Teams.DeleteItemAsync(team.Id);
            }
            tenant.Teams.Remove(channelData.Team.Id);
            await Cache.Tenants.AddOrUpdateItemAsync(tenant.Id, tenant);

        }

        private static async Task AddTeamDetails(Activity message, TeamsChannelData channelData, Tenant tenant)
        {
            if (channelData.Team != null && !tenant.Teams.Contains(channelData.Team.Id))
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                var members = await connector.Conversations.GetConversationMembersAsync(channelData.Team.Id);
                int count = members.Count;
                var team = new Team
                {
                    Id = channelData.Team.Id,
                    Name = channelData.Team.Name,
                    MemberCount = count
                };

                // Add all teams and channels
                ConversationList channels = connector.GetTeamsConnectorClient().Teams.FetchChannelList(message.GetChannelData<TeamsChannelData>().Team.Id);
                foreach (var channel in channels.Conversations)
                {
                    team.Channels.Add(new Channel() { Id = channel.Id, Name = channel.Name ?? "General" });
                }
                await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);

                tenant.Teams.Add(channelData.Team.Id);

                await Cache.Tenants.AddOrUpdateItemAsync(tenant.Id, tenant);

                await SendWelcomeMessageToAllMembers(tenant, message,  channelData, members.AsTeamsChannelAccounts());
            }

            await RootDialog.CheckAndAddUserDetails(message, channelData);
        }

        private static async Task SendWelcomeMessageToAllMembers(Tenant tenant, Activity message, TeamsChannelData channelData, IEnumerable<TeamsChannelAccount> members)
        {
            var card = AdaptiveCardDesigns.GetWelcomeScreen(false);
            foreach (var member in members)
            {
                var userDetails = await Cache.Users.GetItemAsync(member.UserPrincipalName);
                if (userDetails == null)
                {
                    userDetails = new User()
                    {
                        BotConversationId = member.Id,
                        Id = member.UserPrincipalName,
                        Name = member.Name ?? member.GivenName
                    };
                    await Cache.Users.AddOrUpdateItemAsync(userDetails.Id, userDetails);

                    tenant.Users.Add(userDetails.Id);
                    await Cache.Tenants.AddOrUpdateItemAsync(tenant.Id, tenant);

                    var result = await ProactiveMessageHelper.SendNotification(message.ServiceUrl, channelData.Tenant.Id, member.Id, null, card);
                    if (!result.IsSuccessful)
                    {
                        ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                        var reply = message.CreateReply();
                        reply.Text = $"Failed to send welcome message to {member.UserPrincipalName}. Error: {result.FailureMessage}";
                        connector.Conversations.ReplyToActivity(reply);
                    }
                }
            }
        }

        private static async Task UpdateTeamCount(Activity message, TeamsChannelData channelData, Tenant tenant)
        {
            if (tenant.Teams.Contains(channelData.Team.Id))
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                var members = await connector.Conversations.GetConversationMembersAsync(channelData.Team.Id);
                int count = members.Count;

                var team = await Cache.Teams.GetItemAsync(channelData.Team.Id);
                team.MemberCount = count;
                await Cache.Teams.AddOrUpdateItemAsync(team.Id, team);
            }

        }
    }
}
