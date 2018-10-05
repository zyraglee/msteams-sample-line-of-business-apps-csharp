using System.Threading.Tasks;
using AdaptiveCards;
using System.Data;
using System.Collections.Generic;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Teams;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using System;
using Microsoft.Teams.Samples.HelloWorld.Web.Helpers;
using Microsoft.Teams.Samples.HelloWorld.Web.Helper;

namespace Microsoft.Teams.Samples.HelloWorld.Web
{
    public class EchoBot
    {

        public static Attachment WelcomeLeaveCard(string userName, bool isManager)
        {
            var WelcomeCard = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                {

                    new AdaptiveContainer
                    {

                        Items=new List<AdaptiveElement>()
                        {
                            new AdaptiveImage()
                            {
                                Url = new Uri(ApplicationSettings.BaseUrl + "/Resources/welcomebanner.png")
                            },
                            new AdaptiveTextBlock()
                            {
                                Text=$"Hey {userName}! Here what I can do for you",
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

            if (isManager)
                WelcomeCard.Actions.Insert(2,
                    new AdaptiveSubmitAction()
                    {
                        Title = "Show Pending Approvals",
                        DataJson = @"{'Type':'" + Constants.ShowPendingApprovals + "'}"
                    });

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = WelcomeCard
            };

        }

        public static Attachment SetManagerCard()
        {
            var Card = new AdaptiveCard()
            {
                Body = new List<AdaptiveElement>()
                          {
                              new AdaptiveTextBlock(){Text="Enter Manager Email Id:"},
                              new AdaptiveTextInput(){Id="txtManager", IsMultiline=false, IsRequired=true, Placeholder="Manager Email Id"}
                          },
                Actions = new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Set Manager",
                                  DataJson= @"{'Type':'" + Constants.SetManager+"'}"
                              }
                          }
            };

            return new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = Card
            };
        }

        public static Attachment LeaveRequest()
        {
            var durations = new List<AdaptiveChoice>();
            durations.Add(new AdaptiveChoice() { Title = "FullDay", Value = "FullDay" });
            durations.Add(new AdaptiveChoice() { Title = "HalfDay", Value = "HalfDay" });

            var paidLeave = new AdaptiveChoice() { Title = "Paid Leave", Value = LeaveType.PaidLeave.ToString() };
            var sickLeave = new AdaptiveChoice() { Title = "Sick Leave", Value = LeaveType.SickLeave.ToString() };
            var optionalLeave = new AdaptiveChoice() { Title = "Optional Leave", Value = LeaveType.OptionalLeave.ToString() };
            var carriedOverLeave = new AdaptiveChoice() { Title = "Carried over from last year", Value = LeaveType.CarriedLeave.ToString() };

            var maternityLeave = new AdaptiveChoice() { Title = "Maternity Leave", Value = LeaveType.MaternityLeave.ToString() };
            var paternityLeave = new AdaptiveChoice() { Title = "Paternity Leave", Value = LeaveType.PaternityLeave.ToString() };
            var caregiverLeave = new AdaptiveChoice() { Title = "Caregiver Leave", Value = LeaveType.Caregiver.ToString() };

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
                                    new AdaptiveDateInput(){Id="FromDate",Placeholder="From Date"}


                                }


                            },
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                   new AdaptiveTextBlock(){Text="Duration", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveChoiceSetInput(){Id="FromDuration", Choices=new List<AdaptiveChoice>(durations), IsMultiSelect=false,Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay"}

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
                                    new AdaptiveDateInput(){Id="ToDate",Placeholder="To Date"}


                                }


                            },
                            new AdaptiveColumn()
                            {
                                Width="50",
                                Items=new List<AdaptiveElement>()
                                {
                                   new AdaptiveTextBlock(){Text="Duration", Weight=AdaptiveTextWeight.Lighter,Size=AdaptiveTextSize.Medium,Wrap=true },
                                    new AdaptiveChoiceSetInput(){Id="ToDuration", Choices=new List<AdaptiveChoice>(durations), IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay"}

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
                            new AdaptiveTextBlock(){Text="Please specify a reason for your leave", Color=AdaptiveTextColor.Accent, Weight=AdaptiveTextWeight.Lighter, Size=AdaptiveTextSize.Default}
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
                                            Spacing = AdaptiveSpacing.None,
                                            Width="auto",
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveImage() { Url = new Uri(ApplicationSettings.BaseUrl + "/Resources/Vacation-01.png") }
                                            }
                                        },
                                        new AdaptiveColumn()
                                        {
                                            Spacing = AdaptiveSpacing.Small,
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveTextBlock(){Text="Yay! have a great Vacation!"}

                                            }

                                        }
                                    }

                                },


                                    }
                                }
                             ,
                              new AdaptiveChoiceSetInput(){Id="LeaveType", Choices=new List<AdaptiveChoice>{paidLeave, optionalLeave, carriedOverLeave  } , IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="LeaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Submit",
                                  DataJson= @"{'Type':'" + Constants.ApplyForVacation+"'}"
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
                                            Spacing = AdaptiveSpacing.None,
                                            Width="auto",
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveImage() { Url = new Uri(ApplicationSettings.BaseUrl + "/Resources/HeartIcon.png") }
                                            }
                                        },
                                        new AdaptiveColumn()
                                        {
                                            Spacing = AdaptiveSpacing.Small,
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveTextBlock(){Text="Get well soon!"}

                                            }

                                        }
                                    }

                                },


                                    }
                                }
                               ,
                              new AdaptiveChoiceSetInput(){Id="LeaveType", Choices=new List<AdaptiveChoice>(){ sickLeave }, IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="LeaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Sickness",
                                  DataJson= @"{'Type':'" + Constants.ApplyForSickLeave+"'}"
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
                                            Spacing = AdaptiveSpacing.None,
                                            Width="auto",
                                            Items=new List<AdaptiveElement>()
                                            {
                                                new AdaptiveImage() { Url = new Uri(ApplicationSettings.BaseUrl + "/Resources/Like.png") }
                                            }
                                        },
                                        new AdaptiveColumn()
                                        {
                                            Spacing = AdaptiveSpacing.Small,
                                            Items=new List<AdaptiveElement>()
                                            {
                                                    new AdaptiveTextBlock(){Text="Go ahead"}
                                            }

                                        }
                                    }

                                },


                                    }
                                },
                              new AdaptiveChoiceSetInput(){Id="LeaveType", Choices=new List<AdaptiveChoice>() { paidLeave, optionalLeave, carriedOverLeave }, IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                              new AdaptiveTextInput(){Id="LeaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Personal",
                                  DataJson= @"{'Type':'" + Constants.ApplyForPersonalLeave+"'}"
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
                              new AdaptiveTextInput(){Id="LeaveReason", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"},
                              new AdaptiveChoiceSetInput(){Id="LeaveType", Choices=new List<AdaptiveChoice>() { optionalLeave, maternityLeave, paternityLeave, caregiverLeave }, IsMultiSelect=false, Style=AdaptiveChoiceInputStyle.Compact, Value="FullDay", IsRequired=true},
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Other",
                                  DataJson= @"{'Type':'" + Constants.ApplyForOtherLeave+"'}"
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

        public static Attachment ManagerViewCard(Employee employee, LeaveDetails leaveDetails)
        {
            double dayCount = GetDayCount(leaveDetails);

            dayCount = Math.Round(dayCount, 1);

            var startDay = leaveDetails.StartDate.Date.ToString("dddd");
            var endDay = leaveDetails.EndDate.Date.ToString("dddd");

            var startDate = leaveDetails.StartDate.Date.ToString("MMM d");
            var endDate = leaveDetails.EndDate.Date.ToString("MMM d");

            var leaveType = GetDisplayText(leaveDetails.LeaveType);
            Uri photoUri = null;
            if (employee.PhotoPath != null)
                photoUri = new Uri(employee.PhotoPath);

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

                                    new AdaptiveImage(){Size=AdaptiveImageSize.Large,Url=photoUri,
                                        Style =AdaptiveImageStyle.Person}
                                }

                            },
                            new AdaptiveColumn()
                            {

                                Spacing=AdaptiveSpacing.Large,
                                Width=AdaptiveColumnWidth.Stretch,
                                Items=new List<AdaptiveElement>()
                                {
                                    new AdaptiveTextBlock(){Text=$"{employee.DisplayName} has requsted for Paid leave for {dayCount} Days", Size=AdaptiveTextSize.Medium,Wrap=true},

                                     new AdaptiveTextBlock(){Text=$"{startDay}   {endDay}", Size=AdaptiveTextSize.Default,Wrap=true},
                                     new AdaptiveTextBlock(){Text=$"{startDate}   - {endDate}, {leaveDetails.EndDate.Date.Year}",Size=AdaptiveTextSize.Default,Wrap=true},
                                     new AdaptiveTextBlock(){Text=$"Reason:{leaveType}",Weight=AdaptiveTextWeight.Bolder,Size=AdaptiveTextSize.Medium,Wrap=true},
                                    new AdaptiveTextBlock(){Text=leaveDetails.EmployeeComment,HorizontalAlignment=AdaptiveHorizontalAlignment.Left,Wrap=true }

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
                              new AdaptiveTextInput(){Id="ManagerComment", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Approve",
                                  DataJson= @"{'Type':'" + Constants.ApproveLeave+"', 'LeaveId':'" + leaveDetails.LeaveId+"'}"

                              }
                          }
                       }
                    },
                    new AdaptiveShowCardAction()
                    {
                        Title="Reject",

                         Card=new AdaptiveCard()
                       {
                          Body=new List<AdaptiveElement>()
                          {
                              new AdaptiveTextInput(){Id="ManagerComment", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Write a reason (Optional)"}
                          },
                          Actions=new List<AdaptiveAction>()
                          {
                              new AdaptiveSubmitAction()
                              {
                                  Title="Reject",
                                  DataJson= @"{'Type':'" + Constants.RejectLeave+"', 'LeaveId':'" + leaveDetails.LeaveId +"'}"
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

        public static Attachment EmployeeViewCard(Employee employee, LeaveDetails leaveDetails)
        {
            double dayCount = GetDayCount(leaveDetails);

            dayCount = Math.Round(dayCount, 1);

            var startDay = leaveDetails.StartDate.Date.ToString("dddd");
            var endDay = leaveDetails.EndDate.Date.ToString("dddd");

            var startDate = leaveDetails.StartDate.Date.ToString("MMM d");
            var endDate = leaveDetails.EndDate.Date.ToString("MMM d, yyyy");

            var leaveType = GetShortDisplayText(leaveDetails.LeaveType);
            Uri photoUri = null;
            if (employee.PhotoPath != null)
                photoUri = new Uri(employee.PhotoPath);

            var json = AdaptiveCardHelper.GetAdaptiveCardJson();
            json = json.Replace("{StartDay}", startDay);
            json = json.Replace("{EndDay}", endDay);

            json = json.Replace("{StartDate}", startDate);
            json = json.Replace("{EndDate}", endDate);

            json = json.Replace("{DayCount}", dayCount.ToString());

            json = json.Replace("{LeaveType}", leaveType);

            json = json.Replace("{LeaveReason}", leaveDetails.EmployeeComment);

            json = json.Replace("{Status}", "Pending");

            var card = AdaptiveCardHelper.GetAdaptiveCardFromJosn(json);
            //var card3 = new AdaptiveCard()
            //{
            //    Body = new List<AdaptiveElement>()
            //    {
            //        new AdaptiveContainer
            //        {
            //            Items=new List<AdaptiveElement>()
            //            {
            //                 new AdaptiveColumnSet()
            //        {
            //            Columns=new List<AdaptiveColumn>()
            //            {
            //                new AdaptiveColumn()
            //                {
            //                    Width=AdaptiveColumnWidth.Auto,
            //                    Items=new List<AdaptiveElement>()
            //                    {

            //                        new AdaptiveImage(){Size=AdaptiveImageSize.Large,Url=photoUri,
            //                            Style =AdaptiveImageStyle.Person}
            //                    }

            //                },
            //                new AdaptiveColumn()
            //                {

            //                    Spacing=AdaptiveSpacing.Large,
            //                    Width=AdaptiveColumnWidth.Stretch,
            //                    Items=new List<AdaptiveElement>()
            //                    {
            //                        new AdaptiveTextBlock(){Text=$"{employee.DisplayName} has requsted for Paid leave for {dayCount} Days", Size=AdaptiveTextSize.Medium,Wrap=true},

            //                         new AdaptiveTextBlock(){Text=$"{startDay}   {endDay}", Size=AdaptiveTextSize.Default,Wrap=true},
            //                         new AdaptiveTextBlock(){Text=$"{startDate}   - {endDate}, {leaveDetails.EndDate.Date.Year}",Size=AdaptiveTextSize.Default,Wrap=true},
            //                         new AdaptiveTextBlock(){Text=$"Reason:{leaveType}",Weight=AdaptiveTextWeight.Bolder,Size=AdaptiveTextSize.Medium,Wrap=true},
            //                        new AdaptiveTextBlock(){Text=leaveDetails.EmployeeComment,HorizontalAlignment=AdaptiveHorizontalAlignment.Left,Wrap=true }

            //                    }

            //                }
            //            }

            //        },
            //            }
            //        }

            //    },
            //    Actions = new List<AdaptiveAction>()
            //    {
            //        new AdaptiveShowCardAction()
            //        {
            //            Title="Approve",

            //             Card=new AdaptiveCard()
            //           {
            //              Body=new List<AdaptiveElement>()
            //              {
            //                  new AdaptiveTextInput(){Id="ManagerComment", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Comments (Optional)"}
            //              },
            //              Actions=new List<AdaptiveAction>()
            //              {
            //                  new AdaptiveSubmitAction()
            //                  {
            //                      Title="Approve",
            //                      DataJson= @"{'Type':'" + Constants.ApproveLeave+"', 'LeaveId':'" + leaveDetails.LeaveId+"'}"

            //                  }
            //              }
            //           }
            //        },
            //        new AdaptiveShowCardAction()
            //        {
            //            Title="Reject",

            //             Card=new AdaptiveCard()
            //           {
            //              Body=new List<AdaptiveElement>()
            //              {
            //                  new AdaptiveTextInput(){Id="ManagerComment", IsMultiline=true,MaxLength=300, IsRequired=true, Placeholder="Write a reason (Optional)"}
            //              },
            //              Actions=new List<AdaptiveAction>()
            //              {
            //                  new AdaptiveSubmitAction()
            //                  {
            //                      Title="Reject",
            //                      DataJson= @"{'Type':'" + Constants.RejectLeave+"', 'LeaveId':'" + leaveDetails.LeaveId +"'}"
            //                  }
            //              }
            //           }
            //        }

            //    },
            //};
            return card;
        }

        public static double GetDayCount(LeaveDetails leaveDetails)
        {
            var dayCount = (leaveDetails.EndDate.Date - leaveDetails.StartDate.Date).TotalDays + 1;
            if (leaveDetails.EndDate.Type == DayType.HalfDay)
                dayCount -= 0.5;
            if (leaveDetails.StartDate.Type == DayType.HalfDay)
                dayCount -= 0.5;
            return dayCount;
        }

        private static string GetDisplayText(LeaveType leaveDetails)
        {
            switch (leaveDetails)
            {
                case LeaveType.PaidLeave:
                    return "Paid Leave";
                case LeaveType.SickLeave:
                    return "Sick Leave";
                case LeaveType.OptionalLeave:
                    return "Optional Leave";
                case LeaveType.CarriedLeave:
                    return "Carried Leave";
                case LeaveType.MaternityLeave:
                    return "Maternity Leave";
                case LeaveType.PaternityLeave:
                    return "Paternity Leave";
                case LeaveType.Caregiver:
                    return "Caregiver";
                default:
                    break;
            }
            return leaveDetails.ToString();
        }

        private static string GetShortDisplayText(LeaveType leaveDetails)
        {
            switch (leaveDetails)
            {
                case LeaveType.PaidLeave:
                    return "PL";
                case LeaveType.SickLeave:
                    return "SL";
                case LeaveType.OptionalLeave:
                    return "OL";
                case LeaveType.CarriedLeave:
                    return "CL";
                case LeaveType.MaternityLeave:
                    return "ML";
                case LeaveType.PaternityLeave:
                    return "PT";
                case LeaveType.Caregiver:
                    return "CG";
                default:
                    break;
            }
            return leaveDetails.ToString();
        }

        public static Attachment ViewLeaveBalance(Employee employee)
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

                                     new AdaptiveTextBlock(){Text=employee.LeaveBalance.PaidLeave.ToString(), Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent}


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


                                     new AdaptiveTextBlock(){Text=employee.LeaveBalance.SickLeave.ToString(), Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent}


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
                                     new AdaptiveTextBlock(){Text=employee.LeaveBalance.OptionalLeave.ToString(), Size=AdaptiveTextSize.Medium,Wrap=true, Separator=true,  Spacing=AdaptiveSpacing.Padding, Color=AdaptiveTextColor.Accent},

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

        public static Attachment PublicHolidays()
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
        public const string ShowPendingApprovals = "ShowPendingApprovals";

        public const string ApplyForVacation = "ApplyForVacation";
        public const string ApplyForSickLeave = "ApplyForSickLeave";
        public const string ApplyForPersonalLeave = "ApplyForPersonalLeave";
        public const string ApplyForOtherLeave = "ApplyForOtherLeave";

        public const string ApproveLeave = "ApproveLeave";
        public const string RejectLeave = "RejectLeave";
        public const string SetManager = "SetManager";

        public const string LeaveBalance = "View Leave Balance";
        public const string Holidays = "View List of Public Holidays";
    }

    public class InputDetails
    {
        public string Type { get; set; }

    }

    public class SetManagerDetails
    {
        public string Type { get; set; }
        public string txtManager { get; set; }
    }


    public class VacationDetails
    {
        public string Type { get; set; }
        public string FromDate { get; set; }
        public string FromDuration { get; set; }
        public string ToDate { get; set; }
        public string ToDuration { get; set; }
        public string LeaveType { get; set; }
        public string LeaveReason { get; set; }
    }



    public class ManagerResponse
    {
        public string Type { get; set; }
        public string LeaveId { get; set; }
        public string ManagerComment { get; set; }
    }



}
