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
    public class SlotsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly SlotsRepository slotsRepository = new SlotsRepository();

        public SlotsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Slots
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<SlotModel> list;
            switch (sortLayout)
            {
                case "ProjectID_asc":
                    list = await _context.Slot.OrderBy(e => e.ProjectID).ToListAsync();
                    break;
                case "ProjectID_desc":
                    list = await _context.Slot.OrderByDescending(e => e.ProjectID).ToListAsync();
                    break;
                case "WeekID_asc":
                    list = await _context.Slot.OrderBy(e => e.WeekID).ToListAsync();
                    break;
                case "WeekID_desc":
                    list = await _context.Slot.OrderByDescending(e => e.WeekID).ToListAsync();
                    break;
                case "ActivityID_asc":
                    list = await _context.Slot.OrderBy(e => e.ActivityID).ToListAsync();
                    break;
                case "ActivityID_desc":
                    list = await _context.Slot.OrderByDescending(e => e.ActivityID).ToListAsync();
                    break;
                case "EmployeeID_asc":
                    list = await _context.Slot.OrderBy(e => e.EmployeeID).ToListAsync();
                    break;
                case "EmployeeID_desc":
                    list = await _context.Slot.OrderByDescending(e => e.EmployeeID).ToListAsync();
                    break;
                case "Notes_asc":
                    list = await _context.Slot.OrderBy(e => e.Notes).ToListAsync();
                    break;
                case "Notes_desc":
                    list = await _context.Slot.OrderByDescending(e => e.Notes).ToListAsync();
                    break;
                default:
                    list = await _context.Slot.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
                list = list.Where(e => e.ProjectID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.WeekID.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.ActivityID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.EmployeeID.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Notes.Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList(); 
             var viewModel = new IndexSlotViewModel
            {
                SlotModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: Slots/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(int? ProjectID, string WeekID, int? ActivityID, string EmployeeID)
        {
            if (ProjectID == null || WeekID == null || ActivityID == null || EmployeeID == null)
                return NotFound();
            var slot = await _context.Slot
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.WeekID == WeekID && m.ActivityID == ActivityID && m.EmployeeID == EmployeeID);
            if (slot == null)
                return NotFound();
            var activity = _context.Activity.Single(e => e.ActivityID == ActivityID);
            var activityRecord = _context.ActivityRecord.SingleOrDefault(e => e.ActivityID == ActivityID && e.EmployeeID == EmployeeID);
            var slotViewModel = new DetailsSlotViewModel
            {
                ProjectID = slot.ProjectID,
                WeekID = slot.WeekID,
                ActivityID = slot.ActivityID,
                EmployeeID = slot.EmployeeID,
                Notes = slot.Notes,
                DetailsActivityViewModel = new DetailsActivityViewModel
                {
                    ActivityID = activity.ActivityID,
                    Name = activity.Name,
                    DayOfWeek = activity.DayOfWeek,
                    TimeOfDay = activity.TimeOfDay,
                    Hours = activity.Hours,
                    Importance = activity.Importance,
                    Owner = activity.Owner,
                    Tags = _context.Tag.Where(e => e.ActivityID == ActivityID.Value).Select(e => e.TagID).ToList(),
                },
                ActivityRecordModel = activityRecord,
            };
            return View(slotViewModel);
        }

        // GET: Slots/Create
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            return View(new SlotModel());
        }

        // POST: Slots/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(SlotModel slot)
        {
            if (ModelState.IsValid)
            {
                slotsRepository.CreateSlot(_context, slot);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(slot);
        }

        // GET: Slots/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? ProjectID, string WeekID, int? ActivityID, string EmployeeID)
        {
            if (ProjectID == null || WeekID == null || ActivityID == null || EmployeeID == null)
                return NotFound();
            var slot = await _context.Slot.FindAsync(ProjectID, WeekID, ActivityID, EmployeeID);
            if (slot == null)
                return NotFound();
            return View(slot);
        }

        // POST: Slots/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int ProjectID, string WeekID, int ActivityID, string EmployeeID, SlotModel slot)
        {
            if (ProjectID != slot.ProjectID || WeekID != slot.WeekID || ActivityID != slot.ActivityID || EmployeeID != slot.EmployeeID)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(slot);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SlotExists(slot.ProjectID, slot.WeekID, slot.ActivityID, slot.EmployeeID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(slot);
        }

        // GET: Slots/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? ProjectID, string WeekID, int? ActivityID, string EmployeeID)
        {
            if (ProjectID == null || WeekID == null || ActivityID == null || EmployeeID == null)
                return NotFound();
            var slot = await _context.Slot.FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.WeekID == WeekID && m.ActivityID == ActivityID && m.EmployeeID == EmployeeID);
            if (slot == null)
                return NotFound();
            return View(slot);
        }

        // POST: Slots/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int ProjectID, string WeekID, int ActivityID, string EmployeeID)
        {
            await slotsRepository.DeleteSlotAsync(_context, ProjectID, WeekID, ActivityID, EmployeeID);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Add(int? ProjectID, string WeekID, int? ActivityID)
        {
            if (ProjectID == null || WeekID == null || ActivityID == null)
            {
                return NotFound();
            }
            int numOfSimilarSlots = _context.Slot.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekID && e.ActivityID == ActivityID).Count();
            string EmployeeID = "_" + (numOfSimilarSlots + 1);
            slotsRepository.CreateSlot(_context, new SlotModel { ProjectID = ProjectID.Value, WeekID = WeekID, ActivityID = ActivityID.Value, EmployeeID = EmployeeID });
                await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Weeks", new { ProjectID = ProjectID.Value, WeekID = WeekID });
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Remove(int? ProjectID, string WeekID, int? ActivityID)
        {
            if (ProjectID == null || WeekID == null || ActivityID == null)
            {
                return NotFound();
            }
            var slots = _context.Slot.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekID && e.ActivityID == ActivityID);
            if (slots == null || slots.Count() == 0)
            {
                return NotFound();
            }
            var s = slots.Last();
            await slotsRepository.DeleteSlotAsync(_context, s.ProjectID, s.WeekID, s.ActivityID, s.EmployeeID);
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Weeks", new { ProjectID = ProjectID.Value, WeekID = WeekID });
        }

        private bool SlotExists(int ProjectID, string WeekID, int ActivityID, string EmployeeID)
        {
            return _context.Slot.Any(e => e.ProjectID == ProjectID && e.WeekID == WeekID && e.ActivityID == ActivityID && e.EmployeeID == EmployeeID);
        }
    }
}
