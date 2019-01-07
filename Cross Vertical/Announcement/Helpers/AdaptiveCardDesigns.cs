using AdaptiveCards;
using CrossVertical.Announcement.Helper;
using CrossVertical.Announcement.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using TaskModule;

namespace CrossVertical.Announcement.Helpers
{
    public class AdaptiveCardDesigns
    {
        public static Attachment GetWelcomeScreen(bool isChannelCard)
        {
            var card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url=new System.Uri("https://cixdoq.bn.files.1drv.com/y4mswnHfii0b0TH6lDflshlunlPiT0Xu8_Ncrbjr3bSFfCF8gV8IyIDUl8zA8rZWFgryCXQ6gJ-JImjNfLkM8NgQIxqsbeIgI9vwcNRg4GoWSPchAjX9beEYdncTExDslPlkAULCcy67-D7qvAaIGVnY38JoNekYOiKlUqp68gxZ2EINJIu_LN-PdBzPtlkpnGC?width=1373&height=242&cropmode=none")
                            },
                            new AdaptiveTextBlock()
                            {
                                Weight=AdaptiveTextWeight.Bolder,
                                Text="Reach people right where they collaborate. "

                            },
                            new AdaptiveTextBlock()
                            {
                                IsSubtle=true,
                                Text="Get the message out to employees using Microsoft Teams. Send announcements to a set of employees, stores, roles or locations in one or more channels or individually.\nUsing this app, you can:",
                                Wrap=true
                            },
                            new AdaptiveTextBlock()
                            {
                                Size=AdaptiveTextSize.Small,
                                IsSubtle=true,
                                Wrap=true,
                                Text="* Collaborate and communicate with large employee groups\n* Target announcements via 1:1 chats for select employees\n* Post in Channels to encourage discussion and feedback\n* Deliver announcements to desktop, web clients or mobile clients of Microsoft Teams  – wherever users are\n* Track and report employee engagement on what you post\n* Track and report employee’s “read receipt if requested "
                            },
                            new AdaptiveTextBlock()
                            {
                                Text= isChannelCard ? "Note: This application works only in personal scope." : "Take your pick to get started:",
                                IsSubtle=true,

                            }
                        }
                    }
                }
            };
            if (isChannelCard)
            {
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Id = "chatinpersonal",
                    Title = "Go to Personal App",
                    Url = new System.Uri($"https://teams.microsoft.com/l/chat/0/0?users=28:{ApplicationSettings.AppId}")
                });

            }
            else
                card.Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveSubmitAction()
                    {
                        Id = "createmessage",
                        Title = "📢 Create Message",
                        Data = new AdaptiveCardValue<ActionDetails>()
                        { Data = new ActionDetails() { ActionType = Constants.CreateOrEditAnnouncement } }
                    },
                        new AdaptiveSubmitAction()
                        {
                            Id="showdrafts",
                            Title="⏱️ View Drafts & Schedules",
                            Data = new ActionDetails() { ActionType = Constants.ShowAllDrafts}
                        },
                        new AdaptiveOpenUrlAction()
                        {
                            Id="viewall",
                            Title="📄 View All", // Take to Tab
                            Url = new System.Uri(Constants.HistoryTabDeeplink)
                        },
                        //,
                        //new AdaptiveSubmitAction()
                        //{
                        //    Id="viewstatistics",
                        //    Title="👁‍🗨 View Statistics"
                        //},
                        new AdaptiveSubmitAction()
                        {
                            Id = "adminpanel",
                            Title = "⚙️ Admin Panel",
                            Data = new ActionDetails() { ActionType = Constants.Configure }
                        }
                };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };
        }


        public static Attachment GetConfirmationCard(string announcementId, string date, string time)
        {
            var Card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Weight=AdaptiveTextWeight.Bolder,
                                Text="Please select your action:"
                            }
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                          {
                            new AdaptiveSubmitAction()
                            {
                                Id = "sendNow",
                                Title = "Send Now",
                                Data = new AnnouncementActionDetails()
                                {
                                    ActionType = Constants.SendAnnouncement ,
                                    Id = announcementId
                                }
                            },
                            new AdaptiveShowCardAction()
                            {
                                Id = "sendLater",
                                Title="Send Later",
                                Card=new AdaptiveCard()
                                {
                                    Body=new List<AdaptiveElement>()
                                    {
                                        new AdaptiveContainer()
                                        {
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveTextBlock()
                                                {
                                                    Text="Schedule your anouncement here"
                                                },
                                                new AdaptiveDateInput()
                                                {
                                                    Id = "Date",
                                                    Placeholder="Select Date",
                                                    Value = date
                                                },
                                                new AdaptiveTimeInput()
                                                {
                                                    Id = "Time",
                                                    Placeholder="Select time",
                                                    Value = time
                                                }
                                            }
                                        }
                                    },
                                    Actions=new List<AdaptiveAction>()
                                    {
                                      new AdaptiveSubmitAction()
                                      {
                                          Id= "schedule",
                                          Title="Schedule",
                                          Data = new AnnouncementActionDetails(){ Id = announcementId,  ActionType = Constants.ScheduleAnnouncement}
                                      }
                                    }
                                }
                              },
                              new AdaptiveSubmitAction()
                              {
                                  Id = "editAnnouncement",
                                  Title="Edit",
                                  Data = new AdaptiveCardValue<AnnouncementActionDetails>() {
                                      Data = new AnnouncementActionDetails() {
                                          ActionType = Constants.ShowEditAnnouncementTaskModule,
                                          Id = announcementId } }
                              }
                          }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };
        }

        public static Attachment GetUpdateMessageCard(string message)
        {
            var Card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Weight=AdaptiveTextWeight.Bolder,
                                Text=message,
                                Wrap = true
                            }
                        }
                    }
                }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };
        }

        public static Attachment GetCardWithAcknowledgementDetails(Attachment campaignAttachment, string id, string userId, string groupId)
        {
            var campaign = campaignAttachment.Content as AdaptiveCard;
            var action = campaign.Actions.FirstOrDefault(a => a.Title == Constants.Acknowledge);
            if (action != null)
            {
                var acknowledgeAction = action as AdaptiveSubmitAction;
                acknowledgeAction.Data = new AnnouncementAcknowledgeActionDetails()
                {
                    ActionType = Constants.Acknowledge,
                    Id = id,
                    GroupId = groupId,
                    UserId = userId
                };
            }
            return campaignAttachment;
        }

        public static Attachment GetCardToUpdatePreviewCard(Attachment campaignAttachment, string message)
        {

            var campaign = campaignAttachment.Content as AdaptiveCard;

            campaign.Body.Add(
                new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock()
                                    {
                                        Text= message,
                                        Wrap=true,
                                        HorizontalAlignment=AdaptiveHorizontalAlignment.Left,
                                        Spacing=AdaptiveSpacing.None,
                                        Weight=AdaptiveTextWeight.Bolder,
                                        Color= AdaptiveTextColor.Attention,
                                        MaxLines=1
                                    }
                                }
                });
            return campaignAttachment;
        }


        public static Attachment GetCardWithoutAcknowledgementAction(Attachment campaignAttachment)
        {
            var campaign = campaignAttachment.Content as AdaptiveCard;
            RemoveAction(campaign, Constants.Acknowledge);
            RemoveAction(campaign, Constants.ContactSender);
            return campaignAttachment;
        }

        private static void RemoveAction(AdaptiveCard campaign, string actionName)
        {
            var action = campaign.Actions.FirstOrDefault(a => a.Title == actionName);
            if (action != null)
            {
                campaign.Actions.Remove(action);
            }
        }
    }
}