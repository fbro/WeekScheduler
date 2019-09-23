using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class IndexActivityViewModel
    {
        public List<ActivityModel> ActivityModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }

    public class DetailsActivityViewModel : ActivityModel
    {
        public List<string> Tags { get; set; }
    }

    public class EditActivityViewModel : ActivityModel
    {
        public List<TagCount> Tags { get; set; } // tag name and number of occurences
    }

    public class TagCount
    {
        public string TagName { get; set; }
        public int TagCounter { get; set; }
        public bool IsInActivity { get; set; }
    }
}
