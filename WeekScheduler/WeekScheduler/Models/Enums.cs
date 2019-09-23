using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeekScheduler.Models
{
    public enum TimeSlot { MORNING = 0, AFTERNOON = 1, NIGHT = 2, ALL_DAY = 3, NOT_SET = 4 };

    public enum DaySlot { SUNDAY = 0, MONDAY = 1, TUESDAY = 2, WEDNESDAY = 3, THURSDAY = 4, FRIDAY = 5, SATURDAY = 6, NOT_SET = 7 };
}