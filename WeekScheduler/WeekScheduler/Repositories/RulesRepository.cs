using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;
using WeekScheduler.Models;
using WeekScheduler.ViewModels;

namespace WeekScheduler.Repositories
{
    public class RulesRepository
    {
        private readonly List<RuleModel> _rules;

        public RulesRepository(List<RuleModel> rules)
        {
            _rules = rules;
        }

        // below is a list of rules that can be turned on and off

        // Rule 1) A day off after NIGHT work
        public bool Rule1(string initialsForLastSundayWatch, EmployeeInfo employeeInfo, Tuple<SlotModel, ActivityModel> slotActivity, List<Tuple<SlotModel, ActivityModel>> slotsActivities)
        {
            if (!_rules.Single(e => e.RuleID == 1).RuleStatus)
                return true;

            // must have free monday when one where on watch sunday
            if (slotActivity.Item2.DayOfWeek != DaySlot.MONDAY || employeeInfo.Employee.EmployeeID != initialsForLastSundayWatch)
            {
                // must have a free day after a night shift
                if (slotActivity.Item2.DayOfWeek == DaySlot.MONDAY || !slotsActivities.Where(e => e.Item1.EmployeeID == employeeInfo.Employee.EmployeeID && e.Item2.TimeOfDay == TimeSlot.NIGHT && ((int)e.Item2.DayOfWeek + 1) == (int)slotActivity.Item2.DayOfWeek).Any())
                    return true;
            }
            return false;
        }

        // Rule 2) Prefer AFTERNOON work when NIGHT work
        public int Rule2(IEnumerable<Tuple<SlotModel, ActivityModel>> alreadyAssignedSlotsActivities, Tuple<SlotModel, ActivityModel> slotActivity)
        {
            if (!_rules.Single(e => e.RuleID == 2).RuleStatus)
                return 0;

            if (alreadyAssignedSlotsActivities.Where(e => e.Item2.TimeOfDay == TimeSlot.NIGHT && e.Item2.DayOfWeek == slotActivity.Item2.DayOfWeek).Any()) // prioritize the slot because it is on the same day as the employees watch
                return 1000;
            return 0;
        }

        // Rule 3) NIGHT work FRIDAY then NIGHT work SUNDAY
        public void Rule3(EmployeeInfo bestEmployeeInfo, Tuple<SlotModel, ActivityModel> bestSlotActivity, List<Tuple<SlotModel, ActivityModel>> slotsActivities)
        {
            if (!_rules.Single(e => e.RuleID == 3).RuleStatus)
                return;

            Tuple<SlotModel, ActivityModel> bestSlot2Activity = null;
            if (bestSlotActivity.Item2.DayOfWeek == DaySlot.FRIDAY && bestSlotActivity.Item2.TimeOfDay == TimeSlot.NIGHT)
            {
                bestSlot2Activity = slotsActivities.Single(e => e.Item2.TimeOfDay == TimeSlot.NIGHT && e.Item2.DayOfWeek == DaySlot.SUNDAY);
            }
            else if (bestSlotActivity.Item2.DayOfWeek == DaySlot.SUNDAY && bestSlotActivity.Item2.TimeOfDay == TimeSlot.NIGHT)
            {
                bestSlot2Activity = slotsActivities.Single(e => e.Item2.TimeOfDay == TimeSlot.NIGHT && e.Item2.DayOfWeek == DaySlot.FRIDAY);
            }
            if (bestSlot2Activity != null)
            {
                bestSlot2Activity.Item1.EmployeeID = bestEmployeeInfo.Employee.EmployeeID; // slot is assigned
                bestEmployeeInfo.WorkHourStats.WeekSum += bestSlot2Activity.Item2.Hours; // add hours so that the next iteration of EmployeeHours will look at WeeklyWorkHours
            }
        }

        // Rule 4) Try one NIGHT work a week
        public int Rule4(IEnumerable<Tuple<SlotModel, ActivityModel>> alreadyAssignedSlotsActivities, Tuple<SlotModel, ActivityModel> slotActivity) // prefer to only have one night shift a week 
        {
            if (!_rules.Single(e => e.RuleID == 4).RuleStatus)
                return 0;

            if (alreadyAssignedSlotsActivities.Where(e => e.Item2.TimeOfDay == TimeSlot.NIGHT).Any() && slotActivity.Item2.TimeOfDay == TimeSlot.NIGHT)
                return -1000;
            return 0;
        }

        // Rule 5) "preferred off day" from NIGHT work
        public int Rule5(IEnumerable<Tuple<SlotModel, ActivityModel>> alreadyAssignedSlotsActivities, Tuple<SlotModel, ActivityModel> slotActivity)
        {
            if (!_rules.Single(e => e.RuleID == 5).RuleStatus)
                return 0;

            if (alreadyAssignedSlotsActivities.Where(e => e.Item2.DayOfWeek == slotActivity.Item2.DayOfWeek && e.Item2.Name.Contains("preferred off day")).Any() && slotActivity.Item2.TimeOfDay == TimeSlot.NIGHT)
                return -1000;
            return 0;
        }

        // Rule 6) No NumOfWeeklyCounseling then prioritize emergency work. if NumOfWeeklyCounseling is high then prioritize counseling
        public int Rule6(EmployeeInfo employeeInfo, Tuple<SlotModel, ActivityModel> slotActivity)
        {
            if (!_rules.Single(e => e.RuleID == 6).RuleStatus)
                return 0;

            if (employeeInfo.Employee.NumOfWeeklyCounseling == 0 && slotActivity.Item2.Name.Contains("emergency")) // if employee does not have counseling then prioritize emergency work
                return 500;
            else if (slotActivity.Item2.Name.Contains("counseling"))
                return 100 * employeeInfo.Employee.NumOfWeeklyCounseling;
            return 0;
        }

        // Rule 7) employee workhour status add to weight
        public int Rule7(double WeekExpected, double WeekSum)
        {
            if (!_rules.Single(e => e.RuleID == 7).RuleStatus)
                return 0;
            var result = WeekExpected - WeekSum;
            return (int) result;
        }

        // Rule 8) public holiday
        public void Rule8(ProjectModel project, WeekModel week, List<ActivityModel> activities, List<EmployeeInfo> EmployeesInProjectInfoInYear, List<Tuple<SlotModel, ActivityModel>> slotsActivities)
        {
            if (!_rules.Single(e => e.RuleID == 8).RuleStatus)
                return;
            var publicHolidays = Nager.Date.DateSystem.GetPublicHoliday(project.CountryCode, week.FirstDayOfTheWeek, week.FirstDayOfTheWeek.AddDays(6)).ToList();
            foreach (var publicHoliday in publicHolidays)
            {
                var dow = (int)publicHoliday.Date.DayOfWeek;
                var PublicHolidayActivity = activities.Where(e => e.TimeOfDay == TimeSlot.ALL_DAY && e.Name.Contains("public holiday") && e.DayOfWeek == (DaySlot)dow).Single();
                foreach (var employeeInfo in EmployeesInProjectInfoInYear)
                {
                    var s = slotsActivities.Where(e => e.Item1.ProjectID == project.ProjectID && e.Item1.ActivityID == PublicHolidayActivity.ActivityID && e.Item1.EmployeeID == '#' + employeeInfo.Employee.EmployeeID).SingleOrDefault();
                    if (s == null) // only add public holidays once - do not add again when regenerating
                        slotsActivities.Add(new Tuple<SlotModel, ActivityModel>(new SlotModel { ProjectID = project.ProjectID, WeekID = week.WeekID, ActivityID = PublicHolidayActivity.ActivityID, EmployeeID = employeeInfo.Employee.EmployeeID }, PublicHolidayActivity));
                    else if (s.Item1.EmployeeID.Substring(0, 1).Equals("#"))
                        s.Item1.EmployeeID = s.Item1.EmployeeID.Substring(1); // remove # symbol
                    employeeInfo.WorkHourStats.WeekSum += PublicHolidayActivity.Hours;
                }
            }
        }
    }
}