using Microsoft.EntityFrameworkCore;
using MODEL;
using MODEL.Entities;
using REPOSITORY.Repositories.IRepositories;

namespace REPOSITORY.Repositories.Repositories
{
    public class TaskDetailRepository : GenericRepository<TaskDetail>, ITaskDetailRepository
    {
        private readonly DataContext _context;

        public TaskDetailRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskDetail>> GetActiveByTaskIdAsync(Guid taskId)
        {
            return await _context.TaskDetails
                .Where(d => d.TaskId == taskId && d.IsActive)
                .AsNoTracking()
                .OrderBy(d => d.LineNo)
                .ToListAsync();
        }

        public async Task<int> GetNextLineNoAsync(Guid taskId)
        {
            var maxLineNo = await _context.TaskDetails
                .Where(d => d.TaskId == taskId && d.IsActive)
                .MaxAsync(d => (int?)d.LineNo) ?? 0;

            return maxLineNo + 1;
        }
    }
}
