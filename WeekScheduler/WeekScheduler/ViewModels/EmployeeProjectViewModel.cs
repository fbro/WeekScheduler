using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class EmployeeProjectViewModel
    {
        public List<EmployeeProjectModel> EmployeesInProject { get; set; }
        public ProjectModel Project { get; set; }
    }

    public class IndexEmployeeProjectViewModel
    {
        public List<EmployeeProjectModel> EmployeeProjectModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }
}
