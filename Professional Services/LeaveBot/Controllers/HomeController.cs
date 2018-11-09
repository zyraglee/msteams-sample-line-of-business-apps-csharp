using ProfessionalServices.LeaveBot.Repository;
using System.Web.Mvc;
using ProfessionalServices.LeaveBot.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ProfessionalServices.LeaveBot.Dialogs;
using Microsoft.Bot.Connector;

namespace ProfessionalServices.LeaveBot.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public async Task<ActionResult> Index(string Emailid)

        {
            if (Emailid != null)
            {
                
                var readEmployee = await DocumentDBRepository.GetItemAsync<Employee>(Emailid);
                DateTime nextholiday;
                foreach(var item in PublicHolidaysList.HolidayList)
                {
                    if(item.Date.Date>DateTime.Now.Date)
                    {
                        nextholiday = item.Date.Date;
                    }
                }
                if (readEmployee != null)
                {
                    readEmployee.IsManager = await RootDialog.IsManager(readEmployee);

                    var managername = await DocumentDBRepository.GetItemAsync<Employee>(readEmployee.ManagerEmailId);
                    if(managername!=null)
                    {
                        readEmployee.ManagerName = managername.DisplayName;
                        readEmployee.AzureADId = managername.AzureADId;
                    }
                }
                else
                {
                    return View();
                }

                List<LeaveExtended> leaveDetails = null;
                var readLeave = await DocumentDBRepository.GetItemsAsync<LeaveExtended>(e => e.Type == LeaveDetails.TYPE && e.AppliedByEmailId == Emailid);
                if (readLeave != null)
                {
                    readEmployee.Totalleaves = 0;
                    foreach (var item in readLeave)
                    {
                        TimeSpan diff = item.EndDate.Date.Subtract(item.StartDate.Date);
                        item.DaysDiff = Convert.ToInt32(diff.TotalDays);
                        item.startDay = item.StartDate.Date.ToString("dddd");
                        item.EndDay = item.EndDate.Date.ToString("dddd");
                        
                        item.StartDateval = item.StartDate.Date.ToString("MMM d");
                        item.EndDateVal = item.EndDate.Date.ToString("MMM d");
                        if (item.Status == LeaveStatus.Approved)
                            readEmployee.Totalleaves += item.DaysDiff;

                    }
                    leaveDetails = readLeave.ToList();
                }
                
                List<ManagerDetails> mrgLeavedata = null;
                var managerleave = await DocumentDBRepository.GetItemsAsync<ManagerDetails>(e => e.Type == LeaveDetails.TYPE && e.ManagerEmailId == Emailid && e.Status==0);
                if(managerleave!=null)
                {
                    foreach(var item in managerleave)
                    {
                        TimeSpan diff1 = item.EndDate.Date.Subtract(item.StartDate.Date);
                        item.mgrDaysdiff = Convert.ToInt32(diff1.TotalDays);
                        item.mgrstartDay = item.StartDate.Date.ToString("dddd");
                        item.mgrEndDay = item.EndDate.Date.ToString("dddd");
                        item.mgrStartDateval = item.StartDate.Date.ToString("MMM d");
                        item.mgrEndDateVal = item.EndDate.Date.ToString("MMM d");
                        var managerresource = await DocumentDBRepository.GetItemsAsync<Employee>(e => e.Type == Employee.TYPE && e.EmailId == item.AppliedByEmailId);
                        if(managerresource!=null)
                        {
                            foreach (var name in managerresource)
                            {
                                item.ResourceName = name.DisplayName;
                            }
                        }
                    }
                    mrgLeavedata = managerleave.ToList();
                }
                
                
                return View(Tuple.Create(readEmployee, leaveDetails, mrgLeavedata));
            }
            else
            {
                return View();
            }
        }


        [Route("first")]
        public ActionResult First()
        {
            List<PublicHoliday> holidayList = PublicHolidaysList.HolidayList;
            return View(holidayList);
        }

        [Route("second")]
        public ActionResult Second()
        {
            return View();
        }

        [Route("configure")]
        public ActionResult Configure()
        {
            return View();
        }

        [Route("GetEditCard")]
        public async Task<JsonResult> GetEditCard(string leaveId)
        {
            var leaveDetails = await DocumentDBRepository.GetItemAsync<LeaveDetails>(leaveId);
            var card = EchoBot.LeaveRequest(leaveDetails);
            return Json(card, JsonRequestBehavior.AllowGet);
        }
    }
}
