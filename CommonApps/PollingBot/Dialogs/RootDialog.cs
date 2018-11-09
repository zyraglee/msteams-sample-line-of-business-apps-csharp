using System;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using AdaptiveCards;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;
using System.IO;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Dialogs
{

    /// <summary>
    /// This Dialog enables the user to issue a set of commands against AAD
    /// to do things like list recent email, send an email, and identify the user
    /// This Dialog also makes use of the GetTokenDialog to help the user login
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        /// <summary>
        /// This is the name of the OAuth Connection Setting that is configured for this bot
        /// </summary>
        private static string ConnectionName = ConfigurationManager.AppSettings["ConnectionName"];
        private static string BaseUri = ConfigurationManager.AppSettings["BaseUri"];

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        /// <summary>
        /// Supports the commands recents, send, me, and signout against the Graph API
        /// </summary>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            GetWelcomeMessage();
            var message = Bot.Connector.Teams.ActivityExtensions.GetTextWithoutMentions(activity).ToLowerInvariant();


            if (message.Equals("me"))
            {
                // First ask Bot Service if it already has a token for this user
                var token = await context.GetUserTokenAsync(ConnectionName).ConfigureAwait(false);
                if (token != null)
                {
                    // use the token to do exciting things!
                    await ReplyProfileInfo(context, token);
                }
                else
                {
                    // If Bot Service does not have a token, send an OAuth card to sign in
                    await SendOAuthCardAsync(context, (Activity)context.Activity);
                }
            }
            else if (message.ToLowerInvariant().Equals("signout"))
            {
                // Sign the user out from AAD
                await Signout(context);
            }
            else {
                await context.PostAsync("You can type 'me' & 'signout' commands.");
                context.Wait(MessageReceivedAsync);
            }
        }

        internal static ThumbnailCard GetWelcomeMessage()
        {
            ThumbnailCard card = new ThumbnailCard
            {
                Title = "Welcome to Survey Application",
                Subtitle = "Your survey delivered here",
            };

            return card;
        }


        private async Task SendOAuthCardAsync(IDialogContext context, Activity activity)
        {
            await context.PostAsync($"To do this, you'll first need to sign in.");
            var SignInUrl = context.GetSignInUrlAsync(ConnectionName).Result;
            var reply = await context.Activity.CreateOAuthReplyAsync(ConnectionName, "Please sign in", "Sign In", true).ConfigureAwait(false);
            await context.PostAsync(reply);

            context.Wait(WaitForToken);
        }

        private async Task WaitForToken(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var tokenResponse = activity.ReadTokenResponseContent();
            if (tokenResponse != null)
            {
                // Use the token to do exciting things!
                await ReplyProfileInfo(context, tokenResponse);
            }
            else
            {
                // Get the Activity Message as well as activity.value in case of Auto closing of pop-up
                string input = activity.Type == ActivityTypes.Message ? Bot.Connector.Teams.ActivityExtensions.GetTextWithoutMentions(activity)
                                                                : ((dynamic)(activity.Value)).state.ToString();
                if (!string.IsNullOrEmpty(input))
                {
                    tokenResponse = await context.GetUserTokenAsync(ConnectionName, input.Trim());
                    if (tokenResponse != null)
                    {
                        try
                        {
                            await ReplyProfileInfo(context, tokenResponse);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }

                        context.Wait(MessageReceivedAsync);
                        return;
                    }
                }
                await context.PostAsync($"Hmm. Something went wrong. Let's try again.");
                await SendOAuthCardAsync(context, activity);
            }
        }

        private static async Task ReplyProfileInfo(IDialogContext context, TokenResponse tokenResponse)
        {
            var client = new SimpleGraphClient(tokenResponse.Token);

            var me = await client.GetMe();

            var photo = await client.GetProfilePhoto();
            var fileName = me.Id + "-ProflePhoto.png";
            var imagePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ProfilePhotos/" + fileName);

            using (var fileStream = File.Create(imagePath))
            {
                photo.Seek(0, SeekOrigin.Begin);
                photo.CopyTo(fileStream);
            }

            Attachment attachment = null;
            try
            {
                // Image attachment inline.
                attachment = new Attachment
                {
                    Name = fileName,
                    ContentType = "image/png",
                    ContentUrl = BaseUri + "/ProfilePhotos/" + fileName
                };

            }
            catch (Exception ex)
            {

                Console.WriteLine(ex);
            }

            var msg = context.MakeMessage();
            msg.Text = $"You are {me.DisplayName} and here is you profile photo.";
            msg.Attachments.Add(attachment);
            await context.PostAsync(msg);
        }


        /// <summary>
        /// Signs the user out from AAD
        /// </summary>
        public static async Task Signout(IDialogContext context)
        {
            await context.SignOutUserAsync(ConnectionName);
            await context.PostAsync($"You have been signed out.");
        }

    }
}