using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using System.Web.Mvc;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public async Task<ActionResult> Index(string Emailid)
        {
            if (Emailid != null)
            {
                //Emailid = "v-washai@microsoft.com";
                var readEmployee = await DocumentDBRepository.GetItemAsync<Employee>(Emailid);
                if (readEmployee != null)
                {
                    string[] name = readEmployee.Name.Split();
                    readEmployee.Name = name[0];
                }

                List<LeaveDetails> leaveDetails = null;
                //List<string> lastMonth = null;
                var readLeave = await DocumentDBRepository.GetItemsAsync<LeaveDetails>(e => e.Type == LeaveDetails.TYPE && e.AppliedByEmailId == Emailid);
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
                var upatedEmp = await DocumentDBRepository.UpdateItemAsync<Employee>(readEmployee.EmailId, readEmployee);

                return View(Tuple.Create(readEmployee, leaveDetails));
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
                Status = LeaveStatus.PendingApproval,
                ManagerComment = "You can tkae the leave"
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
