using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public class WeeksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly WeeksRepository _WeeksRepository;
        private readonly SlotsRepository _slotsRepository = new SlotsRepository();

        public WeeksController(ApplicationDbContext context)
        {
            _context = context;
            _WeeksRepository = new WeeksRepository(_context.Rule.ToList());
        }

        // GET: Weeks
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Index(string sortLayout = "", string filterString = "")
        {
            List<WeekModel> list;
            switch (sortLayout)
            {
                case "ProjectID_asc":
                    list = await _context.Week.OrderBy(e => e.ProjectID).ToListAsync();
                    break;
                case "ProjectID_desc":
                    list = await _context.Week.OrderByDescending(e => e.ProjectID).ToListAsync();
                    break;
                case "WeekID_asc":
                    list = await _context.Week.ToListAsync();
                    list.Sort(new CompareWeekModel(true));
                    break;
                case "WeekID_desc":
                    list = await _context.Week.ToListAsync();
                    list.Sort(new CompareWeekModel(false));
                    break;
                case "FirstDayOfTheWeek_asc":
                    list = await _context.Week.OrderBy(e => e.FirstDayOfTheWeek).ToListAsync();
                    break;
                case "FirstDayOfTheWeek_desc":
                    list = await _context.Week.OrderByDescending(e => e.FirstDayOfTheWeek).ToListAsync();
                    break;
                default:
                    list = await _context.Week.ToListAsync();
                    list.Sort(new CompareWeekModel());
                    break;
            }
            if (filterString != null && filterString != "")
                list = list.Where(e => e.ProjectID.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase) 
                || e.WeekID.Contains(filterString, StringComparison.OrdinalIgnoreCase) 
                || e.FirstDayOfTheWeek.ToString().Contains(filterString, StringComparison.OrdinalIgnoreCase)).ToList();
            var viewModel = new IndexWeekViewModel
            {
                WeekModels = list,
                SortLayout = sortLayout,
                FilterString = filterString
            };
            return View(viewModel);
        }

        // GET: Weeks/Details/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Details(int? ProjectID, string WeekID)
        {
            if (ProjectID == null || WeekID == null)
            {
                return NotFound();
            }
            var week = await _context.Week
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.WeekID == WeekID);
            if (week == null)
            {
                return NotFound();
            }
            var weekViewModel = _WeeksRepository.PrepareWeekViewModel(_context, week);
            return View(weekViewModel);
        }

        // GET: Weeks/Create
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult Create(int? ProjectID = 0)
        {
            var templateWeek = _context.Week.Where(e => e.ProjectID == ProjectID).LastOrDefault(); // take last generated week
            var createWeekViewModel = new CreateWeekViewModel();
            if(templateWeek != null)
            {
                createWeekViewModel.ProjectID = templateWeek.ProjectID;
                createWeekViewModel.WeekTemplateID = templateWeek.WeekID;
                createWeekViewModel.Notes1Monday = templateWeek.Notes1Monday;
                createWeekViewModel.Notes2Tuesday = templateWeek.Notes2Tuesday;
                createWeekViewModel.Notes3Wednesday = templateWeek.Notes3Wednesday;
                createWeekViewModel.Notes4Thursday = templateWeek.Notes4Thursday;
                createWeekViewModel.Notes5Friday = templateWeek.Notes5Friday;
                createWeekViewModel.Notes6Saturday = templateWeek.Notes6Saturday;
                createWeekViewModel.Notes7Sunday = templateWeek.Notes7Sunday;
            }
            var foundFreeWeek = false;
            var weekCount = Convert.ToInt16(createWeekViewModel.WeekID.Substring(0, 2));
            var year = createWeekViewModel.WeekID.Substring(3, 4);
            while (!foundFreeWeek)
            {
                foundFreeWeek = !_context.Week.Any(e => e.ProjectID == ProjectID && e.WeekID == (weekCount < 10 ? "0" + weekCount : "" + weekCount) + "-" + year);
                if (!foundFreeWeek)
                    weekCount++;
            }
            createWeekViewModel.WeekID = (weekCount < 10 ? "0" + weekCount : "" + weekCount) + "-" + year;
            return View(createWeekViewModel);
        }

        // POST: Weeks/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Create(CreateWeekViewModel createWeekViewModel)
        {
            // with this call, a week can be created from scratch or with some slots already referencing this week
            if (ModelState.IsValid)
            {
                var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
                if (!WeekIDRegex.IsMatch(createWeekViewModel.WeekID))
                    return BadRequest();
                var WeekIDInt = Int32.Parse(createWeekViewModel.WeekID.Substring(0, 2));
                if (WeekIDInt <= 0 || WeekIDInt > 52)
                    return BadRequest();
                if (WeekExists(createWeekViewModel.ProjectID, createWeekViewModel.WeekID)
                    || !_context.Project.Any(e => e.ProjectID == createWeekViewModel.ProjectID))
                    return BadRequest();

                var projectID = createWeekViewModel.ProjectID;
                var weekID = createWeekViewModel.WeekID;
                var weekSlots = _context.Slot.AsNoTracking().Where(e => e.ProjectID == projectID && e.WeekID == weekID).ToList();
                var weekTemplateSlots = _context.Slot.AsNoTracking().Where(e => e.ProjectID == projectID && e.WeekID == createWeekViewModel.WeekTemplateID).ToList();
                var activities = _context.Activity.ToList();

                var newSlots = new List<SlotModel>();
                foreach (var weekTemplateSlot in weekTemplateSlots.Where(e => !weekSlots.Exists(d => d.ProjectID == e.ProjectID && d.ActivityID == e.ActivityID && d.EmployeeID == e.EmployeeID)))
                {
                    if (!activities.Single(e => e.ActivityID == weekTemplateSlot.ActivityID).Name.Contains("public holiday")) // do not clone public holidays
                    {
                        var dubletCount = newSlots.Where(e => e.ProjectID == projectID && e.WeekID == weekID && e.ActivityID == weekTemplateSlot.ActivityID).Count();
                        var employeeID = "_" + (dubletCount + 1);
                        newSlots.Add(new SlotModel { ProjectID = projectID, WeekID = weekID, ActivityID = weekTemplateSlot.ActivityID, EmployeeID = employeeID });
                    }
                }
                if (!_context.Week.Any(e => e.WeekID.Substring(3, 4).Equals(weekID.Substring(3, 4)) && e.ProjectID == projectID)) // any week with a new year in its id?
                {
                    _WeeksRepository.WeightDecay(_context.ActivityRecord.ToList()); // divide all weights by two every time a new year of work is recorded
                }
                await _context.Slot.AddRangeAsync(newSlots);
                await _context.Week.AddAsync(new WeekModel
                {
                    ProjectID = projectID,
                    WeekID = weekID,
                    FirstDayOfTheWeek = WeekModel.GetMondayFromWeekID(createWeekViewModel.WeekID),
                    Notes1Monday = createWeekViewModel.Notes1Monday,
                    Notes2Tuesday = createWeekViewModel.Notes2Tuesday,
                    Notes3Wednesday = createWeekViewModel.Notes3Wednesday,
                    Notes4Thursday = createWeekViewModel.Notes4Thursday,
                    Notes5Friday = createWeekViewModel.Notes5Friday,
                    Notes6Saturday = createWeekViewModel.Notes6Saturday,
                    Notes7Sunday = createWeekViewModel.Notes7Sunday,

                });
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { projectID, weekID });
            }
            return View(createWeekViewModel);
        }

        // GET: Weeks/Edit/5
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int? ProjectID, string WeekID)
        {
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (ProjectID == null || WeekID == null || !WeekIDRegex.IsMatch(WeekID))
                return BadRequest();
            var WeekIDInt = Int32.Parse(WeekID.Substring(0, 2));
            if (WeekIDInt <= 0 || WeekIDInt > 52)
                return BadRequest();

            var week = await _context.Week.FindAsync(ProjectID, WeekID);
            if (week == null)
            {
                return NotFound();
            }
            var employees = _context.Employee.ToList();
            var employeeProjects = _context.EmployeeProject.Where(e => e.ProjectID == ProjectID).ToList();
            var activitiesWithSlots = new List<ActivityWithSlots>();
            var activities = _context.Activity.ToList();
            var slots = _context.Slot.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekID).ToList();
            foreach (var activity in activities)
            {
                var activityWithSlots = new ActivityWithSlots
                {
                    Activity = activity,
                    Slots = new List<SlotModel>(),
                };
                foreach (var slot in slots)
                    if(slot.ActivityID == activity.ActivityID)
                        activityWithSlots.Slots.Add(slot);
                activitiesWithSlots.Add(activityWithSlots);
            }
            var editWeekViewModel = new EditWeekViewModel
            {
                ProjectID = week.ProjectID,
                WeekID = week.WeekID,
                FirstDayOfTheWeek = week.FirstDayOfTheWeek,
                Notes1Monday = week.Notes1Monday,
                Notes2Tuesday = week.Notes2Tuesday,
                Notes3Wednesday = week.Notes3Wednesday,
                Notes4Thursday = week.Notes4Thursday,
                Notes5Friday = week.Notes5Friday,
                Notes6Saturday = week.Notes6Saturday,
                Notes7Sunday = week.Notes7Sunday,

                Employees = employees,
                EmployeeProjects = employeeProjects,
                ActivitiesWithSlots = activitiesWithSlots,
            };
            return View(editWeekViewModel);
        }

        // POST: Weeks/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager")]
        public async Task<IActionResult> Edit(int ProjectID, string WeekID, EditWeekViewModel editWeekViewModel)
        {
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (!WeekIDRegex.IsMatch(WeekID))
                return BadRequest();
            var WeekIDInt = Int32.Parse(WeekID.Substring(0, 2));
            if (WeekIDInt <= 0 || WeekIDInt > 52)
                return BadRequest();

            if (ModelState.IsValid)
            {
                var week = await _context.Week.FindAsync(ProjectID, WeekID);
                if(week == null)
                    return NotFound();
                week.Notes1Monday = editWeekViewModel.Notes1Monday;
                week.Notes2Tuesday = editWeekViewModel.Notes2Tuesday;
                week.Notes3Wednesday = editWeekViewModel.Notes3Wednesday;
                week.Notes4Thursday = editWeekViewModel.Notes4Thursday;
                week.Notes5Friday = editWeekViewModel.Notes5Friday;
                week.Notes6Saturday = editWeekViewModel.Notes6Saturday;
                week.Notes7Sunday = editWeekViewModel.Notes7Sunday;
                _context.Update(week);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { ProjectID, WeekID });
            }
            return View(nameof(Index));
        }

        // GET: Weeks/Delete/5
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> Delete(int? ProjectID, string WeekID)
        {
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (ProjectID == null || WeekID == null || !WeekIDRegex.IsMatch(WeekID))
                return BadRequest();
            var WeekIDInt = Int32.Parse(WeekID.Substring(0, 2));
            if (WeekIDInt <= 0 || WeekIDInt > 52)
                return BadRequest();

            var week = await _context.Week
                .FirstOrDefaultAsync(m => m.ProjectID == ProjectID && m.WeekID == WeekID);
            if (week == null)
            {
                return NotFound();
            }

            return View(week);
        }

        // POST: Weeks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> DeleteConfirmed(int ProjectID, string WeekID)
        {
            await _WeeksRepository.DeleteWeekAsync(_context, ProjectID, WeekID);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> CalculateWeek(int? ProjectID, string WeekID)
        {
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (ProjectID == null || WeekID == null || !WeekIDRegex.IsMatch(WeekID))
                return BadRequest();
            var WeekIDInt = Int32.Parse(WeekID.Substring(0, 2));
            if(WeekIDInt <= 0 || WeekIDInt > 52)
                return BadRequest();

            // prepare data for CalculateWeek
            var project = _context.Project.Single(e => e.ProjectID == ProjectID);
            var week = _context.Week.Single(e => e.ProjectID == ProjectID && e.WeekID == WeekID);
            var lastWeekNumber = WeekIDInt - 1;
            var lastWeekNumberString = lastWeekNumber < 10 ? "0" + lastWeekNumber : "" + lastWeekNumber;
            var lastWeekID = lastWeekNumberString + "-" + WeekID.Substring(3, 4);
            var activities = _context.Activity.ToList();
            var activityForSundayNight = activities.Single(e => e.DayOfWeek == DaySlot.SUNDAY && e.TimeOfDay == TimeSlot.NIGHT);
            var employeeIDForLastSundayNight = _context.Slot.SingleOrDefault(e => e.ProjectID == ProjectID && e.WeekID == lastWeekID && e.ActivityID == activityForSundayNight.ActivityID)?.EmployeeID ?? "";
            var slotsInYear = _context.Slot.Where(e => e.WeekID.Substring(3, 4).Equals(week.WeekID.Substring(3, 4)));
            var weekSlots = slotsInYear.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekID).ToList();
            var clonedWeekSlots = new List<SlotModel>();
            foreach (var weekSlot in weekSlots)
                clonedWeekSlots.Add(new SlotModel { ProjectID = weekSlot.ProjectID, WeekID = weekSlot.WeekID, ActivityID = weekSlot.ActivityID, EmployeeID = weekSlot.EmployeeID });
            var employeeProjects = _context.EmployeeProject.Where(e => e.ProjectID == ProjectID).ToList();
            var employeesInProject = _context.Employee.Where(e => employeeProjects.Where(d => d.EmployeeID == e.EmployeeID).Any()).ToList();
            var activityRecordsForEmployeesInProject = _context.ActivityRecord.Where(e => employeesInProject.Where(d => d.EmployeeID == e.EmployeeID).Any()).ToList();

            var numOfCreatedWeeksInThisYear = _context.Week.Where(e => e.WeekID.Substring(3, 4).Equals(WeekID.Substring(3, 4))).Count();
            var EmployeeInfo = new List<EmployeeInfo>();

            foreach (var employeeInProject in employeesInProject)
            {
                var weeksSum = 0.0; // sum from all other weeks in this year on this employee
                var weeksExpected = employeeInProject.WeeklyWorkHours * numOfCreatedWeeksInThisYear;
                foreach (var employeeSlotsInYear in slotsInYear.Where(e => e.EmployeeID == employeeInProject.EmployeeID && !e.WeekID.Equals(WeekID)))
                {
                    var activity = activities.Single(e => e.ActivityID == employeeSlotsInYear.ActivityID);
                    weeksSum += activity.Hours;
                }
                EmployeeInfo.Add(new EmployeeInfo
                {
                    Employee = employeeInProject,
                    ActivityRecords = activityRecordsForEmployeesInProject.Where(e => e.EmployeeID == employeeInProject.EmployeeID).ToList(),
                    WorkHourStats = new WorkHourStats
                    {
                        WeeksSum = weeksSum,
                        WeekExpected = weeksExpected - weeksSum,
                        WeeksExpected = weeksExpected,
                    }
                });
            }

            // calculate week
            List<SlotModel> newWeekSlots = _WeeksRepository.CalculateWeek(project, week, employeeIDForLastSundayNight, clonedWeekSlots, activities, EmployeeInfo);
            
            // save data from CalculateWeek
            var activityRecordsToUpdate = new List<ActivityRecordModel>();
            var newActivityRecords = new List<ActivityRecordModel>();
            var newWeekSlotsToAdd = newWeekSlots.Where(e => weekSlots.FirstOrDefault(d => e.ActivityID == d.ActivityID && e.EmployeeID == d.EmployeeID) == null); // do not add the slot if it already exists
            foreach (var slot in newWeekSlotsToAdd.Where(e => !e.EmployeeID.StartsWith("_"))) // add to or create ActivityRecords from newWeekSlots, ignore placeholders
            {
                var activityRecord = _context.ActivityRecord.SingleOrDefault(e => e.ActivityID == slot.ActivityID && e.EmployeeID == slot.EmployeeID);
                if (activityRecord != null)
                {
                    activityRecord.Weight++;
                    activityRecordsToUpdate.Add(activityRecord);
                }
                else
                {
                    activityRecord = newActivityRecords.SingleOrDefault(e => e.ActivityID == slot.ActivityID && e.EmployeeID == slot.EmployeeID);
                    if (activityRecord != null)
                        activityRecord.Weight++;
                    else
                        newActivityRecords.Add(new ActivityRecordModel { ActivityID = slot.ActivityID, EmployeeID = slot.EmployeeID, Weight = 1, });
                }
            }
            var weekSlotsToRemove = weekSlots.Where(e => newWeekSlots.FirstOrDefault(d => e.ActivityID == d.ActivityID && e.EmployeeID == d.EmployeeID) == null);
            var activityRecordsToDelete = new List<ActivityRecordModel>();
            foreach (var weekSlotToRemove in weekSlotsToRemove) // also remove/substract old ActivityRecord
            {
                var activityRecordToDecrement = _context.ActivityRecord.SingleOrDefault(e => e.ActivityID == weekSlotToRemove.ActivityID && e.EmployeeID == weekSlotToRemove.EmployeeID);
                if(activityRecordToDecrement != null && activityRecordToDecrement.Weight <= 1)
                    activityRecordsToDelete.Add(activityRecordToDecrement);
                else if(activityRecordToDecrement != null)
                {
                    activityRecordToDecrement.Weight--;
                    activityRecordsToUpdate.Add(activityRecordToDecrement);
                }
            }
            _context.ActivityRecord.RemoveRange(activityRecordsToDelete);
            _context.ActivityRecord.UpdateRange(activityRecordsToUpdate);
            await _context.ActivityRecord.AddRangeAsync(newActivityRecords);
            _context.Slot.RemoveRange(weekSlotsToRemove); // remove all old slots on the week
            await _context.Slot.AddRangeAsync(newWeekSlotsToAdd); // add all the new slots

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { ProjectID, WeekID });
        }

        // GET: Weeks/CreateRangeOfWeeks
        [Authorize(Roles = "Admin, Manager, Member")]
        public IActionResult CreateRangeOfWeeks(int? ProjectID)
        {
            if (ProjectID == null)
            {
                return NotFound();
            }
            var templateWeek = _context.Week.LastOrDefault(); // take last generated week
            var createRangeOfWeeksViewModel = new CreateRangeOfWeeksViewModel();
            if (templateWeek != null)
            {
                createRangeOfWeeksViewModel.ProjectID = ProjectID.Value;
                createRangeOfWeeksViewModel.WeekTemplateID = templateWeek.WeekID;
            }
            var foundFreeWeek = false;
            var weekCount = Convert.ToInt16(createRangeOfWeeksViewModel.FromWeekID.Substring(0, 2));
            var year = createRangeOfWeeksViewModel.FromWeekID.Substring(3, 4);
            while (!foundFreeWeek)
            {
                foundFreeWeek = !_context.Week.Any(e => e.WeekID == (weekCount < 10 ? "0" + weekCount : "" + weekCount) + "-" + year);
                if (!foundFreeWeek)
                    weekCount++;
            }
            createRangeOfWeeksViewModel.FromWeekID = (weekCount < 10 ? "0" + weekCount : "" + weekCount) + "-" + year;
            weekCount++;
            createRangeOfWeeksViewModel.ToWeekID = (weekCount < 10 ? "0" + weekCount : "" + weekCount) + "-" + year;
            return View(createRangeOfWeeksViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, Manager, Member")]
        public async Task<IActionResult> CreateRangeOfWeeks(int? ProjectID, string WeekTemplateID, string fromWeekID, string toWeekID)
        {
            // this method requires that the range of weeks does not have any history - they must not exist prior to this call
            var WeekIDRegex = new Regex(@"^\d\d-\d\d\d\d$");
            if (ProjectID == null || WeekTemplateID == null || !WeekIDRegex.IsMatch(WeekTemplateID) || fromWeekID == null || !WeekIDRegex.IsMatch(fromWeekID) || toWeekID == null || !WeekIDRegex.IsMatch(toWeekID))
                return NotFound();
            var fromWeekIDInt = Int32.Parse(fromWeekID.Substring(0, 2));
            var toWeekIDInt = Int32.Parse(toWeekID.Substring(0, 2));
            if (!fromWeekID.Substring(2, 5).Equals(toWeekID.Substring(2, 5)) || fromWeekIDInt > toWeekIDInt || fromWeekIDInt <= 0 || toWeekIDInt > 52) // must be within the same year and from < to and not smaller than 0 or higher than 52
                return NotFound();
            for (int i = 0; (fromWeekIDInt + i) <= toWeekIDInt; i++)
            {
                var weekNumber = fromWeekIDInt + i;
                var weekNumberString = weekNumber < 10 ? "0" + weekNumber : "" + weekNumber;
                var weekID = weekNumberString + "-" + fromWeekID.Substring(3, 4);
                if (WeekExists(ProjectID.Value, weekID))
                    return BadRequest();
            }

            // prepare data for CreateRangeOfWeeks
            var project = _context.Project.Single(e => e.ProjectID == ProjectID);
            var weekTemplate = _context.Week.Single(e => e.ProjectID == ProjectID && e.WeekID == WeekTemplateID);
            var lastWeekNumber = fromWeekIDInt - 1;
            var lastWeekNumberString = lastWeekNumber < 10 ? "0" + lastWeekNumber : "" + lastWeekNumber;
            var lastWeekID = lastWeekNumberString + "-" + fromWeekID.Substring(3, 4);
            var activities = _context.Activity.ToList();
            var tags = _context.Tag.ToList();
            var activityForSundayNight = activities.Single(e => e.DayOfWeek == DaySlot.SUNDAY && e.TimeOfDay == TimeSlot.NIGHT);
            var employeeIDForLastSundayNight = _context.Slot.SingleOrDefault(e => e.ProjectID == ProjectID && e.WeekID == lastWeekID && e.ActivityID == activityForSundayNight.ActivityID)?.EmployeeID ?? "";
            var slotsInYear = _context.Slot.Where(e => e.WeekID.Substring(3, 4).Equals(fromWeekID.Substring(3, 4))).ToList();
            var templateSlots = _context.Slot.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekTemplateID).ToList();
            var employeeProjects = _context.EmployeeProject.Where(e => e.ProjectID == ProjectID).ToList();
            var employeesInProject = _context.Employee.Where(e => employeeProjects.Where(d => d.EmployeeID == e.EmployeeID).Any()).ToList();
            var activityRecordsForEmployeesInProject = _context.ActivityRecord.Where(e => employeesInProject.Where(d => d.EmployeeID == e.EmployeeID).Any()).ToList();
            var numOfCreatedWeeksInThisYear = _context.Week.Where(e => e.WeekID.Substring(3, 4).Equals(fromWeekID.Substring(3, 4))).Count();

            if (!_context.Week.Any(e => e.WeekID.Substring(3, 4).Equals(fromWeekID.Substring(3, 4)) && e.ProjectID == ProjectID)) // any week with a new year in its id?
            {
                _WeeksRepository.WeightDecay(_context.ActivityRecord.ToList()); // divide all weights by two every time a new year of work is recorded
            }

            var newWeeks = _WeeksRepository.CreateRangeOfWeeks
                (
                project, 
                weekTemplate, 
                fromWeekID, 
                toWeekID, 
                employeeIDForLastSundayNight, 
                templateSlots, 
                activities, 
                tags,
                employeesInProject, 
                activityRecordsForEmployeesInProject,
                numOfCreatedWeeksInThisYear,
                slotsInYear,
                out List<SlotModel> newWeeksSlots
                );

            // save data from CreateRangeOfWeeks
            await _context.Week.AddRangeAsync(newWeeks);
            var activityRecordsToUpdate = new List<ActivityRecordModel>();
            var newActivityRecords = new List<ActivityRecordModel>();
            foreach (var slot in newWeeksSlots.Where(e => !e.EmployeeID.StartsWith('_')))
            {
                var activityRecord = _context.ActivityRecord.SingleOrDefault(e => e.ActivityID == slot.ActivityID && e.EmployeeID == slot.EmployeeID);
                if (activityRecord != null)
                {
                    activityRecord.Weight++;
                    activityRecordsToUpdate.Add(activityRecord);
                }
                else
                {
                    activityRecord = newActivityRecords.SingleOrDefault(e => e.ActivityID == slot.ActivityID && e.EmployeeID == slot.EmployeeID);
                    if (activityRecord != null)
                        activityRecord.Weight++;
                    else
                        newActivityRecords.Add(new ActivityRecordModel { ActivityID = slot.ActivityID, EmployeeID = slot.EmployeeID, Weight = 1, });
                }
            }
            _context.ActivityRecord.UpdateRange(activityRecordsToUpdate);
            await _context.ActivityRecord.AddRangeAsync(newActivityRecords);
            await _context.Slot.AddRangeAsync(newWeeksSlots);

            await _context.SaveChangesAsync();
            return RedirectToAction("ListProjectWeeks", "Projects", new { ProjectID = ProjectID });
        }

        private bool WeekExists(int ProjectID, string WeekID)
        {
            return _context.Week.Any(e => e.ProjectID == ProjectID && e.WeekID == WeekID);
        }
    }
}
