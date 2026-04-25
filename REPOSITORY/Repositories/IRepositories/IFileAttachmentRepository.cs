using MODEL.Entities;

namespace REPOSITORY.Repositories.IRepositories
{
    public interface IFileAttachmentRepository : IGenericRepository<FileAttachment>
    {
        Task<IEnumerable<FileAttachment>> GetActiveByTaskIdAsync(Guid taskId);
        Task<FileAttachment?> GetActiveByIdAsync(Guid fileId);
    }
}
