using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ProjectsRepository _ProjectsRepository;
        private readonly WeeksRepository _WeeksRepository;

        public ProjectsController(ApplicationDbContext context)
        {
            _context = context;
            _ProjectsRepository = new ProjectsRepository(_context);
            _WeeksRepository = new WeeksRepository(_context.Rule.ToList());
        }

        // GET: Projects
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Project.ToListAsync());
        }

        // GET: Projects/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(int? ProjectID)
        {
            if (ProjectID == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, Manager")]
        public IActionResult Create()
        {
            return View(new ProjectModel());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Create(ProjectModel project)
        {
            if (ModelState.IsValid)
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? ProjectID)
        {
            if (ProjectID == null)
            {
                return NotFound();
            }

            var project = await _context.Project.FindAsync(ProjectID);
            if (project == null)
            {
                return NotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int ProjectID, ProjectModel project)
        {
            if (ProjectID != project.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectID))
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
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(int? ProjectID)
        {
            if (ProjectID == null)
            {
                return NotFound();
            }

            var project = await _context.Project
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(int ProjectID)
        {
            await _ProjectsRepository.DeleteProjectAsync(_context, ProjectID);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult ListProjectWeeks(int? ProjectID, string FromWeekID = "", string ToWeekID = "")
        {
            if (ProjectID == null)
            {
                return NotFound();
            }
            // TODO add regex check on from and to week id

            var weeks = _context.Week.Where(e => e.ProjectID == ProjectID);
            var weekComparer = new CompareWeekModel();
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (FromWeekID != null && FromWeekID != "" && WeekIDRegex.IsMatch(FromWeekID)) // FromWeekID is valid
            {
                if (ToWeekID != null && ToWeekID != "" && WeekIDRegex.IsMatch(ToWeekID)) // FromWeekID and ToWeekID is valid
                {
                }
                else // FromWeekID is valid and ToWeekID is invalid
                {
                    var fromDate = WeekModel.GetMondayFromWeekID(FromWeekID);
                    ToWeekID = WeekModel.GetIso8601WeekOfYear(fromDate.AddDays(21)) + "-" + DateTime.Now.Year;
                }
            }
            else // FromWeekID is invalid
            {
                if(ToWeekID != null && ToWeekID != "" && WeekIDRegex.IsMatch(ToWeekID)) // FromWeekID is invalid and ToWeekID is valid
                {
                    var toDate = WeekModel.GetMondayFromWeekID(ToWeekID);
                    FromWeekID = WeekModel.GetIso8601WeekOfYear(toDate.AddDays(-21)) + "-" + DateTime.Now.Year;
                }
                else // FromWeekID is invalid and ToWeekID is invalid
                {
                    FromWeekID = WeekModel.GetIso8601WeekOfYear(DateTime.Now) + "-" + DateTime.Now.Year;
                    ToWeekID = WeekModel.GetIso8601WeekOfYear(DateTime.Now.AddDays(21)) + "-" + DateTime.Now.Year;
                }
            }
            weeks = weeks.Where(e => weekComparer.CompareWeekIDs(e.WeekID, ToWeekID) <= 0 && weekComparer.CompareWeekIDs(e.WeekID, FromWeekID) >= 0);

            var weekList = weeks.ToList();
            weekList.Sort(new CompareWeekModel());
            var weeksView = new List<DetailsWeekViewModel>();
            foreach (var week in weekList)
            {
                weeksView.Add(_WeeksRepository.PrepareWeekViewModel(_context, week));
            }
            var ListProjectWeekViewModel = new ListProjectWeekViewModel
            {
                ProjectID = ProjectID.Value,
                FromWeekID = FromWeekID,
                ToWeekID = ToWeekID,
                WeekViewModels = weeksView
            };
            return View(ListProjectWeekViewModel);
        }

        public bool ProjectExists(int ProjectID)
        {
            return _context.Project.Any(e => e.ProjectID == ProjectID);
        }
    }
}
