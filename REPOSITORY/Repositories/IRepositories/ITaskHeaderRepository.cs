using MODEL.Entities;

namespace REPOSITORY.Repositories.IRepositories
{
    public interface ITaskHeaderRepository : IGenericRepository<TaskHeader>
    {
        Task<TaskHeader?> GetByIdWithDetailsAsync(Guid taskId);
        Task<IEnumerable<TaskHeader>> GetAllActiveAsync();
        Task<(IEnumerable<TaskHeader> Items, int TotalCount)> GetAllActivePagedAsync(int page, int pageSize);
        Task<bool> TaskCodeExistsAsync(string taskCode);
    }
}
