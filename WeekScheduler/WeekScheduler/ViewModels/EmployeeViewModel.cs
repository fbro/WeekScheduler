using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class IndexEmployeeViewModel
    {
        public List<EmployeeModel> EmployeeModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }

    public class DetailsEmployeeViewModel : EmployeeModel
    {
        [Display(Name = "Activity Records")]
        public List<ActivityRecordModel> ActivityRecords { get; set; }

    }

    public class EmployeeInfo
    {
        public EmployeeModel Employee { get; set; }

        public List<ActivityRecordModel> ActivityRecords { get; set; }

        public WorkHourStats WorkHourStats { get; set; }
    }

    public class EmployeeWithWorkHourStatsViewModel : EmployeeModel // this should be calculated yearly
    {
        public WorkHourStats WorkHourStats { get; set; }
    }

    public class WorkHourStats // per week per employee
    {
        public double WeekSum { get; set; } // example: 38,512 hours was actually assigned this week

        public double WeeksSum { get; set; } // with two weeks it could be 38,512 + 36,123 = 74,635 work hours expected

        public double WeekExpected { get; set; } // example: almost always 37 work hours expected should just be EmployeeModel.WeeklyWorkHours

        public double WeeksExpected { get; set; } // with two weeks it could be 37 * 2 = 74 work hours expected
    }
}
