using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using gzWeb.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using AutoMapper;

namespace gzWeb.Controllers
{
    public class InvestmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Investments
        [Authorize]
        public ActionResult Index()
        {
            db.Database.Log = new DebugTextWriter().Write;

            var manager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var customer = manager.FindById(User.Identity.GetUserId<int>());

            //var lastInv = customer.Transxes.Where(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss).OrderByDescending(t => t.Id).Select(t => t.Amount).FirstOrDefault();
            var invbal = customer.InvBalances.OrderByDescending(b => b.Id).Select(b => b.Balance).FirstOrDefault();

            var customerVM = new CustomerViewModel();
            Mapper.Map<ApplicationUser, CustomerViewModel>(customer, customerVM);
            return View(customerVM);
        }



        // GET: Investments ajax
        public JsonResult GetInvestAmnt() {
            var manager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var customer = manager.FindById(User.Identity.GetUserId<int>());

            var investments = customer.Transxes.Where(t => t.Type.Code == TransferTypeEnum.CreditedPlayingLoss).OrderByDescending(t => t.CreatedOnUTC).Select(t => new { t.Amount, t.CreatedOnUTC}).ToList();

            return Json(investments, JsonRequestBehavior.AllowGet);
        }

        // GET: Investments/Create
        public ActionResult Create()
        {
            ViewBag.Id = new SelectList(db.InvBalances, "CustomerId", "CustomerId");
            return View();
        }

        // POST: Investments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Birthday,PlatformCustomerId,ActiveCustomerIdInPlatform,PlatformBalance,LastUpdatedBalance,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.ApplicationUsers.Add(applicationUser);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    ViewBag.Id = new SelectList(db.InvBalances, "CustomerId", "CustomerId", applicationUser.Id);
        //    return View(applicationUser);
        //}

        // GET: Investments/Edit/5
        //public ActionResult Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ApplicationUser applicationUser = db.ApplicationUsers.Find(id);
        //    if (applicationUser == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    ViewBag.Id = new SelectList(db.InvBalances, "CustomerId", "CustomerId", applicationUser.Id);
        //    return View(applicationUser);
        //}

        // POST: Investments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Birthday,PlatformCustomerId,ActiveCustomerIdInPlatform,PlatformBalance,LastUpdatedBalance,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                db.Entry(applicationUser).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.InvBalances, "CustomerId", "CustomerId", applicationUser.Id);
            return View(applicationUser);
        }

        // GET: Investments/Delete/5
        //public ActionResult Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    ApplicationUser applicationUser = db.ApplicationUsers.Find(id);
        //    if (applicationUser == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(applicationUser);
        //}

        // POST: Investments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    ApplicationUser applicationUser = db.ApplicationUsers.Find(id);
        //    db.ApplicationUsers.Remove(applicationUser);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
