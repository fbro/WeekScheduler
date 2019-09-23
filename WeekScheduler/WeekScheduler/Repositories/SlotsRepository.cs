using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Models;

namespace WeekScheduler.Repositories
{
    public class SlotsRepository
    {

        public async Task DeleteSlotAsync(Data.ApplicationDbContext _context, int ProjectID, string WeekID, int ActivityID, string EmployeeID)
        {
            var slot = await _context.Slot.FindAsync(ProjectID, WeekID, ActivityID, EmployeeID).ConfigureAwait(false);
            _context.Slot.Remove(slot);

            var activityRecord = _context.ActivityRecord.Where(e => e.EmployeeID == slot.EmployeeID && e.ActivityID == slot.ActivityID).SingleOrDefault();
            if(activityRecord != null)
            {
                activityRecord.Weight--;
                if (activityRecord.Weight <= 0)
                    _context.ActivityRecord.Remove(await _context.ActivityRecord.FindAsync(activityRecord.EmployeeID, activityRecord.ActivityID));
                else
                    _context.ActivityRecord.Update(activityRecord);
            }
        }

        public void CreateSlot(Data.ApplicationDbContext _context, SlotModel slot)
        {
            var activityRecord = _context.ActivityRecord.Where(e => e.EmployeeID == slot.EmployeeID && e.ActivityID == slot.ActivityID).SingleOrDefault();
            if (activityRecord != null)
            {
                activityRecord.Weight++;
                _context.ActivityRecord.Update(activityRecord);
            }
            else
            {
                activityRecord = new ActivityRecordModel
                {
                    EmployeeID = slot.EmployeeID,
                    ActivityID = slot.ActivityID,
                    Weight = 1,
                };
                _context.ActivityRecord.Add(activityRecord);
            }
            _context.Slot.Add(slot);
        }
    }
}