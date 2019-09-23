using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeekScheduler.Data;

namespace WeekScheduler.Repositories
{
    public class ProjectsRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly WeeksRepository _weeksRepository;

        public ProjectsRepository(ApplicationDbContext context)
        {
            _context = context;
            _weeksRepository = new WeeksRepository(_context.Rule.ToList());
        }

        public async Task DeleteProjectAsync(Data.ApplicationDbContext _context, int ProjectID)
        {
            var project = await _context.Project.FindAsync(ProjectID).ConfigureAwait(false);
            _context.Project.Remove(project);
            _context.EmployeeProject.RemoveRange(_context.EmployeeProject.Where(e => e.ProjectID == ProjectID));
            
            foreach (var week in _context.Week.Where(e => e.ProjectID == ProjectID).ToList())
            {
                await _weeksRepository.DeleteWeekAsync(_context, week.ProjectID, week.WeekID).ConfigureAwait(false);
            }
        }
    }
}