using Microsoft.Teams.Samples.HelloWorld.Web.Repository;
using System.Web.Mvc;
using Microsoft.Teams.Samples.HelloWorld.Web.Models;
using System;
using System.Threading.Tasks;

namespace Microsoft.Teams.Samples.HelloWorld.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("")]
        public ActionResult Index()
        {
            return View();
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
                    OptionalLeave = 2,
                    PersonalLeave = 20,
                    SickLeave = 10
                },
                ManagerEmailId = "v-washai@microsft.com",
                PhotoPath = @"D:\test\updatedpath\photo.png",
            };
            var employeeDoc = await DocumentDBRepository.CreateItemAsync(emp);

            var leave = new LeaveDetails()
            {
                AppliedByEmailId = "v-washai@micso.com",
                LeaveId = "someuniqueId123456876543",
                EmployeeComment = "Vacation",
                LeaveType = LeaveType.PersonalLeave,
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

            return View("Index");
        }

        [Route("first")]
        public ActionResult First()
        {
            return View();
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
