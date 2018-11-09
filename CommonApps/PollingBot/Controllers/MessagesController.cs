using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
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
                await Conversation.SendAsync(activity, () => new EchoBot());
                return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
        }

        private async Task<string> GetUserEmailId(Activity activity)
        {
            // Fetch the members in the current conversation
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var members = await connector.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
            return members.Where(m => m.Id == activity.From.Id).First().AsTeamsChannelAccount().Email;
            
        }

        private async Task<string> GetUserName(Activity activity)
        {
            // Fetch the members in the current conversation
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var members = await connector.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
            return members.Where(m => m.Id == activity.From.Id).First().AsTeamsChannelAccount().Name;

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
                        var connectorClient = new ConnectorClient(new Uri(message.ServiceUrl));
                        var user = new UserDetails();
                        user.EmaildId = await GetUserEmailId(message);
                        user.UserId = message.From.Id;
                        user.UserName = await GetUserName(message);
                        if(user.UserName!=null)
                        {
                            user.UserName = user.UserName.Split(' ').FirstOrDefault();
                        }
                        user.Type = Helper.Constants.NewUser;
                        var NewUserRecord = await DocumentDBRepository.CreateItemAsync(user);
                        ThumbnailCard card = EchoBot.GetWelcomeMessage();
                        //ThumbnailCard card = EchoBot.GetHelpMessage();
                        var reply = message.CreateReply();
                        reply.TextFormat = TextFormatTypes.Xml;
                        reply.Attachments.Add(card.ToAttachment());
                        await connectorClient.Conversations.ReplyToActivityAsync(reply);
                        break;
                    }
                    else
                    {
                        try
                        {
                            var userId = message.MembersAdded[i].Id;
                            var channelData = message.GetChannelData<TeamsChannelData>();
                            var connectorClient = new ConnectorClient(new Uri(message.ServiceUrl));
                            var user = new UserDetails();
                            user.EmaildId = await GetUserEmailId(message);
                            user.UserId = message.From.Id;
                            var parameters = new ConversationParameters
                            {
                                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                                ChannelData = new TeamsChannelData
                                {
                                    Tenant = channelData.Tenant,
                                    Notification = new NotificationInfo() { Alert = true }
                                }
                            };

                            var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);

                            var replyMessage = Activity.CreateMessageActivity();
                            replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                            replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                            var name = message.MembersAdded[i].Name;
                            if (name != null)
                            {
                                name = name.Split(' ').First();
                            }
                            ThumbnailCard card = EchoBot.GetWelcomeMessage();
                            replyMessage.Attachments.Add(card.ToAttachment());

                            await connectorClient.Conversations.SendToConversationAsync((Activity)replyMessage);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
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
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }


    }
}
