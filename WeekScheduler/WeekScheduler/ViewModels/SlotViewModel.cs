using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.ViewModels
{
    public class IndexSlotViewModel
    {
        public List<SlotModel> SlotModels { get; set; }
        public string SortLayout { get; set; }
        public string FilterString { get; set; }
    }

    public class DetailsSlotViewModel : SlotModel
    {
        public DetailsActivityViewModel DetailsActivityViewModel { get; set; }
        public ActivityRecordModel ActivityRecordModel { get; set; }
    }
}
