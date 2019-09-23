using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WeekScheduler.Data;
using WeekScheduler.Models;
using WeekScheduler.Repositories;
using WeekScheduler.ViewModels;

namespace WeekScheduler.Controllers
{
    public class ActivitiesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivitiesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Activities
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<ActivityModel> list;
            switch (sortLayout)
            {
                case "ActivityID_asc":
                    list = await _context.Activity.OrderBy(e => e.ActivityID).ToListAsync();
                    break;
                case "ActivityID_desc":
                    list = await _context.Activity.OrderByDescending(e => e.ActivityID).ToListAsync();
                    break;
                case "Name_asc":
                    list = await _context.Activity.OrderBy(e => e.Name).ToListAsync();
                    break;
                case "Name_desc":
                    list = await _context.Activity.OrderByDescending(e => e.Name).ToListAsync();
                    break;
                case "DayOfWeek_asc":
                    list = await _context.Activity.OrderBy(e => e.DayOfWeek).ToListAsync();
                    break;
                case "DayOfWeek_desc":
                    list = await _context.Activity.OrderByDescending(e => e.DayOfWeek).ToListAsync();
                    break;
                case "TimeOfDay_asc":
                    list = await _context.Activity.OrderBy(e => e.TimeOfDay).ToListAsync();
                    break;
                case "TimeOfDay_desc":
                    list = await _context.Activity.OrderByDescending(e => e.TimeOfDay).ToListAsync();
                    break;
                case "Hours_asc":
                    list = await _context.Activity.OrderBy(e => e.Hours).ToListAsync();
                    break;
                case "Hours_desc":
                    list = await _context.Activity.OrderByDescending(e => e.Hours).ToListAsync();
                    break;
                case "Importance_asc":
                    list = await _context.Activity.OrderBy(e => e.Importance).ToListAsync();
                    break;
                case "Importance_desc":
                    list = await _context.Activity.OrderByDescending(e => e.Importance).ToListAsync();
                    break;
                case "Owner_asc":
                    list = await _context.Activity.OrderBy(e => e.Owner).ToListAsync();
                    break;
                case "Owner_desc":
                    list = await _context.Activity.OrderByDescending(e => e.Owner).ToListAsync();
                    break;
                default:
                    list = await _context.Activity.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
            {
                list = list.Where(e => e.ActivityID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.DayOfWeek.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.TimeOfDay.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Hours.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Importance.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Owner.Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            }
                
            var viewModel = new IndexActivityViewModel
            {
                ActivityModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: Activities/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(int? ActivityID)
        {
            if (ActivityID == null)
                return NotFound();
            var activity = await _context.Activity.FirstOrDefaultAsync(m => m.ActivityID == ActivityID);
            if (activity == null)
                return NotFound();
            var detailsActivityViewModel = new DetailsActivityViewModel
            {
                ActivityID = activity.ActivityID,
                Name = activity.Name,
                DayOfWeek = activity.DayOfWeek,
                TimeOfDay = activity.TimeOfDay,
                Hours = activity.Hours,
                Importance = activity.Importance,
                Owner = activity.Owner,
                Tags = _context.Tag.Where(e => e.ActivityID == ActivityID.Value).Select(e => e.TagID).ToList(),
            };
            return View(detailsActivityViewModel);
        }

        // GET: Activities/Create
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Create()
        {
            var activity = new ActivityModel
            {
                ActivityID = _context.Activity.Count() + 1
            };
            return View(activity);
        }

        // POST: Activities/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Create(ActivityModel activity)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activity);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        // GET: Activities/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? ActivityID)
        {
            if (ActivityID == null)
            {
                return NotFound();
            }

            var activity = await _context.Activity.FindAsync(ActivityID);
            if (activity == null)
            {
                return NotFound();
            }
            var TagsCounter = new List<TagCount>();
            foreach (var tag in _context.Tag)
            {
                var cTag = TagsCounter.SingleOrDefault(e => e.TagName.Equals(tag.TagID));
                if (cTag == null)
                    if(tag.ActivityID.Equals(ActivityID.Value))
                        TagsCounter.Add(new TagCount { TagName = tag.TagID, TagCounter = 1, IsInActivity = true });
                    else
                        TagsCounter.Add(new TagCount { TagName = tag.TagID, TagCounter = 1, IsInActivity = false });
                else
                {
                    if (tag.ActivityID.Equals(ActivityID.Value))
                    {
                        cTag.TagCounter++;
                        cTag.IsInActivity = true;
                    }
                    else
                        cTag.TagCounter++;
                }
            }
            TagsCounter = TagsCounter.OrderByDescending(e => e.IsInActivity == true ? 100000 : 0 + e.TagCounter).ToList();
            var EditActivityViewModel = new EditActivityViewModel
            {
                ActivityID = activity.ActivityID,
                Name = activity.Name,
                DayOfWeek = activity.DayOfWeek,
                TimeOfDay = activity.TimeOfDay,
                Hours = activity.Hours,
                Importance = activity.Importance,
                Owner = activity.Owner,
                Tags = TagsCounter,
            };
            return View(EditActivityViewModel);
        }

        // POST: Activities/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int ActivityID, EditActivityViewModel activity)
        {
            if (ActivityID != activity.ActivityID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activity);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityExists(activity.ActivityID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(activity);
        }

        // GET: Activities/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? ActivityID)
        {
            if (ActivityID == null)
            {
                return NotFound();
            }

            var activity = await _context.Activity
                .FirstOrDefaultAsync(m => m.ActivityID == ActivityID);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // POST: Activities/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int ActivityID)
        {
            var activity = _context.Activity.Single(e => e.ActivityID == ActivityID);
            var slotsToDelete = new List<SlotModel>();
            var activityRecordsToDelete = new List<ActivityRecordModel>();
            foreach (var slot in _context.Slot.Where(e => e.ActivityID == ActivityID))
                slotsToDelete.Add(slot);
            foreach (var activityRecord in _context.ActivityRecord.Where(e => e.ActivityID == ActivityID))
                activityRecordsToDelete.Add(activityRecord);
            _context.Activity.Remove(activity);
            _context.Slot.RemoveRange(slotsToDelete);
            _context.ActivityRecord.RemoveRange(activityRecordsToDelete);
            _context.Tag.RemoveRange(_context.Tag.Where(e => e.ActivityID == ActivityID));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityExists(int ActivityID)
        {
            return _context.Activity.Any(e => e.ActivityID == ActivityID);
        }
    }
}