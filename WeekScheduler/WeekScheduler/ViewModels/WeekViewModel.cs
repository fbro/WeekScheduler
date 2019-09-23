using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class DetailsWeekViewModel : WeekModel
    {
        [Display(Name = "Slots joined With its activities")]
        public List<SlotJoinActivity> SlotsJoinActivities { get; set; }

        [Display(Name = "Generated list of types")]
        public List<string> Types { get; set; }

        public List<EmployeeWithWorkHourStatsViewModel> EmployeesWithWorkHourStats { get; set; }
    }

    public class IndexWeekViewModel
    {
        public List<WeekModel> WeekModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }

    public class CreateWeekViewModel : WeekModel
    {
        [Display(Name = "Week number and year for template")]
        public string WeekTemplateID { get; set; }

        public List<EmployeeModel> Employees { get; set; }
        public List<EmployeeProjectModel> EmployeeProjects { get; set; }
    }

    public class EditWeekViewModel : WeekModel
    {
        public List<EmployeeModel> Employees { get; set; }
        public List<EmployeeProjectModel> EmployeeProjects { get; set; }

        public List<ActivityWithSlots> ActivitiesWithSlots { get; set; }
    }
    
    public class CreateRangeOfWeeksViewModel
    {
        [Display(Name = "Project ID")]
        public int ProjectID { get; set; }

        [Display(Name = "Week number and year for template")]
        public string WeekTemplateID { get; set; }

        [RegularExpression(@"\d\d-\d\d\d\d", ErrorMessage = "week number first must be 2 decimals then a hyphen followed by a year of 4 decimals")]
        [Display(Name = "From Week number and year")]
        public string FromWeekID { get; set; } = WeekModel.GetIso8601WeekOfYear(DateTime.Now) + "-" + DateTime.Now.Year;

        [RegularExpression(@"\d\d-\d\d\d\d", ErrorMessage = "week number first must be 2 decimals then a hyphen followed by a year of 4 decimals")]
        [Display(Name = "To Week number and year")]
        public string ToWeekID { get; set; } = WeekModel.GetIso8601WeekOfYear(DateTime.Now.AddDays(7)) + "-" + DateTime.Now.AddDays(7).Year;
    }

    public class SlotJoinActivity
    {
        [Display(Name = "Slot")]
        public SlotModel Slot { get; set; }

        [Display(Name = "Activity")]
        public ActivityModel Activity { get; set; }

        [Display(Name = "Type generated from Name")]
        public string Type { get; set; }
    }

    public class ActivityWithSlots
    {
        public ActivityModel Activity { get; set; }

        public List<SlotModel> Slots { get; set; }
    }
}