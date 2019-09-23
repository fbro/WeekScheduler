using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Controllers;
using WeekScheduler.Models;
using WeekScheduler.ViewModels;

namespace WeekScheduler.Test
{
    public class TestInitialize
    {
        #region private readonly fields

        private readonly Data.ApplicationDbContext _context;
        private readonly List<EmployeeModel> employees = new List<EmployeeModel>
        {
            new EmployeeModel{ EmployeeID = "CS", Name = "Christina", NumOfWeeklyCounseling = 1, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "JM", Name = "Josefine",  NumOfWeeklyCounseling = 1, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "MD", Name = "Maria",     NumOfWeeklyCounseling = 4, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "HB", Name = "Heidie",    NumOfWeeklyCounseling = 4, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "LS", Name = "Louise",    NumOfWeeklyCounseling = 2, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "LH", Name = "Lisbeth",   NumOfWeeklyCounseling = 5, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "LD", Name = "Lars",      NumOfWeeklyCounseling = 5, WeeklyWorkHours = 37, Color = null },
            new EmployeeModel{ EmployeeID = "FS", Name = "Frederik",  NumOfWeeklyCounseling = 5, WeeklyWorkHours = 37, Color = null },
        };

        private static readonly double hoursMorningEmergency = 6.25;
        private static readonly double hoursMorning = 6;
        private static readonly double hoursAfternoonEmergency = 4.25;
        private static readonly double hoursAfternoon = 4;
        private static readonly double hoursWatchMondayToFriday = 2.54545454545;
        private static readonly double hoursWatchSundayAndSaturday = 4.36363636364;
        private static readonly double hoursWatchHoliday = 4.36363636364; // for holidays on workdays the watch time is set to this value
        private static readonly double hoursOnHoliday = 7.4;

        private readonly List<ActivityModel> activities = new List<ActivityModel>
        {
            // Watch
            new ActivityModel{ ActivityID = 1, Name=   "monday watch", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchMondayToFriday, Owner = "" },
            new ActivityModel{ ActivityID = 2, Name=  "tuesday watch", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchMondayToFriday, Owner = "" },
            new ActivityModel{ ActivityID = 3, Name="wednesday watch", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchMondayToFriday, Owner = "" },
            new ActivityModel{ ActivityID = 4, Name= "thursday watch", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchMondayToFriday, Owner = "" },
            new ActivityModel{ ActivityID = 5, Name=   "friday watch", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchMondayToFriday, Owner = "" },
            new ActivityModel{ ActivityID = 6, Name= "saturday watch", DayOfWeek=DaySlot.SATURDAY , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchSundayAndSaturday, Owner = "" },
            new ActivityModel{ ActivityID = 7, Name=   "sunday watch", DayOfWeek=DaySlot.SUNDAY   , TimeOfDay=TimeSlot.NIGHT, Importance=5, Hours = hoursWatchSundayAndSaturday, Owner = "" },

            // MorningEmergency
            new ActivityModel{ ActivityID =  8, Name=   "monday morning emergency", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.MORNING, Importance=3, Hours = hoursMorningEmergency, Owner = "" },
            new ActivityModel{ ActivityID =  9, Name=  "tuesday morning emergency", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.MORNING, Importance=3, Hours = hoursMorningEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 10, Name="wednesday morning emergency", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.MORNING, Importance=3, Hours = hoursMorningEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 11, Name= "thursday morning emergency", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.MORNING, Importance=3, Hours = hoursMorningEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 12, Name=   "friday morning emergency", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.MORNING, Importance=3, Hours = hoursMorningEmergency, Owner = "" },

            // AfternoonEmergency
            new ActivityModel{ ActivityID = 13, Name=   "monday afternoon emergency", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.AFTERNOON, Importance=3, Hours = hoursAfternoonEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 14, Name=  "tuesday afternoon emergency", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.AFTERNOON, Importance=3, Hours = hoursAfternoonEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 15, Name="wednesday afternoon emergency", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.AFTERNOON, Importance=3, Hours = hoursAfternoonEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 16, Name= "thursday afternoon emergency", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.AFTERNOON, Importance=3, Hours = hoursAfternoonEmergency, Owner = "" },
            new ActivityModel{ ActivityID = 17, Name=   "friday afternoon emergency", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.AFTERNOON, Importance=3, Hours = hoursAfternoonEmergency, Owner = "" },

            // MorningCounseling
            new ActivityModel{ ActivityID = 18, Name=   "monday morning counseling", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.MORNING, Importance=2, Hours = hoursMorning, Owner = "" },
            new ActivityModel{ ActivityID = 19, Name=  "tuesday morning counseling", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.MORNING, Importance=2, Hours = hoursMorning, Owner = "" },
            new ActivityModel{ ActivityID = 20, Name="wednesday morning counseling", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.MORNING, Importance=2, Hours = hoursMorning, Owner = "" },
            new ActivityModel{ ActivityID = 21, Name= "thursday morning counseling", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.MORNING, Importance=2, Hours = hoursMorning, Owner = "" },
            new ActivityModel{ ActivityID = 22, Name=   "friday morning counseling", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.MORNING, Importance=2, Hours = hoursMorning, Owner = "" },

            // AfternoonCounseling
            new ActivityModel{ ActivityID = 23, Name=   "monday afternoon counseling", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.AFTERNOON, Importance=1, Hours = hoursAfternoon, Owner = "" },
            new ActivityModel{ ActivityID = 24, Name=  "tuesday afternoon counseling", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.AFTERNOON, Importance=1, Hours = hoursAfternoon, Owner = "" },
            new ActivityModel{ ActivityID = 25, Name="wednesday afternoon counseling", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.AFTERNOON, Importance=1, Hours = hoursAfternoon, Owner = "" },
            new ActivityModel{ ActivityID = 26, Name= "thursday afternoon counseling", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.AFTERNOON, Importance=1, Hours = hoursAfternoon, Owner = "" },
            new ActivityModel{ ActivityID = 27, Name=   "friday afternoon counseling", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.AFTERNOON, Importance=1, Hours = hoursAfternoon, Owner = "" },

            // Holiday
            new ActivityModel{ ActivityID = 28, Name=   "monday holiday", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 29, Name=  "tuesday holiday", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 30, Name="wednesday holiday", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 31, Name= "thursday holiday", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 32, Name=   "friday holiday", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 33, Name= "saturday holiday", DayOfWeek=DaySlot.SATURDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 34, Name=   "sunday holiday", DayOfWeek=DaySlot.SUNDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=6, Hours = 0, Owner = "" },

            // PublicHoliday
            new ActivityModel{ ActivityID = 35, Name=   "monday public holiday", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 36, Name=  "tuesday public holiday", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 37, Name="wednesday public holiday", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 38, Name= "thursday public holiday", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 39, Name=   "friday public holiday", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = hoursOnHoliday, Owner = "" },
            new ActivityModel{ ActivityID = 40, Name= "saturday public holiday", DayOfWeek=DaySlot.SATURDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 41, Name=   "sunday public holiday", DayOfWeek=DaySlot.SUNDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=4, Hours = 0, Owner = "" },

            // preferred off day
            new ActivityModel{ ActivityID = 42, Name=   "monday preferred off day", DayOfWeek=DaySlot.MONDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 43, Name=  "tuesday preferred off day", DayOfWeek=DaySlot.TUESDAY  , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 44, Name="wednesday preferred off day", DayOfWeek=DaySlot.WEDNESDAY, TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 45, Name= "thursday preferred off day", DayOfWeek=DaySlot.THURSDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 46, Name=   "friday preferred off day", DayOfWeek=DaySlot.FRIDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 47, Name= "saturday preferred off day", DayOfWeek=DaySlot.SATURDAY , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
            new ActivityModel{ ActivityID = 48, Name=   "sunday preferred off day", DayOfWeek=DaySlot.SUNDAY   , TimeOfDay=TimeSlot.ALL_DAY, Importance=0, Hours = 0, Owner = "" },
        };

        private readonly List<TagModel> tags = new List<TagModel>
        {
            new TagModel{ TagID = "watch", ActivityID = 1},
            new TagModel{ TagID = "watch", ActivityID = 2},
            new TagModel{ TagID = "watch", ActivityID = 3},
            new TagModel{ TagID = "watch", ActivityID = 4},
            new TagModel{ TagID = "watch", ActivityID = 5},
            new TagModel{ TagID = "watch", ActivityID = 6},
            new TagModel{ TagID = "watch", ActivityID = 7},
            new TagModel{ TagID = "emergency", ActivityID = 8},
            new TagModel{ TagID = "emergency", ActivityID = 9},
            new TagModel{ TagID = "emergency", ActivityID = 10},
            new TagModel{ TagID = "emergency", ActivityID = 11},
            new TagModel{ TagID = "emergency", ActivityID = 12},
            new TagModel{ TagID = "emergency", ActivityID = 13},
            new TagModel{ TagID = "emergency", ActivityID = 14},
            new TagModel{ TagID = "emergency", ActivityID = 15},
            new TagModel{ TagID = "emergency", ActivityID = 16},
            new TagModel{ TagID = "emergency", ActivityID = 17},
            new TagModel{ TagID = "counseling", ActivityID = 18},
            new TagModel{ TagID = "counseling", ActivityID = 19},
            new TagModel{ TagID = "counseling", ActivityID = 20},
            new TagModel{ TagID = "counseling", ActivityID = 21},
            new TagModel{ TagID = "counseling", ActivityID = 22},
            new TagModel{ TagID = "counseling", ActivityID = 23},
            new TagModel{ TagID = "counseling", ActivityID = 24},
            new TagModel{ TagID = "counseling", ActivityID = 25},
            new TagModel{ TagID = "counseling", ActivityID = 26},
            new TagModel{ TagID = "counseling", ActivityID = 27},
            new TagModel{ TagID = "public holiday", ActivityID = 35},
            new TagModel{ TagID = "public holiday", ActivityID = 36},
            new TagModel{ TagID = "public holiday", ActivityID = 37},
            new TagModel{ TagID = "public holiday", ActivityID = 38},
            new TagModel{ TagID = "public holiday", ActivityID = 39},
            new TagModel{ TagID = "public holiday", ActivityID = 40},
            new TagModel{ TagID = "public holiday", ActivityID = 41},
            new TagModel{ TagID = "preferred off day", ActivityID = 42},
            new TagModel{ TagID = "preferred off day", ActivityID = 43},
            new TagModel{ TagID = "preferred off day", ActivityID = 44},
            new TagModel{ TagID = "preferred off day", ActivityID = 45},
            new TagModel{ TagID = "preferred off day", ActivityID = 46},
            new TagModel{ TagID = "preferred off day", ActivityID = 47},
            new TagModel{ TagID = "preferred off day", ActivityID = 48},
        };

        private readonly List<ProjectModel> projects = new List<ProjectModel>
        {
            new ProjectModel{ ProjectID = 1, Name = "UniVet", CountryCode = "DK" },
        };

        private readonly List<EmployeeProjectModel> employeeProjects = new List<EmployeeProjectModel>
        {
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "CS" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "JM" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "MD" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "HB" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "LS" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "LH" },
            new EmployeeProjectModel{ ProjectID = 1, EmployeeID = "LD" },
        };

        private readonly List<WeekModel> weeks = new List<WeekModel>
        {
            new WeekModel{ProjectID = 1, WeekID = "01-2018", FirstDayOfTheWeek = new DateTime(2018, 1, 1)},
        };

        private readonly List<RuleModel> rules = new List<RuleModel>
        {
            new RuleModel{ RuleID = 1, Name = "A day off after NIGHT work", RuleStatus = true },
            new RuleModel{ RuleID = 2, Name = "Prefer AFTERNOON work when NIGHT work", RuleStatus = true },
            new RuleModel{ RuleID = 3, Name = "NIGHT work FRIDAY then NIGHT work SUNDAY", RuleStatus = true },
            new RuleModel{ RuleID = 4, Name = "Try one NIGHT work a week", RuleStatus = true },
            new RuleModel{ RuleID = 5, Name = "\"preferred off day\" from NIGHT work", RuleStatus = true },
            new RuleModel{ RuleID = 6, Name = "NumOfWeeklyCounseling prioritization", RuleStatus = true },
            new RuleModel{ RuleID = 7, Name = "Prioritize Work hour balance", RuleStatus = true },
            new RuleModel{ RuleID = 8, Name = "Public holidays", RuleStatus = true },
        };

        #endregion private readonly fields

        public TestInitialize(Data.ApplicationDbContext context)
        {
            _context = context;
        }

        #region Create


        private async Task CreateEmployeesAsync()
        {
            var c = new EmployeesController(_context);
            foreach (var employee in employees)
            {
                var result = await c.Create(employee);
            }
        }

        private async Task CreateActivitiesAsync()
        {

            var c = new ActivitiesController(_context);
            foreach (var activity in activities)
            {
                var result = await c.Create(activity);
            }
        }

        private async Task CreateProjectsAsync()
        {
            var c = new ProjectsController(_context);
            foreach (var project in projects)
            {
                var result = await c.Create(project);
            }
        }

        private async Task CreateEmployeeProjectsAsync()
        {
            var c = new EmployeeProjectsController(_context);
            foreach (var employeeProject in employeeProjects)
            {
                var result = await c.Create(employeeProject);
            }
        }

        private async Task CreateWeeksAsync()
        {
            var c = new WeeksController(_context);
            foreach (var week in weeks)
            {
                var result = await c.Create(new CreateWeekViewModel { ProjectID = week.ProjectID, WeekID = week.WeekID, FirstDayOfTheWeek = week.FirstDayOfTheWeek, WeekTemplateID = "" });
            }
        }

        private async Task CreateSlotsAsync()
        {
            var slots = new List<SlotModel>();
            var c = new SlotsController(_context);
            foreach (var project in projects)
            {
                foreach (var week in weeks)
                {
                    foreach (var activity in activities)
                    {
                        if (activity.ActivityID >= 1 && activity.ActivityID <= 17) // create one for all these activites
                            slots.Add(new SlotModel { ProjectID = project.ProjectID, WeekID = week.WeekID, ActivityID = activity.ActivityID, EmployeeID = "_" + 1 });
                        if (activity.ActivityID >= 8 && activity.ActivityID <= 12) // morning emergency - create an extra
                            slots.Add(new SlotModel { ProjectID = project.ProjectID, WeekID = week.WeekID, ActivityID = activity.ActivityID, EmployeeID = "_" + 2 });
                        if (activity.ActivityID >= 18 && activity.ActivityID <= 27)
                            for (int i = 1; i <= 5; i++) // create 5 of each counseling activity
                                slots.Add(new SlotModel { ProjectID = project.ProjectID, WeekID = week.WeekID, ActivityID = activity.ActivityID, EmployeeID = "_" + i });
                    }
                }
            }
            await _context.Slot.AddRangeAsync(slots).ConfigureAwait(false);
        }

        private async Task CreateRulesAsync()
        {
            var c = new RulesController(_context);
            foreach (var rule in rules)
            {
                var result = await c.Create(new RuleModel { RuleID = rule.RuleID, Name = rule.Name, RuleStatus = rule.RuleStatus });
            }
        }

        private async Task CreateTagsAsync()
        {
            var c = new TagsController(_context);
            foreach (var tag in tags)
            {
                var result = await c.Create(new TagModel { TagID = tag.TagID, ActivityID = tag.ActivityID });
            }
        }

        #endregion Create

        #region Delete all

        private async Task DeleteAllEmployeesAsync()
        {
            var c = new EmployeesController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult) index;
            var IndexEmployeeViewModel = (IndexEmployeeViewModel)list.Model;
            foreach (var employee in IndexEmployeeViewModel.EmployeeModels)
            {
                var result = await c.DeleteConfirmed(employee.EmployeeID);
            }
        }

        private async Task DeleteAllActivitiesAsync()
        {
            var c = new ActivitiesController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult) index;
            var IndexActivityViewModel = (IndexActivityViewModel)list.Model;
            foreach (var activity in IndexActivityViewModel.ActivityModels)
            {
                var result = await c.DeleteConfirmed(activity.ActivityID);
            }
        }

        private async Task DeleteAllActivityRecordsAsync() // is only called because DeleteAllEmployeesAsync will not delete the placeholder employees that are present in the ActivityRecordsModel
        {
            var c = new ActivityRecordsController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult)index;
            var IndexActivityRecordViewModel = (IndexActivityRecordViewModel)list.Model;
            foreach (var activityRecord in IndexActivityRecordViewModel.ActivityRecordModels)
            {
                var result = await c.DeleteConfirmed(activityRecord.EmployeeID, activityRecord.ActivityID);
            }
        }

        private async Task DeleteAllProjectsAsync()
        {
            var c = new ProjectsController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult) index;
            foreach (var project in (List<ProjectModel>) list.Model)
            {
                var result = await c.DeleteConfirmed(project.ProjectID);
            }
        }

        private async Task DeleteAllRulesAsync()
        {
            var c = new RulesController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult)index;
            foreach (var rule in (List<RuleModel>)list.Model)
            {
                var result = await c.DeleteConfirmed(rule.RuleID);
            }
        }

        private async Task DeleteAllTagsAsync()
        {
            var c = new TagsController(_context);
            var index = await c.Index();
            var list = (Microsoft.AspNetCore.Mvc.ViewResult)index;
            var IndexTagViewModel = (IndexTagViewModel)list.Model;
            foreach (var tag in IndexTagViewModel.TagModels)
            {
                var result = await c.DeleteConfirmed(tag.TagID, tag.ActivityID);
            }
        }

        #endregion Delete all

        public bool TestInit()
        {
            try
            {
                // clean up - remove all data from db
                Task employees = DeleteAllEmployeesAsync();
                employees.GetAwaiter().GetResult();
                Task activities = DeleteAllActivitiesAsync();
                activities.GetAwaiter().GetResult();
                Task activityRecords = DeleteAllActivityRecordsAsync();
                activityRecords.GetAwaiter().GetResult();
                Task projects = DeleteAllProjectsAsync();
                projects.GetAwaiter().GetResult();
                Task rules = DeleteAllRulesAsync();
                rules.GetAwaiter().GetResult();
                Task tags = DeleteAllTagsAsync();
                tags.GetAwaiter().GetResult();

                // add test data
                employees = CreateEmployeesAsync();
                employees.GetAwaiter().GetResult();
                activities = CreateActivitiesAsync();
                activities.GetAwaiter().GetResult();
                projects = CreateProjectsAsync();
                projects.GetAwaiter().GetResult();
                Task employeeProjects = CreateEmployeeProjectsAsync();
                employeeProjects.GetAwaiter().GetResult();
                Task weeks = CreateWeeksAsync();
                weeks.GetAwaiter().GetResult();
                // slots are not assigned they are assigned by the algorithm
                Task slots = CreateSlotsAsync();
                slots.GetAwaiter().GetResult();
                rules = CreateRulesAsync();
                rules.GetAwaiter().GetResult();
                tags = CreateTagsAsync();
                tags.GetAwaiter().GetResult();

                return true;
            }
            catch(Exception e)
            {
                Console.Out.Write("TestInit fail " + e);
                return false;
            }
        }
    }
}
