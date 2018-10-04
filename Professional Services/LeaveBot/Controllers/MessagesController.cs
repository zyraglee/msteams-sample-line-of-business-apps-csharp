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
            }
            else if (activity.Type == ActivityTypes.Invoke)
            {
                if (activity.IsO365ConnectorCardActionQuery())
                {
                    activity.Text = "ConnectorAction";
                    await Conversation.SendAsync(activity, () => new RootDialog());
                    return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
                }
                else if (activity.Name == "signin/verifyState")
                {
                    await Conversation.SendAsync(activity, () => new RootDialog());
                }
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            return new HttpResponseMessage(System.Net.HttpStatusCode.Accepted);
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
