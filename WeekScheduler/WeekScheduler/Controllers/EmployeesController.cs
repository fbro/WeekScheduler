using System;
using System.Collections.Generic;
using System.Drawing;
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
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeesRepository _employeesRepository = new EmployeesRepository();

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Employees
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<EmployeeModel> list;
            switch (sortLayout)
            {
                case "EmployeeID_asc":
                    list = await _context.Employee.OrderBy(e => e.EmployeeID).ToListAsync();
                    break;
                case "EmployeeID_desc":
                    list = await _context.Employee.OrderByDescending(e => e.EmployeeID).ToListAsync();
                    break;
                case "Name_asc":
                    list = await _context.Employee.OrderBy(e => e.Name).ToListAsync();
                    break;
                case "Name_desc":
                    list = await _context.Employee.OrderByDescending(e => e.Name).ToListAsync();
                    break;
                case "WeeklyWorkHours_asc":
                    list = await _context.Employee.OrderBy(e => e.WeeklyWorkHours).ToListAsync();
                    break;
                case "WeeklyWorkHours_desc":
                    list = await _context.Employee.OrderByDescending(e => e.WeeklyWorkHours).ToListAsync();
                    break;
                case "NumOfWeeklyCounseling_asc":
                    list = await _context.Employee.OrderBy(e => e.NumOfWeeklyCounseling).ToListAsync();
                    break;
                case "NumOfWeeklyCounseling_desc":
                    list = await _context.Employee.OrderByDescending(e => e.NumOfWeeklyCounseling).ToListAsync();
                    break;
                case "Color_asc":
                    list = await _context.Employee.OrderBy(e => e.Color).ToListAsync();
                    break;
                case "Color_desc":
                    list = await _context.Employee.OrderByDescending(e => e.Color).ToListAsync();
                    break;
                default:
                    list = await _context.Employee.ToListAsync();
                    break;
            }
            if (filterString != null && filterString != "")
            {
                list = list.Where(e => e.EmployeeID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.Name.Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.WeeklyWorkHours.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)
                || e.NumOfWeeklyCounseling.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            var viewModel = new IndexEmployeeViewModel
            {
                EmployeeModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: Employees/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(string EmployeeID)
        {
            if (EmployeeID == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeID == EmployeeID);
            if (employee == null)
            {
                return NotFound();
            }
            var activityRecords = _context.ActivityRecord.Where(e => e.EmployeeID == EmployeeID).ToList();
            var employeeViewModel = new DetailsEmployeeViewModel
            {
                EmployeeID = employee.EmployeeID,
                Name = employee.Name,
                WeeklyWorkHours = employee.WeeklyWorkHours,
                NumOfWeeklyCounseling = employee.NumOfWeeklyCounseling,
                Color = employee.Color,
                ActivityRecords = activityRecords
            };
            return View(employeeViewModel);
        }

        // GET: Employees/Create
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Create()
        {
            var employeeModel = new EmployeeModel
            {
                Color = _employeesRepository.GetFreeColor(_context.Employee.Select(e => e.Color).ToList())
            };
            return View(employeeModel);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Create(EmployeeModel employee)
        {
            if (ModelState.IsValid)
            {
                if (employee.Color == null)
                    employee.Color = _employeesRepository.GetFreeColor(_context.Employee.Select(e => e.Color).ToList());
                _context.Add(employee);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(employee);
        }

        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(string EmployeeID)
        {
            if (EmployeeID == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee.FindAsync(EmployeeID);
            if (employee == null)
            {
                return NotFound();
            }
            return View(employee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(string EmployeeID, EmployeeModel employee)
        {
            if (EmployeeID != employee.EmployeeID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeeExists(employee.EmployeeID))
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
            return View(employee);
        }

        // GET: Employees/Delete/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Delete(string EmployeeID)
        {
            if (EmployeeID == null)
            {
                return NotFound();
            }

            var employee = await _context.Employee
                .FirstOrDefaultAsync(m => m.EmployeeID == EmployeeID);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> DeleteConfirmed(string EmployeeID)
        {
            _context.Employee.Remove(await _context.Employee.FindAsync(EmployeeID).ConfigureAwait(false));
            _context.EmployeeProject.RemoveRange(_context.EmployeeProject.Where(e => e.EmployeeID == EmployeeID));
            _context.ActivityRecord.RemoveRange(_context.ActivityRecord.Where(e => e.EmployeeID == EmployeeID));
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EmployeeExists(string EmployeeID)
        {
            return _context.Employee.Any(e => e.EmployeeID == EmployeeID);
        }
    }
}
