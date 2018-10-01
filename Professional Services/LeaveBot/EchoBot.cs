using System.Threading.Tasks;
using AdaptiveCards;
using System.Data;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class EchoBot
    {
        public static async Task EchoMessage(ConnectorClient connector, Activity activity)
        {
            var reply = activity.CreateReply();
            if (activity.Value != null)
            {

                if (activity.Value.ToString().Contains(Constants.LeaveRequest))
                {
                    reply.Attachments.Add(LeaveRequest());
                }
                else if (activity.Value.ToString().Contains(Constants.LeaveBalance))
                {
                    reply.Attachments.Add(ViewLeaveBalance());
                }
                else if (activity.Value.ToString().Contains(Constants.Holidays))
                {
                    reply.Attachments.Add(PublicHolidays());
                }
                else
                {
                    reply = activity.CreateReply("It will redirect to the tab");
                }

                await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
            }
            else
            {
                //reply = activity.CreateReply("Welcome to Adaptive Card Features");
                reply.Attachments.Add(WelcomeLeaveCard());
                //reply.Attachments.Add(LeaveRequest());
                //reply.Attachments.Add(ManagerViewCard());
                //reply.Attachments.Add(PublicHolidays());
                //reply.Attachments.Add(ViewLeaveBalance());
                await connector.Conversations.ReplyToActivityWithRetriesAsync(reply);
            }
        }

        private static Attachment WelcomeLeaveCard()
        {
            var WelcomeCard = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {

                    new AdaptiveContainer
                    {

                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock()
                            {
                                Text="Hey User! Here what I can do for you",
                                Size=AdaptiveTextSize.Large
                            },
                        }
                    }
                },
                Actions = new List<AdaptiveAction>()
                {
                     new AdaptiveSubmitAction()
                              {
                                  Title="Make a leave request",
                                  DataJson= @"{'Type':'" + Constants.LeaveRequest+"'}"

                              },
                     new AdaptiveSubmitAction()
                     {
                         Title="View Leave Balance",
                          DataJson= @"{'Type':'" + Constants.LeaveBalance+"'}"
                     },
                     new AdaptiveSubmitAction()
                     {
                         Title="View List of Public Holidays",
                          DataJson= @"{'Type':'" + Constants.Holidays+"'}"
                     },
                     new AdaptiveSubmitAction()
                     {
                         Title="See how it works",

                     }
                }
            };
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = WelcomeCard
            };

        }

        private static Attachment LeaveRequest()
        {
            var durations = new List<AdaptiveChoice>();
            durations.Add(new AdaptiveChoice() { Title = "FullDay", Value = "FullDay" });
            durations.Add(new AdaptiveChoice() { Title = "HalfDay", Value = "HalfDay" });

            var LeaveType = new List<AdaptiveChoice>();
            LeaveType.Add(new AdaptiveChoice() { Title = "Carried over from last year", Value = "1" });
            LeaveType.Add(new AdaptiveChoice() { Title = "Paid Leave", Value = "2" });

            var LeaveRequest = new AdaptiveCard()
            {

                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {

                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveColumnSet()
                    {
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text="From", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveDateInput(){Id="fromDate",Placeholder="From Date"}


                                }


                            },
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                   new AdaptiveTextBlock(){Text="Duration", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveChoiceSetInput(){Id="ddlFromDuration", Choices=new List<AdaptiveChoice>(durations), IsMultiSelect=false,Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay"}

                                }

                            }
                        }

                    },


                        }
                    },
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveColumnSet()
                    {
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text="To", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveDateInput(){Id="toDate",Placeholder="To Date"}


                                }


                            },
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                   new AdaptiveTextBlock(){Text="Duration", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveChoiceSetInput(){Id="ddlToDuration", Choices=new List<AdaptiveChoice>(durations), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay"}

                                }

                            }
                        }

                    },


                        }
                    },
                    new AdaptiveContainer()
                    {
                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveTextBlock(){Text="Please specify a reason for your leave", Color=AdaptiveTextColor.Attention, Weight=AdaptiveTextWeight.Lighter, Size=AdaptiveTextSize.Default}
                        }
                    }

                },
                Actions = new List<AdaptiveAction>()
                {
                    
                    new AdaptiveShowCardAction()
                    {
                        Title="Vacation",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextBlock(){Text="Yay! have a great Vacation!"},
                              new AdaptiveChoiceSetInput(){Id="ddlVacationLeave", Choices=new List<AdaptiveChoice>(LeaveType), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="txtleaveTypeReson", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Submit",
                              }
                          }
                       }
                    },
                     new AdaptiveShowCardAction()
                    {
                        Title="Sickness",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                               new AdaptiveTextBlock(){Text="Get well soon!"},
                              new AdaptiveChoiceSetInput(){Id="ddlSickleave", Choices=new List<AdaptiveChoice>(LeaveType), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="txtSickleavetypereason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Sickness",


                              }
                          }
                       }
                    },

                     new AdaptiveShowCardAction()
                    {
                        Title="Personal",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextBlock(){Text="Go ahead"},
                              new AdaptiveChoiceSetInput(){Id="ddlPersonalleave", Choices=new List<AdaptiveChoice>(LeaveType), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="txtPersonalleaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Personal",


                              }
                          }
                       }
                    },
                     new AdaptiveShowCardAction()
                    {
                        Title="Other",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextInput(){Id="txtOtherLeaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"},
                              new AdaptiveChoiceSetInput(){Id="ddlotherLeaveType", Choices=new List<AdaptiveChoice>(LeaveType), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Other",


                              }
                          }
                       }
                    }

                },
            };
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = LeaveRequest
            };
        }

        private static Attachment ManagerViewCard()
        {
            var card3 = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {
                             new AdaptiveColumnSet()
                    {
                        Columns=new List<AdaptiveColumn>()
                        {
                            new AdaptiveColumn()
                            {
                                Width=AdaptiveColumnWidth.Auto,
                                Items=new List<AdaptiveElement>()
                                {

                                    new AdaptiveImage(){Size=AdaptiveImageSize.Large,Url=new System.Uri("https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg"), Style=AdaptiveImageStyle.Person}
                                }

                            },
                            new AdaptiveColumn()
                            {

                                Spacing=AdaptiveSpacing.Large,
                                Width=AdaptiveColumnWidth.Stretch,
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text="Chris Naidoo has requsted for Paid leave for 2 Days", Size=AdaptiveTextSize.Medium,Wrap=true},

                                     new AdaptiveTextBlock(){Text="Thursday   Monday", Size=AdaptiveTextSize.Default,Wrap=true},
                                     new AdaptiveTextBlock(){Text="Sep 13   - Sep 17, 2018",Size=AdaptiveTextSize.Default,Wrap=true},
                                     new AdaptiveTextBlock(){Text="Reason:Personal Work",Weight=AdaptiveTextWeight.Bolder,Size=AdaptiveTextSize.Medium,Wrap=true},
                                    new AdaptiveTextBlock(){Text="I have an appoinment with my Doctor. will be available on call if required",HorizontalAlignment=AdaptiveHorizontalAlignment.Left,Wrap=true }

                                }

                            }
                        }

                    },


                        }
                    }

                },
                Actions = new List<AdaptiveAction>()
                {
                    new AdaptiveShowCardAction()
                    {
                        Title="Approve",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextInput(){Id="txtApproveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Approve"
                              },
                              new AdaptiveSubmitAction()
                              {
                                  Title="Cancel"
                              }
                          }
                       }
                    },
                    new AdaptiveShowCardAction()
                    {
                        Title="Decline",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextInput(){Id="txtDeclineReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Write a reason (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Decline"
                              },
                              new AdaptiveSubmitAction()
                              {
                                  Title="Cancel"
                              }
                          }
                       }
                    }

                },
            };
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card3
            };
        }

        private static Attachment ViewLeaveBalance()
        {
            var LeaveBalanceCard = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {

                            new AdaptiveTextBlock()
                            {
                                Text="Here's your balance status",
                                Size=AdaptiveTextSize.Large
                            },

                             new AdaptiveColumnSet()
                    {
                                 Spacing=AdaptiveSpacing.ExtraLarge,

                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text="Type", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding,Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter}


                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock(){Text="Remaining", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter}


                                }

                            }


                        },


                    },
                             new AdaptiveColumnSet()
                    {
                                 Separator=true,
                                 Spacing=AdaptiveSpacing.ExtraLarge,
                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                    new AdaptiveTextBlock(){Text="Paid leaves", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}



                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                     new AdaptiveTextBlock(){Text="19", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent}


                                }

                            }


                        },


                    },
                             new AdaptiveColumnSet()
                    {
                                 Separator=true,
                                 Spacing=AdaptiveSpacing.ExtraLarge,
                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                     new AdaptiveTextBlock(){Text="Sick leaves", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}



                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {


                                     new AdaptiveTextBlock(){Text="5", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent}


                                }

                            }


                        },


                    },
                             new AdaptiveColumnSet()
                    {
                                 Separator=true,
                                 Spacing=AdaptiveSpacing.ExtraLarge,
                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                      new AdaptiveTextBlock(){Text="Carried over from last year Recommended to utilise for vacations", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}



                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock(){Text="2", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent},

                                }

                            }


                        },


                    }


                        }
                    }


                },
                Actions = new List<AdaptiveAction>()
                {

                    new AdaptiveSubmitAction()
                    {
                        Title="View Details"

                    }


                },
            };
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = LeaveBalanceCard
            };
        }

        private static Attachment PublicHolidays()
        {
            var PublicHolidaysCard = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {
                    new AdaptiveContainer
                    {
                        Items=new List<AdaptiveElement>()
                        {

                            new AdaptiveTextBlock()
                            {
                                Text="Here is the list of upcoming public holidays",
                                Size=AdaptiveTextSize.Large
                            },

                             new AdaptiveColumnSet()
                    {
                                 Spacing=AdaptiveSpacing.ExtraLarge,

                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text="Date", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding,Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter}


                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock(){Text="Day", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter}


                                }

                            },
                             new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {
                                     new AdaptiveTextBlock(){Text="Event", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter}


                                }

                            }



                        },


                    },
                             new AdaptiveColumnSet()
                    {
                                 Separator=true,
                                 Spacing=AdaptiveSpacing.ExtraLarge,
                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                    new AdaptiveTextBlock(){Text="02 Oct 2018", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Bolder}



                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                     new AdaptiveTextBlock(){Text="Tuesday", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}


                                }

                            },
                             new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                     new AdaptiveTextBlock(){Text="Gandhi Jayanti", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}


                                }

                            }


                        },


                    },
                             new AdaptiveColumnSet()
                    {
                                 Separator=true,
                                 Spacing=AdaptiveSpacing.ExtraLarge,
                        Columns=new List<AdaptiveColumn>()
                        {

                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {

                                     new AdaptiveTextBlock(){Text="19 Oct 2018", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true, Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Bolder}



                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {


                                     new AdaptiveTextBlock(){Text="Friday", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}


                                }

                            },
                            new AdaptiveColumn()
                            {

                                Width="50",
                                Spacing=AdaptiveSpacing.ExtraLarge,
                                Items=new List<AdaptiveElement>()
                                {


                                     new AdaptiveTextBlock(){Text="Dussehra", Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Weight=AdaptiveTextWeight.Default}


                                }

                            }


                        },


                    },



                        }
                    }


                },
                Actions = new List<AdaptiveAction>()
                {

                    new AdaptiveSubmitAction()
                    {
                        Title="View Details"

                    }


                },
            };
            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = PublicHolidaysCard
            };
        }
    }

    public class Constants
    {
        public const string LeaveRequest = "Make a leave request";
        public const string LeaveBalance = "View Leave Balance";
        public const string Holidays = "View List of Public Holidays";
    }

    public class InputDetails
    {
        public string Type { get; set; }
    }
}
