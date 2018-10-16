using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Teams.Samples.HelloWorld.Web.Dialogs;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Helpers;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
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
                var action = Newtonsoft.Json.JsonConvert.DeserializeObject<TaskModule.BotFrameworkCardValue<EditLeaveDetails>>(activityValue);

                var leaveDetails = await DocumentDBRepository.GetItemAsync<LeaveDetails>(action.Data.LeaveId);

                // TODO: Convert this to helpers once available.
                JObject taskEnvelope = new JObject();

                JObject taskObj = new JObject();
                JObject taskInfo = new JObject();

                taskObj["type"] = "continue";
                taskObj["value"] = taskInfo;

                taskInfo["card"] = JObject.FromObject(EchoBot.LeaveRequest(leaveDetails));
                taskInfo["title"] = "Edit Leave";

                taskInfo["height"] = 330;
                taskInfo["width"] = 550;

                taskEnvelope["task"] = taskObj;

                return Request.CreateResponse(HttpStatusCode.OK, taskEnvelope);

            }
            else if (activity.Name == "task/submit")
            {
                activity.Name = Constants.EditLeave;
                await Conversation.SendAsync(activity, () => new RootDialog());
                //ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                //Activity reply = activity.CreateReply("Received = " + activity.Value.ToString());
                //connector.Conversations.ReplyToActivity(reply);
            }
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private async Task<Activity> HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                for (int i = 0; i < message.MembersAdded.Count; i++)
                {
                    if (message.MembersAdded[i].Id == message.Recipient.Id)
                    {
                        // Bot is added. Let's send welcome message.
                        message.Text = "hi";
                        await Conversation.SendAsync(message, () => new RootDialog());
                        break;
                    }
                    else
                    {
                        //message.Text = "hi";
                        //message.From.Id = message.MembersAdded[i].Id;
                        //var channleData = message.GetChannelData<TeamsChannelData>();// message.ChannelData
                        //channleData.Channel = null;
                        //channleData.Team = null;
                        //message.ChannelData = channleData;
                        //message.From.Name = message.MembersAdded[i].Name;
                        //await Conversation.SendAsync(message, () => new RootDialog());
                        // break;
                        // Send sign in card.

                        // // testc this

                        //try
                        //{
                        //    var connector = new ConnectorClient(new Uri(message.ServiceUrl));
                        //    var channelData = new Dictionary<string, string>();

                        //    var existingData = message.GetChannelData<TeamsChannelData>();

                        //    // Create a new reply.
                        //    IMessageActivity newMessage = Activity.CreateMessageActivity();
                        //    newMessage.Type = ActivityTypes.Message;

                        //    var card = EchoBot.WelcomeLeaveCard(message.MembersAdded[i].Name, false);
                        //    // var card = GetAdaptiveCard();
                        //    // newMessage.Text = 
                        //    newMessage.Attachments.Add(card);

                        //    ConversationParameters conversationParams = new ConversationParameters(
                        //        isGroup: false,
                        //        bot: message.Recipient,
                        //        members: new ChannelAccount[] { new ChannelAccount(message.MembersAdded[i].Id) },
                        //        activity: (Activity)newMessage,
                        //        channelData: new TeamsChannelData()
                        //        {
                        //            Tenant = new TenantInfo() { Id = existingData.Tenant.Id },
                        //            Notification = new NotificationInfo() { Alert = true }
                        //        });

                        //    await connector.Conversations.CreateConversationAsync(conversationParams);
                        //}
                        //catch (Exception ex)
                        //{

                        //    Console.WriteLine(ex);
                        //}
                    }

                    
                }

            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            return null;
        }
    }
}
