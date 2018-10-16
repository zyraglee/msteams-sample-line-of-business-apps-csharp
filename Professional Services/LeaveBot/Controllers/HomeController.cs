using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using System.Web.Mvc;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Teams.Samples.HelloWorld.Web.Dialogs;
namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public async Task<ActionResult> Index(string Emailid)

        {
            //Emailid = "v-washai@microsoft.com";
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
                //List<string> lastMonth = null;
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

                        //lastMonth.Add(item.EndDate.Date.ToString("MMM"));
                    }
                    leaveDetails = readLeave.ToList();
                    
                    //readEmployee.lastUsed = lastMonth[0].ToString();
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
            //return View(readEmployee);
        }

        [Route("hello")]
        public async Task<ActionResult> Hello()
        {
            var emp = new Employee()
            {
                Name = "Wajeed",
                EmailId = "v-washai@microsoft.com",
                UserUniqueId = "Unique",
                TenantId = "microsfttenatnaid",
                DemoManagerEmailId = "v-washai@microsoft.com",
                LeaveBalance = new LeaveBalance
                {
                    PaidLeave = 20,
                    SickLeave = 10,
                    OptionalLeave = 2,
                    CarriedLeave = 2,
                    MaternityLeave = 0,
                    PaternityLeave = 0,
                    Caregiver = 0
                },
                ManagerEmailId = "v-washai@microsft.com",
                PhotoPath = @"D:\Microsoft\Test\1.png",
            };
            var employeeDoc = await DocumentDBRepository.CreateItemAsync(emp);

            var leave = new LeaveDetails()
            {
                AppliedByEmailId = "v-washai@micso.com",
                LeaveId = "someuniqueId123456876543",
                EmployeeComment = "Vacation",
                LeaveType = LeaveType.PaidLeave,
                StartDate = new LeaveDate() { Date = DateTime.Now, Type = DayType.FullDay },
                EndDate = new LeaveDate() { Date = DateTime.Now, Type = DayType.FullDay },
                Status = LeaveStatus.Pending,
                ManagerComment = "You can take the leave"
            };

            var leaveDoc = await DocumentDBRepository.CreateItemAsync<LeaveDetails>(leave);

            var AllEmployees = await DocumentDBRepository.GetItemsAsync<Employee>(e => e.Type == Employee.TYPE);

            var AllLeave = await DocumentDBRepository.GetItemsAsync<LeaveDetails>(e => e.Type == LeaveDetails.TYPE);


            var readEmployee = await DocumentDBRepository.GetItemAsync<Employee>("v-washai@microsoft.com");

            var readLeave = await DocumentDBRepository.GetItemAsync<LeaveDetails>("someuniqueId123456876543");

            readEmployee.LeaveBalance.OptionalLeave = 100;

            var upatedEmp = await DocumentDBRepository.UpdateItemAsync<Employee>(readEmployee.EmailId, readEmployee);

            return View(Tuple.Create(readEmployee,readLeave));
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
    }
}
