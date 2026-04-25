using MODEL.Entities;

namespace REPOSITORY.Repositories.IRepositories
{
    public interface ITaskDetailRepository : IGenericRepository<TaskDetail>
    {
        Task<IEnumerable<TaskDetail>> GetActiveByTaskIdAsync(Guid taskId);
        Task<int> GetNextLineNoAsync(Guid taskId);
    }
}
