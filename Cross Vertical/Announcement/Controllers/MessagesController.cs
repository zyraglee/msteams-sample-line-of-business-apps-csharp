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
            if (activity != null && activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new RootDialog());
                return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                if (activity.Name == "signin/verifyState")
                {
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }
                else
                {
                    return await HandleInvokeMessages(activity);
                }
            }
            else
            {
                await HandleSystemMessage(activity);

            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }


        private async Task<HttpResponseMessage> HandleInvokeMessages(Activity activity)
        {
            var activityValue = activity.Value.ToString();
            if (activity.Name == "task/fetch")
            {
                return await HandleTaskModuleFetchRequest(activity, activityValue);
            }
            else if (activity.Name == "task/submit")
            {
                var action = JsonConvert.DeserializeObject<TaskModule.BotFrameworkCardValue<ActionDetails>>(activityValue);

                activity.Name = action.Data.ActionType;
                await Conversation.SendAsync(activity, () => new RootDialog());
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private async Task<HttpResponseMessage> HandleTaskModuleFetchRequest(Activity activity, string activityValue)
        {
            var action = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<ActionDetails>>(activityValue);

            // TODO: Convert this to helpers once available.
            JObject taskEnvelope = new JObject();

            JObject taskObj = new JObject();
            JObject taskInfo = new JObject();

            taskObj["type"] = "continue";
            taskObj["value"] = taskInfo;
            taskInfo["height"] = 900;
            taskInfo["width"] = 600;

            JObject card = null;
            // Fetch Tenant Id and pass it.
            var channelData = activity.GetChannelData<TeamsChannelData>();
            switch (action.Data.Data.ActionType)
            {
                case Constants.CreateOrEditAnnouncement:
                    taskInfo["title"] = "New Announcement";
                    card = JObject.FromObject(await AdaptiveCardDesigns.GetCreateNewAnnouncementCard(channelData.Tenant.Id));
                    break;
                case Constants.ShowMoreDetails:
                    taskInfo["title"] = "Announcement";
                    var showDetails = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<AnnouncementActionDetails>>(activityValue);
                    card = JObject.FromObject(await AdaptiveCardDesigns.GetPreviewAnnouncementCard(showDetails.Data.Data.Id));
                    taskInfo["height"] = 900;
                    taskInfo["width"] = 600;

                    break;
                case Constants.ShowEditAnnouncementTaskModule:
                    taskInfo["title"] = "Edit Announcement";
                    var editAnnouncement = JsonConvert.DeserializeObject<TaskModule.TaskModuleActionData<AnnouncementActionDetails>>(activityValue);
                    card = JObject.FromObject(await AdaptiveCardDesigns.GetEditAnnouncementCard(editAnnouncement.Data.Data.Id, channelData.Tenant.Id));
                    break;
                default:
                    break;
            }
            taskInfo["card"] = card;
            taskEnvelope["task"] = taskObj;

            return Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                await HandleConversationUpdate(message);
            }
            else if (message.Type == ActivityTypes.MessageReaction)
            {
                await HandleReactions(message);
                // Handle knowing tha the user is typing
            }
            return null;
        }

        private static async Task HandleConversationUpdate(Activity message)
        {
            var channelData = message.GetChannelData<TeamsChannelData>();
            var tenant = await RootDialog.CheckAndAddTenantDetails(channelData);
            switch (channelData.EventType)
            {
                case "teamMemberAdded":
                    // Add team & channel details 
                    if (message.MembersAdded.Any(m => m.Id.Contains(message.Recipient.Id)))
                    {
                        // Bot is added. Let's send welcome message.
                        message.Text = Constants.ShowWelcomeScreen;
                        await Conversation.SendAsync(message, () => new RootDialog());
                        await AddTeamDetails(message, channelData, tenant);
                    }
                    break;
                case "teamMemberRemoved":
                    // Add team & channel details 
                    if (message.MembersRemoved.Any(m => m.Id.Contains(message.Recipient.Id)))
                    {
                        await RemoveTeamDetails(channelData, tenant);
                    }
                    break;
                case "teamRenamed":
                    // Rename team & channel details 
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
                default:
                    break;
            }
        }

        private static async Task HandleReactions(Activity message)
        {
            if (message.ReactionsAdded != null || message.ReactionsRemoved != null)
            {
                var reactionToAdd = message.ReactionsAdded != null ? 1 : -1;
                var channelData = message.GetChannelData<TeamsChannelData>();
                var replyToId = message.ReplyToId;
                if (channelData.Team != null)
                    replyToId = message.Conversation.Id;
                var tenant = await Cache.Tenants.GetItemAsync(channelData.Tenant.Id);
                bool messageFound = false;
                foreach (var announcementId in tenant.Announcements)
                {
                    var announcement = await Cache.Announcements.GetItemAsync(announcementId);
                    if (announcement == null || announcement.Recipients == null)
                        continue;

                    if (channelData.Team == null)
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
                    if (!messageFound && channelData.Team != null)
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
            if (!tenant.Teams.Contains(channelData.Team.Id))
            {
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));
                var team = new Team
                {
                    Id = channelData.Team.Id,
                    Name = channelData.Team.Name
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
            }
            await RootDialog.CheckAndAddUserDetails(message, channelData);
        }
    }
}
