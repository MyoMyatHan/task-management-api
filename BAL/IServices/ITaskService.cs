using MODEL.ApplicationConfig;
using MODEL.DTOs.TaskDetail;
using MODEL.DTOs.TaskHeader;

namespace BAL.IServices
{
    public interface ITaskService
    {
        Task<ResponseModel> GetAllTasksAsync(int page = 1, int pageSize = 10);
        Task<ResponseModel> GetTaskByIdAsync(Guid taskId);
        Task<ResponseModel> CreateTaskAsync(CreateTaskHeaderDto dto);
        Task<ResponseModel> UpdateTaskAsync(Guid taskId, UpdateTaskHeaderDto dto);
        Task<ResponseModel> DeleteTaskAsync(Guid taskId);
        Task<ResponseModel> DeleteTasksAsync(List<Guid> taskIds);
        Task<ResponseModel> AddTaskDetailAsync(Guid taskId, CreateTaskDetailDto dto);
        Task<ResponseModel> UpdateTaskDetailAsync(Guid taskDetailId, UpdateTaskDetailDto dto);
        Task<ResponseModel> DeleteTaskDetailAsync(Guid taskDetailId);
    }
}
