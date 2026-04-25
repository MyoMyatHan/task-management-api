using Microsoft.EntityFrameworkCore;
using MODEL;
using MODEL.Entities;
using REPOSITORY.Repositories.IRepositories;

namespace REPOSITORY.Repositories.Repositories
{
    public class TaskHeaderRepository : GenericRepository<TaskHeader>, ITaskHeaderRepository
    {
        private readonly DataContext _context;

        public TaskHeaderRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<TaskHeader?> GetByIdWithDetailsAsync(Guid taskId)
        {
            return await _context.TaskHeaders
                .Include(t => t.TaskDetails.Where(d => d.IsActive))
                .Include(t => t.FileAttachments.Where(f => f.IsActive))
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.IsActive);
        }

        public async Task<IEnumerable<TaskHeader>> GetAllActiveAsync()
        {
            return await _context.TaskHeaders
                .Where(t => t.IsActive)
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<(IEnumerable<TaskHeader> Items, int TotalCount)> GetAllActivePagedAsync(int page, int pageSize)
        {
            var query = _context.TaskHeaders
                .Where(t => t.IsActive)
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> TaskCodeExistsAsync(string taskCode)
        {
            return await _context.TaskHeaders
                .AnyAsync(t => t.TaskCode == taskCode && t.IsActive);
        }
    }
}
