using ContosoAirline.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ContosoAirline.Repository;
using System.Net;

namespace ContosoAirline.Controllers
{
    public class PassengerController : Controller
    {
        // GET: Passenger
        public async Task<ActionResult> Index()
        {
            //List<Passenger> lst = TempData["Passenger"] as List<Passenger>;
            //Session["Passenger"] = lst;
            var passengers = await DocumentDBRepository<Passenger>.GetItemsAsync(d => d != null);
            return View(passengers);
        }

        // GET: Passenger/Create
        public ActionResult Create()
        {

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Name,From,To,Gender,Date,Seat,Class,FlightNumber,PNR,Notes,FrequentFlyerNumber,SpecialAssistance")] Passenger passenger)
        {
            if (ModelState.IsValid)
            {

                await DocumentDBRepository<Passenger>.CreateItemAsync(passenger);
                return RedirectToAction("Index");
            }

            return View(passenger);
        }

        // GET: /Passenger/Edit/12
        public async Task<ActionResult> Edit(string PNR)
        {
            if (PNR == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var passengers = await DocumentDBRepository<Passenger>.GetItemsAsync(d => d.PNR == PNR);
            Passenger passenger = passengers.FirstOrDefault();
            if (passenger == null)
            {
                return HttpNotFound();
            }
            return View(passenger);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "From,To,Gender,Seat,Class,FlightNumber,PNR, Notes,FrequentFlyerNumber,SpecialAssistance")] Passenger passenger)
        {
            if (ModelState.IsValid)
            {
                var passengers = await DocumentDBRepository<Passenger>.UpdateItemAsync(passenger.PNR, passenger);
                return RedirectToAction("Index");
            }
            return View(passenger);
        }

        // GET: /Passenger/Delete/5
        public async Task<ActionResult> Delete(string PNR)
        {
            if (PNR == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var passengers = await DocumentDBRepository<Passenger>.GetItemsAsync(d => d.PNR == PNR);
            Passenger passenger = passengers.FirstOrDefault();
            if (passenger == null)
            {
                return HttpNotFound();
            }
            return View(passenger);
        }
        // POST: /Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public  async Task<ActionResult> DeleteConfirmed(string PNR)
        {
            var passengers = await DocumentDBRepository<Passenger>.GetItemsAsync(d => d.PNR == PNR);
            Passenger passenger = passengers.FirstOrDefault();
            await DocumentDBRepository<Passenger>.DeleteDocumentAsync(PNR);
            return RedirectToAction("Index");
        }


        // ////////////////////////////// ==============================>

        //[Route("passenger/index")]
        //[ActionName("Index")]
        //[HttpGet]
        //public async Task<ActionResult> IndexAsync()
        //{
        //    var items = await DocumentDBRepository<Passenger>.GetItemsAsync(d => !string.IsNullOrEmpty(d.PNR));
        //    return View(items);
        //}

        //[ActionName("Create")]
        //public async Task<ActionResult> CreateAsync()
        //{
        //    return View();
        //}

        //[HttpPost]
        //[ActionName("Create")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> CreateAsync([Bind(Include = "Id,Name,Description,Completed")] Passenger passenger)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        await DocumentDBRepository<Passenger>.CreateItemAsync(passenger);
        //        return RedirectToAction("Index");
        //    }

        //    return View(passenger);
        //}


        //[HttpPost]
        //[ActionName("Edit")]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> EditAsync([Bind(Include = "Id,Name,Description,Completed")] Passenger passenger)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        await DocumentDBRepository<Passenger>.UpdateItemAsync(passenger.PNR, passenger);
        //        return RedirectToAction("Index");
        //    }

        //    return View(passenger);
        //}

        //[ActionName("Edit")]
        //public async Task<ActionResult> EditAsync(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    Passenger passenger = await DocumentDBRepository<Passenger>.GetItemAsync(id);
        //    if (passenger == null)
        //    {
        //        return HttpNotFound();
        //    }

        //    return View(passenger);
        //}






        //[Route("passenger/create")]
        //[HttpGet]
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //[Route("passenger/create")]
        //[HttpPost]
        //public async Task<ActionResult> Create(TaskItem item)
        //{
        //    item.Guid = Guid.NewGuid().ToString();
        //    TaskRepository.Tasks.Add(item);

        //    // Loop through subscriptions and notify each channel that task is created.
        //    foreach (var sub in SubscriptionRepository.Subscriptions)
        //    {
        //        await TaskHelper.PostTaskCreatedNotification(sub.WebHookUri, item);
        //    }

        //    return RedirectToAction("Detail", new { id = item.Guid });
        //}

        //[Route("task/detail/{id}")]
        //[HttpGet]
        //public ActionResult Detail(string id)
        //{
        //    return View(TaskRepository.Tasks.FirstOrDefault(i => i.Guid == id));
        //}
    }
}