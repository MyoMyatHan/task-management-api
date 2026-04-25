using REPOSITORY.Repositories.IRepositories;

namespace REPOSITORY.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        ITaskHeaderRepository TaskHeaders { get; }
        ITaskDetailRepository TaskDetails { get; }
        IFileAttachmentRepository FileAttachments { get; }

        Task<int> SaveChangesAsync();
    }
}
