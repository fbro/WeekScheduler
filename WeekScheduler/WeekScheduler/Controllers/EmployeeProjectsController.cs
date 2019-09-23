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
    public class EmployeeProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeProjectsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: EmployeeProjects
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<EmployeeProjectModel> list;
            switch (sortLayout)
            {
                case "ProjectID_asc":
                    list = await _context.EmployeeProject.OrderBy(e => e.ProjectID).ToListAsync();
                    break;
                case "ProjectID_desc":
                    list = await _context.EmployeeProject.OrderByDescending(e => e.ProjectID).ToListAsync();
                    break;
                case "EmployeeID_asc":
                    list = await _context.EmployeeProject.OrderBy(e => e.EmployeeID).ToListAsync();
                    break;
                case "EmployeeID_desc":
                    list = await _context.EmployeeProject.OrderByDescending(e => e.EmployeeID).ToListAsync();
                    break;
                default:
                    list = await _context.EmployeeProject.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
            {
                list = list.Where(e => e.ProjectID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.EmployeeID.Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModel = new IndexEmployeeProjectViewModel
            {
                EmployeeProjectModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: EmployeesInProject
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> EmployeesInProject(int? ProjectID)
        {
            if (ProjectID == null)
            {
                return NotFound();
            }

            var employeeProjectViewModel = new EmployeeProjectViewModel
            {
                Project = await _context.Project.FindAsync(ProjectID),
                EmployeesInProject = await _context.EmployeeProject.Where(e => e.ProjectID == ProjectID).ToListAsync()
            };
            return View(employeeProjectViewModel);
        }

        // GET: EmployeeProjects/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(int? ProjectID, string EmployeeID)
        {
            if (ProjectID == null || EmployeeID == null)
            {
                return NotFound();
            }

            var employeeProjectModel = await _context.EmployeeProject
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.EmployeeID == EmployeeID);
            if (employeeProjectModel == null)
            {
                return NotFound();
            }

            return View(employeeProjectModel);
        }

        // GET: EmployeeProjects/Create
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Create(int? ProjectID = null)
        {
            if(ProjectID == null)
            {
                return View();
            }
            else
            {
                return View(new EmployeeProjectModel { ProjectID = ProjectID.Value, EmployeeID = _context.Employee.Where(e => !_context.EmployeeProject.Where(d => d.ProjectID == ProjectID && d.EmployeeID == e.EmployeeID).Any()).FirstOrDefault()?.EmployeeID ?? "" });
            }
        }

        // POST: EmployeeProjects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Create(EmployeeProjectModel employeeProjectModel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(employeeProjectModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employeeProjectModel);
        }

        // GET: EmployeeProjects/Edit/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Edit(int? ProjectID, string EmployeeID)
        {
            if (ProjectID == null || EmployeeID == null)
            {
                return NotFound();
            }

            var employeeProjectModel = await _context.EmployeeProject.FindAsync(ProjectID, EmployeeID);
            if (employeeProjectModel == null)
            {
                return NotFound();
            }
            return View(employeeProjectModel);
        }

        // POST: EmployeeProjects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Edit(int ProjectID, string EmployeeID, EmployeeProjectModel employeeProjectModel)
        {
            if (ProjectID != employeeProjectModel.ProjectID || EmployeeID != employeeProjectModel.EmployeeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employeeProjectModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeProjectModelExists(employeeProjectModel.ProjectID))
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
            return View(employeeProjectModel);
        }

        // GET: EmployeeProjects/Delete/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Delete(int? ProjectID, string EmployeeID)
        {
            if (ProjectID == null || EmployeeID == null)
            {
                return NotFound();
            }

            var employeeProjectModel = await _context.EmployeeProject
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.EmployeeID == EmployeeID);
            if (employeeProjectModel == null)
            {
                return NotFound();
            }

            return View(employeeProjectModel);
        }

        // POST: EmployeeProjects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> DeleteConfirmed(int ProjectID, string EmployeeID)
        {
            _context.EmployeeProject.Remove(await _context.EmployeeProject.FindAsync(ProjectID, EmployeeID));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Add(int? ProjectID, string WeekID, string EmployeeID)
        {
            if (ProjectID == null || WeekID == null || EmployeeID == null)
            {
                return NotFound();
            }
            _context.EmployeeProject.Add(new EmployeeProjectModel { ProjectID = ProjectID.Value, EmployeeID = EmployeeID });
            _context.SaveChanges();
            return RedirectToAction("Edit", "Weeks", new { ProjectID = ProjectID.Value, WeekID = WeekID });
        }

        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Remove(int? ProjectID, string WeekID, string EmployeeID)
        {
            if (ProjectID == null || WeekID == null || EmployeeID == null)
            {
                return NotFound();
            }
            var employeeProject = _context.EmployeeProject.Find(ProjectID.Value, EmployeeID);
            if (employeeProject == null)
            {
                return NotFound();
            }
            _context.EmployeeProject.Remove(employeeProject);
            _context.SaveChanges();
            return RedirectToAction("Edit", "Weeks", new { ProjectID = ProjectID.Value, WeekID = WeekID });
        }

        private bool EmployeeProjectModelExists(int id)
        {
            return _context.EmployeeProject.Any(e => e.ProjectID == id);
        }
    }
}
