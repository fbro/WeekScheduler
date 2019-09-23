using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;
using WeekScheduler.Models;
using WeekScheduler.ViewModels;

namespace WeekScheduler.Repositories
{
    public class WeeksRepository
    {
        private readonly SlotsRepository _slotsRepository = new SlotsRepository();
        private readonly RulesRepository _rulesRepository;

        public WeeksRepository(List<RuleModel> rules)
        {
            _rulesRepository = new RulesRepository(rules);
        }

        public async Task DeleteWeekAsync(ApplicationDbContext _context, int ProjectID, string WeekID)
        {
            var week = await _context.Week.FindAsync(ProjectID, WeekID).ConfigureAwait(false);
            _context.Week.Remove(week);

            foreach (var slot in _context.Slot.Where(e => e.ProjectID == ProjectID && e.WeekID == WeekID).ToList())
            {
                await _slotsRepository.DeleteSlotAsync(_context, slot.ProjectID, slot.WeekID, slot.ActivityID, slot.EmployeeID).ConfigureAwait(false);
            }
        }

        public DetailsWeekViewModel PrepareWeekViewModel(ApplicationDbContext _context, WeekModel week)
        {
            var slotsInWeek = _context.Slot.Where(e => e.WeekID == week.WeekID).ToList();
            var allActivities = _context.Activity.ToList();
            var slotsJoinActivitiesInWeek = new List<SlotJoinActivity>();
            var typesInWeek = new List<string>();
            foreach (var slot in slotsInWeek)
            {
                var activity = allActivities.Where(e => e.ActivityID == slot.ActivityID).Single();
                var l = activity.Name.ToLower().Split().ToList();
                foreach (var d in Enum.GetNames(typeof(DaySlot))) { l.Remove(d.ToLower()); }
                foreach (var t in Enum.GetNames(typeof(TimeSlot))) { l.Remove(t.ToLower()); }
                var type = "";
                foreach (var s in l)
                    type += s + " ";
                type = type.Trim();
                typesInWeek.Add(type);
                typesInWeek = typesInWeek.Distinct().ToList();
                if (slot.EmployeeID.StartsWith('_')) // all placeholder EmployeeID should be displayed as --
                    slot.EmployeeID = "--";
                slotsJoinActivitiesInWeek.Add(new SlotJoinActivity { Activity = activity, Slot = slot, Type = type });
            }
            slotsJoinActivitiesInWeek = slotsJoinActivitiesInWeek.OrderBy(e => e.Slot.EmployeeID.Equals("--") ? 2 : 1).ToList(); // move placeholder EmployeeID to the right in week view
            var employeesWithWorkHourStats = new List<EmployeeWithWorkHourStatsViewModel>();
            var employeeProjects = _context.EmployeeProject.Where(e => e.ProjectID == week.ProjectID).ToList();
            var employees = _context.Employee.Where(e => employeeProjects.SingleOrDefault(d => d.EmployeeID == e.EmployeeID) != null).ToList();
            var numOfCreatedWeeksInThisYear = _context.Week.Where(e => e.WeekID.Substring(3, 4).Equals(week.WeekID.Substring(3, 4))).Count();
            foreach (var employee in employees)
            {
                var weekSum = 0.0;
                var weeksSum = 0.0;
                var weeksExpected = employee.WeeklyWorkHours * numOfCreatedWeeksInThisYear;
                foreach (var employeeSlotInWeek in slotsInWeek.Where(e => e.EmployeeID == employee.EmployeeID))
                {
                    var activity = allActivities.Single(e => e.ActivityID == employeeSlotInWeek.ActivityID);
                    weekSum += activity.Hours;
                }
                foreach (var employeeSlotInYear in _context.Slot.Where(e => e.EmployeeID == employee.EmployeeID && e.WeekID.Substring(3, 4).Equals(week.WeekID.Substring(3, 4))))
                {
                    var activity = allActivities.Single(e => e.ActivityID == employeeSlotInYear.ActivityID);
                    weeksSum += activity.Hours;
                }
                var workHourStats = new WorkHourStats
                {
                    WeekSum = weekSum, // WeekSum: get all slots in this week on this employee and join with activities - add work hours to get WeekSum
                    WeeksSum = weeksSum, // WeeksSum: get all slots on this employee and join with activities - add work hours to get WeeksSum
                    WeekExpected = (weeksExpected) - weeksSum, // WeekExpected: should be as close to zero as possible
                    WeeksExpected = weeksExpected, // WeeksExpected: is right now always employee.WeeklyWorkHours * number of weeks created in this year
                };
                var employeeWithWorkHourStatsViewModel = new EmployeeWithWorkHourStatsViewModel
                {
                    EmployeeID = employee.EmployeeID,
                    Name = employee.Name,
                    WeeklyWorkHours = employee.WeeklyWorkHours,
                    NumOfWeeklyCounseling = employee.NumOfWeeklyCounseling,
                    Color = employee.Color,
                    WorkHourStats = workHourStats,
                };
                employeesWithWorkHourStats.Add(employeeWithWorkHourStatsViewModel);
            }
            DetailsWeekViewModel weekViewModel = new DetailsWeekViewModel
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

                SlotsJoinActivities = slotsJoinActivitiesInWeek,
                Types = typesInWeek.Distinct().ToList(),
                EmployeesWithWorkHourStats = employeesWithWorkHourStats,
            };
            return weekViewModel;
        }

        public List<WeekModel> CreateRangeOfWeeks(
            ProjectModel project,
            WeekModel weekTemplate,
            string fromWeekID,
            string toWeekID,
            string employeeIDForLastSundayNight,
            List<SlotModel> templateSlots,
            List<ActivityModel> activities,
            List<TagModel> tags,
            List<EmployeeModel> employeesInProject,
            List<ActivityRecordModel> activityRecordsForEmployeesInProject,
            int numOfCreatedWeeksInThisYear,
            List<SlotModel> slotsInYear,
            out List<SlotModel> newWeeksSlots)
        {
            var EmployeesInProjectInfoInYear = new List<EmployeeInfo>();
            var FromWeekNumber = Int32.Parse(fromWeekID.Substring(0, 2));
            var ToWeekNumber = Int32.Parse(toWeekID.Substring(0, 2));
            var Year = fromWeekID.Substring(3, 4);
            var range = ToWeekNumber - FromWeekNumber;
            var newWeeks = new List<WeekModel>();
            newWeeksSlots = new List<SlotModel>();

            foreach (var employeeInProject in employeesInProject)
            {
                var weeksSum = 0.0; // sum from all other weeks in this year on this employee
                var weeksExpected = employeeInProject.WeeklyWorkHours * (numOfCreatedWeeksInThisYear + 1); // +1 because we havent created the first week yet
                foreach (var employeeSlotsInYear in slotsInYear.Where(e => e.EmployeeID == employeeInProject.EmployeeID && !e.WeekID.Equals(fromWeekID)))
                {
                    var activity = activities.Single(e => e.ActivityID == employeeSlotsInYear.ActivityID);
                    weeksSum += activity.Hours;
                }
                EmployeesInProjectInfoInYear.Add(new EmployeeInfo
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

            for (int i = 0; i <= range; i++) // each week
            {
                var newWeekNumber = FromWeekNumber + i;
                var newWeekNumberString = newWeekNumber < 10 ? "0" + newWeekNumber : "" + newWeekNumber;
                var newWeekID = newWeekNumberString + "-" + Year;
                var week = new WeekModel { ProjectID = project.ProjectID, WeekID = newWeekID, FirstDayOfTheWeek = WeekModel.GetMondayFromWeekID(newWeekID) };
                var weekSlots = new List<SlotModel>();
                foreach (var templateSlot in templateSlots)
                {
                    var activity = activities.Single(e => e.ActivityID == templateSlot.ActivityID);
                    var tagsInActivity = tags.Where(e => e.ActivityID == activity.ActivityID);
                    var holidayTag = tagsInActivity.SingleOrDefault(e => e.TagID.Contains("public holiday"));
                    if (holidayTag == null) // do not clone public holidays
                    {
                        var dubletCount = weekSlots.Where(e => e.ProjectID == week.ProjectID && e.WeekID == week.WeekID && e.ActivityID == templateSlot.ActivityID).Count();
                        var employeeID = "_" + (dubletCount + 1);
                        weekSlots.Add(new SlotModel { ProjectID = week.ProjectID, WeekID = week.WeekID, ActivityID = templateSlot.ActivityID, EmployeeID = employeeID });
                    }
                }
                newWeeks.Add(week);
                List<SlotModel> newWeekSlots = CalculateWeek(project, week, employeeIDForLastSundayNight, weekSlots, activities, EmployeesInProjectInfoInYear);
                newWeeksSlots.AddRange(newWeekSlots);

                // Add WeeksSum and WeeksExpected and recalc WeekExpected in WorkHourStats for each employee in EmployeeInProjectInfoInYear
                foreach (var EmployeeInProjectInfoInYear in EmployeesInProjectInfoInYear)
                {
                    var WHS = EmployeeInProjectInfoInYear.WorkHourStats;
                    WHS.WeeksExpected += EmployeeInProjectInfoInYear.Employee.WeeklyWorkHours; // just add another week
                    WHS.WeekExpected = WHS.WeeksExpected - WHS.WeeksSum; // recalc after another week was added
                }

                var sundayNightActivity = activities.Single(e => e.DayOfWeek == DaySlot.SUNDAY && e.TimeOfDay == TimeSlot.NIGHT);
                employeeIDForLastSundayNight = newWeekSlots.Single(e => e.ActivityID == sundayNightActivity.ActivityID).EmployeeID;
            }
            return newWeeks;
        }

        public List<SlotModel> CalculateWeek(
            ProjectModel project,
            WeekModel week, // week to be calculated on
            string employeeIDForLastSundayNight,
            List<SlotModel> weekSlots, // slots to get assigned an employee
            List<ActivityModel> activities, // detailed information about the weekSlot's corresponding activity
            List<EmployeeInfo> employeesInProjectInfoInYear)
        {
            // algorithm takes the slots in this week and gives them employee initials based on the list of employees that where passed in createWeekViewModel
            var slotsActivities = new List<Tuple<SlotModel, ActivityModel>>();
            foreach (var slot in weekSlots)
                slotsActivities.Add(
                    new Tuple<SlotModel, ActivityModel>(
                        new SlotModel
                        {
                            ProjectID = slot.ProjectID,
                            WeekID = slot.WeekID,
                            ActivityID = slot.ActivityID,
                            EmployeeID = "#" + slot.EmployeeID
                        },
                        activities.Where(e => e.ActivityID == slot.ActivityID).Single())
                    );
            _rulesRepository.Rule8(project, week, activities, employeesInProjectInfoInYear, slotsActivities); // public holiday
            while (true) // loop to make sure that the one employee with the single most important activity is assigned first - breaks when no new best candidate is found
            {
                Tuple<SlotModel, ActivityModel> bestSlotActivity = null;
                EmployeeInfo bestEmployeeInfo = null;
                var bestWeight = int.MinValue;
                foreach (var employeeInfo in employeesInProjectInfoInYear.Where(e => e.WorkHourStats.WeekExpected > e.WorkHourStats.WeekSum)?.ToList() ?? new List<EmployeeInfo>()) // loop through each employee that is not full with workhours
                    foreach (var slotActivity in slotsActivities.Where(e => e.Item1.EmployeeID == "" || e.Item1.EmployeeID.StartsWith('#'))?.OrderBy(o => o.Item2.Importance)?.ToList() ?? new List<Tuple<SlotModel, ActivityModel>>()) // loop through each free slot
                        if (!slotsActivities.Where(e => e.Item1.EmployeeID == employeeInfo.Employee.EmployeeID && slotActivity.Item2.DayOfWeek == e.Item2.DayOfWeek && (slotActivity.Item2.TimeOfDay == e.Item2.TimeOfDay || e.Item2.TimeOfDay == TimeSlot.ALL_DAY && !slotActivity.Item2.Name.Contains("watch"))).Any()) // only take jobs that are possible - not at the same time and day as an already assigned job - rule out impossible assigned slot combinations
                            if (!slotActivity.Item2.Name.Contains("counseling") || employeeInfo.Employee.NumOfWeeklyCounseling > 0) // if employee is set to not have counseling then counseling is not an option
                                if (_rulesRepository.Rule1(employeeIDForLastSundayNight, employeeInfo, slotActivity, slotsActivities)) // must have free monday when one where on watch sunday or must have a free day after a night shift
                                    if (slotActivity.Item2.Owner == "" || slotActivity.Item2.Owner == employeeInfo.Employee.EmployeeID)
                                    {
                                        // slot weight calculated
                                        var weight = -employeeInfo?.ActivityRecords?.SingleOrDefault(e => e.ActivityID == slotActivity.Item1.ActivityID)?.Weight ?? 0; // check activityRecord on employee on slot.activity
                                        weight += slotActivity.Item2.Owner == employeeInfo.Employee.EmployeeID ? 10000 : 0;
                                        var alreadyAssignedSlotsActivities = slotsActivities.Where(e => e.Item1.EmployeeID == employeeInfo.Employee.EmployeeID);
                                        weight += _rulesRepository.Rule2(alreadyAssignedSlotsActivities, slotActivity); // prioritize the slot because it is on the same day as the employees watch

                                        weight += _rulesRepository.Rule6(employeeInfo, slotActivity); // if employee does not have counseling then prioritize emergency work if employee has a lot of counseling then prioritize that
                                        
                                        weight += _rulesRepository.Rule7(employeeInfo.WorkHourStats.WeekExpected, employeeInfo.WorkHourStats.WeekSum); // employee workhour status add to weight

                                        weight += _rulesRepository.Rule5(alreadyAssignedSlotsActivities, slotActivity); // if the employee has a "preferred off day" on this day
                                        
                                        weight += _rulesRepository.Rule4(alreadyAssignedSlotsActivities, slotActivity); // prefer to only have one night shift a week 

                                        weight += slotActivity.Item2.Importance * 10000000; // add slotActivity importance
                                        if (weight > bestWeight) // compare weight with last slot weight - take heaviest slot
                                        {
                                            bestSlotActivity = slotActivity;
                                            bestEmployeeInfo = employeeInfo;
                                            bestWeight = weight;
                                        }
                                    }
                // now the single best slot is found - assign it and loop again to assign the remaining slots
                if (bestWeight != int.MinValue)
                {
                    bestSlotActivity.Item1.EmployeeID = bestEmployeeInfo.Employee.EmployeeID; // slot is assigned
                    bestEmployeeInfo.WorkHourStats.WeekSum += bestSlotActivity.Item2.Hours; // add hours so that the next iteration of EmployeeHours will look at WeeklyWorkHours
                    _rulesRepository.Rule3(bestEmployeeInfo, bestSlotActivity, slotsActivities); // NIGHT work FRIDAY then NIGHT work SUNDAY
                }
                else
                    break; // if int.MinValue is present then there are no slots to fill or employees to fill the slots - break.
            }
            // all slots are now assigned to the most suited employee
            slotsActivities.ForEach(e =>
            {
                if (e.Item1.EmployeeID.StartsWith('#'))
                {
                    e.Item1.EmployeeID = e.Item1.EmployeeID.Substring(1); // skip the # tag
                    if (!e.Item1.EmployeeID.StartsWith('_')) // the slot has a real employee as the slot owner but it is from an old calc
                    {
                        var availableValue = 1;
                        var foundAvailableValue = false;
                        while (!foundAvailableValue)
                        {
                            var valueProposal = availableValue;
                            foreach (var sa in slotsActivities.Where(d => d.Item1.ActivityID == e.Item1.ActivityID))
                                if (sa.Item1.EmployeeID.Equals("_" + availableValue))
                                {
                                    availableValue++;
                                    break;
                                }
                            if (valueProposal == availableValue)
                            {
                                foundAvailableValue = true;
                                e.Item1.EmployeeID = "_" + availableValue;
                            }
                        }
                    }
                }
            }); // remove # annotation that was used to identify a slot which had an old EmployeeID
            return slotsActivities.Select(e => e.Item1).ToList();
        }

        public List<ActivityRecordModel> WeightDecay(List<ActivityRecordModel> ActivityRecords)
        {
            foreach (var ActivityRecord in ActivityRecords)
            {
                ActivityRecord.Weight = ActivityRecord.Weight / 2;
            }
            return ActivityRecords;
        }
    }
    
    public class CompareWeekModel : IComparer<WeekModel>
    {
        private readonly bool IsAscending;
        public CompareWeekModel(bool isAscending = true)
        {
            IsAscending = isAscending;
        }

        public int Compare(WeekModel x, WeekModel y)
        {
            if(IsAscending)
                return CompareWeekIDs(x.WeekID, y.WeekID);
            else
                return CompareWeekIDs(y.WeekID, x.WeekID);
        }

        public short CompareWeekIDs(string WeekID1, string WeekID2)
        {
            // WeekID1 < WeekID2
            var WeekID1Year       = Convert.ToInt16(WeekID1.Substring(3, 4));
            var WeekID2Year       = Convert.ToInt16(WeekID2.Substring(3, 4));
            if (WeekID1Year == WeekID2Year)
            {
                var WeekID1WeekNumber = Convert.ToInt16(WeekID1.Substring(0, 2));
                var WeekID2WeekNumber = Convert.ToInt16(WeekID2.Substring(0, 2));
                if (WeekID1WeekNumber < WeekID2WeekNumber)
                    return -1;
                else if (WeekID1WeekNumber == WeekID2WeekNumber)
                    return 0;
                else
                    return 1;
            }
            else if (WeekID1Year < WeekID2Year)
                return -1;
            else
                return 1;
        }
    }
}