using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class IndexTagViewModel
    {
        public List<TagModel> TagModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }

    public class DetailsTagViewModel : TagModel
    {
        public ActivityModel ActivityModel { get; set; }
    }
}
