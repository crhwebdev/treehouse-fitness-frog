﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Treehouse.FitnessFrog.Data;
using Treehouse.FitnessFrog.Models;

namespace Treehouse.FitnessFrog.Controllers
{
    public class EntriesController : Controller
    {
        private EntriesRepository _entriesRepository = null;

        public EntriesController()
        {
            _entriesRepository = new EntriesRepository();
        }

        public ActionResult Index()
        {
            List<Entry> entries = _entriesRepository.GetEntries();

            // Calculate the total activity.
            double totalActivity = entries
                .Where(e => e.Exclude == false)
                .Sum(e => e.Duration);

            // Determine the number of days that have entries.
            int numberOfActiveDays = entries
                .Select(e => e.Date)
                .Distinct()
                .Count();

            ViewBag.TotalActivity = totalActivity;
            ViewBag.AverageDailyActivity = (totalActivity / (double)numberOfActiveDays);

            return View(entries);
        }

        // Add GET Route
        public ActionResult Add()
        {
            var entry = new Entry()
            {
                Date = DateTime.Today
            };

            //passing list of activitiesin Data to Add.cshtml to be used to populate SelectList
            SetupActivitiesSelectListItems();

            //pass a new instance of Entry class to the view
            return View(entry);
        }

        // Add POST Route
        //Define a Post Action to same route as above using an attribute
        // [ActionName("Add"), HttpPost]
        // First part aliases the route to Add
        // Second part tells what kind of http action it is: POST
        // But, because our method signature is now different because we are passing in Form fields as parameters
        // We don't need to alias the method to AddPost
        // Update:  use Entry class instead of idividual properties of form - C# will populate Entry class with properties
        [HttpPost]
        public ActionResult Add(Entry entry)
        {
            ValidateEntry(entry);
            //check to see if Model is valid (i.e. no errors)
            if (ModelState.IsValid)
            {
                //if it has no errors, add it to the repository
                _entriesRepository.AddEntry(entry);

                TempData["Message"] = "Your entry was successfully added!";

                //redirect to Index page
                return RedirectToAction("Index");
            }

            //passing list of activitiesin Data to Add.cshtml to be used to populate SelectList
            SetupActivitiesSelectListItems();

            //return page View with entry object passed back
            return View(entry);
        }
       
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Entry entry = _entriesRepository.GetEntry((int)id);

            if(entry == null)
            {
                return HttpNotFound();
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        [HttpPost]
        public ActionResult Edit(Entry entry)
        {
            ValidateEntry(entry);

            if (ModelState.IsValid)
            {
                _entriesRepository.UpdateEntry(entry);

                //set temp data so that redirect page will have access to TempData["Message"].
                TempData["Message"] = "Your entry was successfully updated!";

                return RedirectToAction("Index");
            }

            SetupActivitiesSelectListItems();

            return View(entry);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Entry entry = _entriesRepository.GetEntry((int)id);

            if (entry == null)
            {
                return HttpNotFound();
            }

            return View(entry);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            _entriesRepository.DeleteEntry(id);

            TempData["Message"] = "Your entry was successfully deleted!";

            return RedirectToAction("Index");
        }

        private void ValidateEntry(Entry entry)
        {
            // If there aren't any "Duration" field validation errors
            // then make sure that the duration is greater than "0".
            if (ModelState.IsValidField("Duration") && entry.Duration <= 0)
            {
                ModelState.AddModelError("Duration",
                    "The Duration field value must be greater than '0'.");
            }
        }

        private void SetupActivitiesSelectListItems()
        {
            ViewBag.ActivitiesSelectListItems = new SelectList(
                Data.Data.Activities, "Id", "Name");
        }
    }
}