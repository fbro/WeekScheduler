using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;
using WeekScheduler.Models;
using WeekScheduler.ViewModels;

namespace WeekScheduler.Controllers
{
    public class TagsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TagsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Tags
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<TagModel> list;
            switch (sortLayout)
            {
                case "TagID_asc":
                    list = await _context.Tag.OrderBy(e => e.TagID).ToListAsync();
                    break;
                case "TagID_desc":
                    list = await _context.Tag.OrderByDescending(e => e.TagID).ToListAsync();
                    break;
                case "ActivityID_asc":
                    list = await _context.Tag.OrderBy(e => e.ActivityID).ToListAsync();
                    break;
                case "ActivityID_desc":
                    list = await _context.Tag.OrderByDescending(e => e.ActivityID).ToListAsync();
                    break;
                
                default:
                    list = await _context.Tag.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
                list = list.Where(e => e.TagID.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.ActivityID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            var viewModel = new IndexTagViewModel
            {
                TagModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: Tags/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(string TagID, int? ActivityID)
        {
            if (TagID == null || ActivityID == null)
            {
                return NotFound();
            }

            var tag = await _context.Tag
                .FirstOrDefaultAsync(m => m.TagID == TagID && m.ActivityID == ActivityID);
            if (tag == null)
            {
                return NotFound();
            }

            var activity = _context.Activity.Single(e => e.ActivityID == ActivityID);
            var tagViewModel = new DetailsTagViewModel
            {
                TagID = tag.TagID,
                ActivityID = tag.ActivityID,
                ActivityModel = activity,
            };

            return View(tagViewModel);
        }

        // GET: Tags/Create
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Create()
        {
            return View(new TagModel());
        }

        // POST: Tags/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Create(TagModel tag)
        {
            if (ModelState.IsValid)
            {
                await _context.Tag.AddAsync(tag);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(string TagID, int? ActivityID)
        {
            if (TagID == null || ActivityID == null)
                return NotFound();
            var tag = await _context.Tag.FindAsync(TagID, ActivityID);
            if (tag == null)
                return NotFound();
            return View(tag);
        }

        // POST: Tags/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(string TagID, int ActivityID, TagModel tag)
        {
            if (TagID != tag.TagID || ActivityID != tag.ActivityID)
                return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tag);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(tag.TagID, tag.ActivityID))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tag);
        }

        // GET: Tags/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(string TagID, int? ActivityID)
        {
            if (TagID == null || ActivityID == null)
                return NotFound();
            var tag = await _context.Tag.FirstOrDefaultAsync(m => m.TagID == TagID && m.ActivityID == ActivityID);
            if (tag == null)
                return NotFound();
            return View(tag);
        }

        // POST: Tags/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(string TagID, int ActivityID)
        {
            var tag = await _context.Tag.FindAsync(TagID, ActivityID);
            if (tag == null)
                return NotFound();
            _context.Tag.Remove(tag);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Add(string TagID, int? ActivityID)
        {
            if (TagID == null || ActivityID == null)
            {
                return NotFound();
            }
            _context.Tag.Add(new TagModel { TagID = TagID, ActivityID = ActivityID.Value });
            _context.SaveChanges();
            return RedirectToAction("Edit", "Activities", new { ActivityID = ActivityID.Value });
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Remove(string TagID, int? ActivityID)
        {
            if (TagID == null || ActivityID == null)
            {
                return NotFound();
            }
            var tag = _context.Tag.Find(TagID, ActivityID = ActivityID.Value);
            if (tag == null)
                return NotFound();
            _context.Tag.Remove(tag);
            _context.SaveChanges();
            return RedirectToAction("Edit", "Activities", new { ActivityID = ActivityID.Value });
        }

        private bool TagExists(string TagID, int ActivityID)
        {
            return _context.Tag.Any(e => e.TagID == TagID && e.ActivityID == ActivityID);
        }
    }
}
