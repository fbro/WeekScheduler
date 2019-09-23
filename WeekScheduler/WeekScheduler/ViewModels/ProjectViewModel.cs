using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.ViewModels
{
    public class ListProjectWeekViewModel
    {
        public List<DetailsWeekViewModel> WeekViewModels { get; set; }
        public int ProjectID { get; set; }
        public string FromWeekID { get; set; }
        public string ToWeekID { get; set; }
    }
}
