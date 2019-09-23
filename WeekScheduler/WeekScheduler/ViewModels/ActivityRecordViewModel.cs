using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class IndexActivityRecordViewModel
    {
        public List<ActivityRecordModel> ActivityRecordModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }
}
