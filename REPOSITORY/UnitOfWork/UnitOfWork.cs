using MODEL;
using REPOSITORY.Repositories.IRepositories;
using REPOSITORY.Repositories.Repositories;

namespace REPOSITORY.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;

        public ITaskHeaderRepository TaskHeaders { get; }
        public ITaskDetailRepository TaskDetails { get; }
        public IFileAttachmentRepository FileAttachments { get; }

        public UnitOfWork(DataContext context)
        {
            _context = context;
            TaskHeaders = new TaskHeaderRepository(context);
            TaskDetails = new TaskDetailRepository(context);
            FileAttachments = new FileAttachmentRepository(context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
