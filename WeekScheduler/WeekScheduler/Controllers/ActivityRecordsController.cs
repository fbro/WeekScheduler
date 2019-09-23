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
    public class ActivityRecordsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ActivityRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ActivityRecords
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {

            List<ActivityRecordModel> list;
            switch (sortLayout)
            {
                case "EmployeeID_asc":
                    list = await _context.ActivityRecord.OrderBy(e => e.EmployeeID).ToListAsync();
                    break;
                case "EmployeeID_desc":
                    list = await _context.ActivityRecord.OrderByDescending(e => e.EmployeeID).ToListAsync();
                    break;
                case "ActivityID_asc":
                    list = await _context.ActivityRecord.OrderBy(e => e.ActivityID).ToListAsync();
                    break;
                case "ActivityID_desc":
                    list = await _context.ActivityRecord.OrderByDescending(e => e.ActivityID).ToListAsync();
                    break;
                case "Weight_asc":
                    list = await _context.ActivityRecord.OrderBy(e => e.Weight).ToListAsync();
                    break;
                case "Weight_desc":
                    list = await _context.ActivityRecord.OrderByDescending(e => e.Weight).ToListAsync();
                    break;
                default:
                    list = await _context.ActivityRecord.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
            {
                list = list.Where(e => e.EmployeeID.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.ActivityID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Weight.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModel = new IndexActivityRecordViewModel
            {
                ActivityRecordModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: ActivityRecords/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(string EmployeeID, int? ActivityID)
        {
            if (EmployeeID == null || ActivityID == null)
            {
                return NotFound();
            }

            var activityRecord = await _context.ActivityRecord
                .FirstOrDefaultAsync(m => m.EmployeeID == EmployeeID && m.ActivityID == ActivityID);
            if (activityRecord == null)
            {
                return NotFound();
            }

            return View(activityRecord);
        }

        // GET: ActivityRecords/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View(new ActivityRecordModel());
        }

        // POST: ActivityRecords/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(ActivityRecordModel activityRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(activityRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(activityRecord);
        }

        // GET: ActivityRecords/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string EmployeeID, int? ActivityID)
        {
            if (EmployeeID == null || ActivityID == null)
            {
                return NotFound();
            }

            var activityRecord = await _context.ActivityRecord.FindAsync(EmployeeID, ActivityID);
            if (activityRecord == null)
            {
                return NotFound();
            }
            return View(activityRecord);
        }

        // POST: ActivityRecords/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string EmployeeID, int ActivityID, ActivityRecordModel activityRecord)
        {
            if (EmployeeID != activityRecord.EmployeeID || ActivityID != activityRecord.ActivityID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(activityRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityRecordExists(activityRecord.EmployeeID, activityRecord.ActivityID))
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
            return View(activityRecord);
        }

        // GET: ActivityRecords/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string EmployeeID, int? ActivityID)
        {
            if (EmployeeID == null || ActivityID == null)
            {
                return NotFound();
            }

            var activityRecord = await _context.ActivityRecord
                .FirstOrDefaultAsync(m => m.EmployeeID == EmployeeID && m.ActivityID == ActivityID);
            if (activityRecord == null)
            {
                return NotFound();
            }

            return View(activityRecord);
        }

        // POST: ActivityRecords/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(string EmployeeID, int ActivityID)
        {
            _context.ActivityRecord.Remove(await _context.ActivityRecord.FindAsync(EmployeeID, ActivityID));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ActivityRecordExists(string EmployeeID, int ActivityID)
        {
            return _context.ActivityRecord.Any(e => e.EmployeeID == EmployeeID && e.ActivityID == ActivityID);
        }
    }
}
