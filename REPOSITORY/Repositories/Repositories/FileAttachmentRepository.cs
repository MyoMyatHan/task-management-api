using Microsoft.EntityFrameworkCore;
using MODEL;
using MODEL.Entities;
using REPOSITORY.Repositories.IRepositories;

namespace REPOSITORY.Repositories.Repositories
{
    public class FileAttachmentRepository : GenericRepository<FileAttachment>, IFileAttachmentRepository
    {
        private readonly DataContext _context;

        public FileAttachmentRepository(DataContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<FileAttachment>> GetActiveByTaskIdAsync(Guid taskId)
        {
            return await _context.FileAttachments
                .Where(f => f.TaskId == taskId && f.IsActive)
                .AsNoTracking()
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();
        }

        public async Task<FileAttachment?> GetActiveByIdAsync(Guid fileId)
        {
            return await _context.FileAttachments
                .AsNoTracking()
                .FirstOrDefaultAsync(f => f.FileId == fileId && f.IsActive);
        }
    }
}
