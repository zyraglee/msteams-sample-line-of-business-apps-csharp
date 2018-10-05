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
//using AdaptiveCards;
using System.Linq;
using System.Text;
using System.Threading;
//using Microsoft.Teams.Samples.HelloWorld.Web.Helpers;
using System.IO;
//using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
//using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using Microsoft.Bot.Connector.Teams.Models;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using Microsoft.Teams.Samples.HelloWorld.Web.Helpers;
using Microsoft.Graph;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;

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
        private const string ProfileKey = "profile";
        private const string EmailKey = "emailId";

        /// <summary>
        /// This is the name of the OAuth Connection Setting that is configured for this bot
        /// </summary>
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

            string message = string.Empty;
            if (activity.Text != null)
                message = Bot.Connector.Teams.ActivityExtensions.GetTextWithoutMentions(activity).ToLowerInvariant();
            string userEmailId = string.Empty;
            if (context.ConversationData.ContainsKey(EmailKey))
            {
                userEmailId = context.ConversationData.GetValue<string>(EmailKey);
            }
            else
            {
                // Fetch from roaster
                userEmailId = await GetUserEmailId(activity);
                context.ConversationData.SetValue<string>(EmailKey, userEmailId);
            }

            Employee employee;
            if (context.ConversationData.ContainsKey(ProfileKey))
            {
                employee = context.ConversationData.GetValue<Employee>(ProfileKey);
            }
            else
            {
                employee = await DocumentDBRepository.GetItemAsync<Employee>(userEmailId);
                if (employee != null)
                    context.ConversationData.SetValue<Employee>(ProfileKey, employee);
            }
            if (employee == null)
            {
                // If Bot Service does not have a token, send an OAuth card to sign in.
                await SendOAuthCardAsync(context, (Activity)context.Activity);
            }
            else if ((string.IsNullOrEmpty(employee.ManagerEmailId) && string.IsNullOrEmpty(employee.DemoManagerEmailId))
                && (!IsValidActionWithoutManager(activity.Value)))
            {
                await SendSetManagerCard(context);
            }
            else if (activity.Value != null)
            {
                await HandleActions(context, activity, employee);
            }
            else
            {
                if (message.ToLowerInvariant().Contains("set manager"))
                {
                    await SendSetManagerCard(context);
                }
                else if (message.ToLowerInvariant().Equals("signout"))
                {
                    // Sign the user out from AAD
                    await Signout(userEmailId, context);
                }
                else
                {
                    var reply = activity.CreateReply();
                    bool isManager = await IsManager(employee);

                    reply.Attachments.Add(EchoBot.WelcomeLeaveCard(employee.DisplayName, isManager));

                    await context.PostAsync(reply);
                }
            }
        }

        public static async Task<bool> IsManager(Employee employee)
        {
            var allEmployees = await DocumentDBRepository.GetItemsAsync<Employee>(l => l.Type == Employee.TYPE);
            return allEmployees.Any(s => s.ManagerEmailId == employee.EmailId);
        }

        private async Task HandleActions(IDialogContext context, Activity activity, Employee employee)
        {
            string type = string.Empty;
            if (activity.Name == Constants.EditLeave) // Edit request
            {
                var editRequest = JsonConvert.DeserializeObject<EditRequest>(activity.Value.ToString());
                type = editRequest.data.Type;
            }
            else
            {
                var details = JsonConvert.DeserializeObject<InputDetails>(activity.Value.ToString());
                type = details.Type;
            }
            var reply = activity.CreateReply();

            switch (type)
            {
                case Constants.ApplyForOtherLeave:
                case Constants.ApplyForPersonalLeave:
                case Constants.ApplyForSickLeave:
                case Constants.ApplyForVacation:


                    await ApplyForVacation(context, activity, employee, type);

                    break;

                case Constants.RejectLeave:
                case Constants.ApproveLeave:

                    await HandleLeaveApprovalOrRejection(context, activity, type);
                    //await SetEmployeeManager(context, activity, employee);
                    break;
                case Constants.SetManager:
                    await SetEmployeeManager(context, activity, employee);
                    break;

                case Constants.ShowPendingApprovals:
                    await ShowPendingApprovals(context, activity, employee);
                    break;
                case Constants.LeaveRequest:
                    try
                    {
                        reply.Attachments.Add(EchoBot.LeaveRequest());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);

                    }

                    await context.PostAsync(reply);
                    break;
                case Constants.LeaveBalance:
                    reply.Attachments.Add(EchoBot.ViewLeaveBalance(employee));
                    await context.PostAsync(reply);
                    break;
                case Constants.Holidays:
                    reply.Attachments.Add(EchoBot.PublicHolidays());
                    await context.PostAsync(reply);
                    break;
                default:
                    reply = activity.CreateReply("It will redirect to the tab");
                    await context.PostAsync(reply);
                    break;
            }
        }

        private async Task ShowPendingApprovals(IDialogContext context, Activity activity, Employee employee)
        {
            var pendingLeaves = await DocumentDBRepository.GetItemsAsync<LeaveDetails>(l => l.Type == LeaveDetails.TYPE);
            pendingLeaves = pendingLeaves.Where(l => l.ManagerEmailId == employee.EmailId && l.Status == LeaveStatus.Pending);
            if (pendingLeaves.Count() == 0)
            {
                var reply = activity.CreateReply();
                reply.Text = "No pending leaves for approval.";
                await context.PostAsync(reply);
            }
            else
            {
                var reply = activity.CreateReply();
                reply.Text = "Here are all the leaves pending for approval:";
                foreach (var leave in pendingLeaves)
                {
                    var attachment = EchoBot.ManagerViewCard(employee, leave);
                    reply.Attachments.Add(attachment);
                }
                await context.PostAsync(reply);
            }
        }

        private bool IsValidActionWithoutManager(object activityValue)
        {
            if (activityValue == null)
                return true;

            return (activityValue.ToString().Contains(Constants.SetManager)
                || activityValue.ToString().Contains(Constants.ApproveLeave)
                || activityValue.ToString().Contains(Constants.RejectLeave)
                || activityValue.ToString().Contains(Constants.LeaveBalance));
        }

        private static async Task HandleLeaveApprovalOrRejection(IDialogContext context, Activity activity, string type)
        {
            var managerResponse = JsonConvert.DeserializeObject<ManagerResponse>(activity.Value.ToString());
            var leaveDetails = await DocumentDBRepository.GetItemAsync<LeaveDetails>(managerResponse.LeaveId);
            var appliedByEmployee = await DocumentDBRepository.GetItemAsync<Employee>(leaveDetails.AppliedByEmailId);

            // Check the leave type and reduce in DB.
            leaveDetails.Status = type == Constants.ApproveLeave ? LeaveStatus.Approved : LeaveStatus.Rejected;
            leaveDetails.ManagerComment = managerResponse.ManagerComment;
            await DocumentDBRepository.UpdateItemAsync(leaveDetails.LeaveId, leaveDetails);

            var conunt = EchoBot.GetDayCount(leaveDetails);
            var msg = $"Your {conunt} days leave has been {leaveDetails.Status.ToString()}. Manager Comments: {leaveDetails.ManagerComment}";

            var employeeView = EchoBot.EmployeeViewCard(appliedByEmployee, leaveDetails);

            MessageIds managerMessageIds = GetMessageId(context, leaveDetails.LeaveId);

            await SendNotification(context, appliedByEmployee.UserUniqueId, null, employeeView, managerMessageIds.Employee);
        }

        private async Task SetEmployeeManager(IDialogContext context, Activity activity, Employee employee)
        {
            var details = JsonConvert.DeserializeObject<SetManagerDetails>(activity.Value.ToString());
            // reply.Attachments.Add(EchoBot.LeaveRequest());

            employee.ManagerEmailId = details.txtManager.ToLower();
            await UpdateEmployeeInDB(context, employee);
        }

        private static async Task ApplyForVacation(IDialogContext context, Activity activity, Employee employee, string leaveCategory)
        {
            if (string.IsNullOrEmpty(employee.ManagerEmailId) && string.IsNullOrEmpty(employee.DemoManagerEmailId))
            {
                var reply = activity.CreateReply();
                reply.Text = "Please set your manager and try again.";
                reply.Attachments.Add(EchoBot.SetManagerCard());
                await context.PostAsync(reply);
                return;
            }
            var managerId = await GetManagerId(employee);
            if (managerId == null)
            {
                var reply = activity.CreateReply();
                reply.Text = "Unable to fetch your manager details. Please make sure that your manager has installed the Leave App.";
                await context.PostAsync(reply);
            }
            else
            {
                VacationDetails vacationDetails = null;
                if (activity.Name == Constants.EditLeave) // Edit request
                {
                    var editRequest = JsonConvert.DeserializeObject<EditRequest>(activity.Value.ToString());
                    vacationDetails = editRequest.data;
                }
                else
                    vacationDetails = JsonConvert.DeserializeObject<VacationDetails>(activity.Value.ToString());

                LeaveDetails leaveDetails;
                if (!string.IsNullOrEmpty(vacationDetails.LeaveId))
                {
                    // Edit request
                    leaveDetails = await DocumentDBRepository.GetItemAsync<LeaveDetails>(vacationDetails.LeaveId);
                }
                else
                {
                    leaveDetails = new LeaveDetails();
                    leaveDetails.LeaveId = Guid.NewGuid().ToString();
                }
                leaveDetails.AppliedByEmailId = employee.EmailId;
                leaveDetails.EmployeeComment = vacationDetails.LeaveReason;
                leaveDetails.StartDate = new LeaveDate()
                {
                    Date = DateTime.Parse(vacationDetails.FromDate),
                    Type = (DayType)Enum.Parse(typeof(DayType), vacationDetails.FromDuration)
                };
                leaveDetails.EndDate = new LeaveDate()
                {
                    Date = DateTime.Parse(vacationDetails.ToDate),
                    Type = (DayType)Enum.Parse(typeof(DayType), vacationDetails.ToDuration)
                };

                leaveDetails.LeaveType = (LeaveType)Enum.Parse(typeof(LeaveType), vacationDetails.LeaveType);
                leaveDetails.Status = LeaveStatus.Pending;
                leaveDetails.ManagerEmailId = employee.ManagerEmailId;// Added for easy reporting.

                switch (leaveCategory)
                {
                    case Constants.ApplyForPersonalLeave:
                        leaveDetails.LeaveCategory = LeaveCategory.Personal;
                        break;
                    case Constants.ApplyForSickLeave:
                        leaveDetails.LeaveCategory = LeaveCategory.Sickness;
                        break;
                    case Constants.ApplyForVacation:
                        leaveDetails.LeaveCategory = LeaveCategory.Vacation;
                        break;
                    case Constants.ApplyForOtherLeave:
                    default:
                        leaveDetails.LeaveCategory = LeaveCategory.Other;
                        break;
                }

                if (!string.IsNullOrEmpty(vacationDetails.LeaveId))
                {
                    // Edit request
                    await DocumentDBRepository.UpdateItemAsync<LeaveDetails>(leaveDetails.LeaveId, leaveDetails);
                }
                else
                {
                    await DocumentDBRepository.CreateItemAsync<LeaveDetails>(leaveDetails);
                }

                var attachment = EchoBot.ManagerViewCard(employee, leaveDetails);

                MessageIds managerMessageIds = GetMessageId(context, leaveDetails.LeaveId);

                // Manger Updates.
                var conversationId = await SendNotification(context, managerId, null, attachment, managerMessageIds.Manager);
                if(!String.IsNullOrEmpty(conversationId))
                {
                    managerMessageIds.Manager = conversationId;
                    context.ConversationData.SetValue(leaveDetails.LeaveId, managerMessageIds);
                }

                if (!String.IsNullOrEmpty(conversationId))
                {

                    ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                    MessageIds messageIds = GetMessageId(context, leaveDetails.LeaveId);
                    var employeeCardReply = activity.CreateReply();
                    var employeeView = EchoBot.EmployeeViewCard(employee, leaveDetails);
                    employeeCardReply.Attachments.Add(employeeView);

                    if (!string.IsNullOrEmpty(messageIds.Employee))
                    {
                        // Update existing item.
                        await connector.Conversations.UpdateActivityAsync(employeeCardReply.Conversation.Id, messageIds.Employee, employeeCardReply);
                        //var reply = activity.CreateReply();
                        //reply.Text = "We have also notified manager with the updates.";
                        //await context.PostAsync(reply);
                    }
                    else
                    {
                        var reply = activity.CreateReply();
                        reply.Text = "Your leave request has been successfully submitted to your manager! Please review your details below";
                        await context.PostAsync(reply);

                        var msgToUpdate = await connector.Conversations.ReplyToActivityAsync(employeeCardReply);
                        messageIds.Employee = msgToUpdate.Id;
                        context.ConversationData.SetValue<MessageIds>(leaveDetails.LeaveId, messageIds);
                    }

                }
                else
                {
                    var reply = activity.CreateReply();
                    reply.Text = "Failed to send notification to your manger. Please try again later.";
                    await context.PostAsync(reply);
                }

            }
        }

        private static MessageIds GetMessageId(IDialogContext context, string leaveId)
        {
            MessageIds messageIds = new MessageIds();
            if (context.ConversationData.ContainsKey(leaveId))
            {
                messageIds = context.ConversationData.GetValue<MessageIds>(leaveId);
            }
            return messageIds;
        }

        private static async Task SendSetManagerCard(IDialogContext context)
        {
            //Ask for manager details.
            var card = EchoBot.SetManagerCard(); // WelcomeLeaveCard(employee.Name.Split(' ').First());

            var msg = context.MakeMessage();
            msg.Text = "Please set your manager so that we can send leaves for approval.";
            msg.Attachments.Add(card);
            await context.PostAsync(msg);
        }

        private async Task UpdateEmployeeInDB(IDialogContext context, Employee employee)
        {
            await DocumentDBRepository.UpdateItemAsync(employee.EmailId, employee);
            context.ConversationData.SetValue<Employee>(ProfileKey, employee);
        }


        private static async Task<string> GetManagerId(Employee employee)
        {
            var managerId = employee.ManagerEmailId ?? employee.DemoManagerEmailId;

            var manager = await DocumentDBRepository.GetItemAsync<Employee>(managerId);
            if (manager != null)
                return manager.UserUniqueId;
            else return null;
        }

        private async Task<string> GetUserEmailId(Activity activity)
        {
            // Fetch the members in the current conversation
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
            var members = await connector.Conversations.GetConversationMembersAsync(activity.Conversation.Id);
            return members.Where(m => m.Id == activity.From.Id).First().AsTeamsChannelAccount().Email;
        }

        private async Task SendOAuthCardAsync(IDialogContext context, Activity activity)
        {
            var reply = await context.Activity.CreateOAuthReplyAsync(ApplicationSettings.ConnectionName, "In order to use Leave Bot we need your basic deatils, Please sign in", "Sign In", true).ConfigureAwait(false);
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
                await AddUserToDatabase(context, tokenResponse);
            }
            else
            {
                // Get the Activity Message as well as activity.value in case of Auto closing of pop-up
                string input = activity.Type == ActivityTypes.Message ? Bot.Connector.Teams.ActivityExtensions.GetTextWithoutMentions(activity)
                                                                : ((dynamic)(activity.Value)).state.ToString();
                if (!string.IsNullOrEmpty(input))
                {
                    tokenResponse = await context.GetUserTokenAsync(ApplicationSettings.ConnectionName, input.Trim());
                    if (tokenResponse != null)
                    {
                        try
                        {
                            await AddUserToDatabase(context, tokenResponse);
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

        private static async Task AddUserToDatabase(IDialogContext context, TokenResponse tokenResponse)
        {
            var client = new SimpleGraphClient(tokenResponse.Token);

            User me = null;
            //User manager = null;
            var profilePhotoUrl = string.Empty;
            try
            {
                me = await client.GetMe();
                // manager = await client.GetManager();
                var photo = await client.GetProfilePhoto();
                var fileName = me.Id + "-ProflePhoto.png";
                string imagePath = System.Web.Hosting.HostingEnvironment.MapPath("~/ProfilePhotos/");
                if (!System.IO.Directory.Exists(imagePath))
                    System.IO.Directory.CreateDirectory(imagePath);
                imagePath += fileName;

                using (var fileStream = System.IO.File.Create(imagePath))
                {
                    photo.Seek(0, SeekOrigin.Begin);
                    photo.CopyTo(fileStream);
                }
                profilePhotoUrl = ApplicationSettings.BaseUrl + "/ProfilePhotos/" + fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                profilePhotoUrl = null;
            }

            ConnectorClient connector = new ConnectorClient(new Uri(context.Activity.ServiceUrl));
            var channelData = context.Activity.GetChannelData<TeamsChannelData>();
            var employee = new Employee()
            {
                Name = me.DisplayName,
                EmailId = me.UserPrincipalName,
                UserUniqueId = context.Activity.From.Id, // For proactive messages
                TenantId = channelData.Tenant.Id,
                DemoManagerEmailId = string.Empty,
                LeaveBalance = new LeaveBalance
                {
                    OptionalLeave = 2,
                    PaidLeave = 20,
                    SickLeave = 10
                },
                PhotoPath = profilePhotoUrl,
            };
            var employeeDoc = await DocumentDBRepository.CreateItemAsync(employee);
            context.ConversationData.SetValue(ProfileKey, employee);

            var msg = context.MakeMessage();
            var card = EchoBot.SetManagerCard(); // WelcomeLeaveCard(employee.Name.Split(' ').First());

            msg.Attachments.Add(card);
            await context.PostAsync(msg);
        }

        /// <summary>
        /// Signs the user out from AAD
        /// </summary>
        public static async Task Signout(string emailId, IDialogContext context)
        {
            Console.WriteLine(emailId);// Use this to clean the DB.
            await context.SignOutUserAsync(ApplicationSettings.ConnectionName);
            await context.PostAsync($"You have been signed out.");
        }

        private static async Task<string> SendNotification(IDialogContext context, string managerUniqueId, string messageText, Bot.Connector.Attachment attachment, string updateMessageId)
        {
            var userId = managerUniqueId.Trim();
            var botId = context.Activity.Recipient.Id;
            var botName = context.Activity.Recipient.Name;

            var channelData = context.Activity.GetChannelData<TeamsChannelData>();
            var connectorClient = new ConnectorClient(new Uri(context.Activity.ServiceUrl));

            var parameters = new ConversationParameters
            {
                Bot = new ChannelAccount(botId, botName),
                Members = new ChannelAccount[] { new ChannelAccount(userId) },
                ChannelData = new TeamsChannelData
                {
                    Tenant = channelData.Tenant,
                }
            };

            try
            {
                var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);
                var replyMessage = Activity.CreateMessageActivity();
                replyMessage.From = new ChannelAccount(botId, botName);
                replyMessage.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
                replyMessage.ChannelData = new TeamsChannelData() { Notification = new NotificationInfo(true) };
                replyMessage.Text = messageText;
                if (attachment != null)
                    replyMessage.Attachments.Add(attachment);//  EchoBot.ManagerViewCard(employee, leaveDetails));

                if (string.IsNullOrEmpty(updateMessageId))
                {
                    var resourceResponse = await connectorClient.Conversations.SendToConversationAsync(conversationResource.Id, (Activity)replyMessage);
                    return resourceResponse.Id;
                }
                else
                {
                    await connectorClient.Conversations.UpdateActivityAsync(conversationResource.Id, updateMessageId, (Activity)replyMessage);
                    return updateMessageId; // Just return the same Id.
                }
            }
            catch (Exception ex)
            {
                // Handle the error.
                var msg = context.MakeMessage();
                msg.Text = ex.Message;
                await context.PostAsync(msg);
                return null;
            }
        }

    }
}