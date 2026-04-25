using BAL.IServices;
using MODEL.ApplicationConfig;
using MODEL.DTOs;
using MODEL.DTOs.TaskDetail;
using MODEL.DTOs.TaskHeader;
using MODEL.DTOs.File;
using MODEL.Entities;
using REPOSITORY.UnitOfWork;

namespace BAL.Services
{
    public class TaskService : ITaskService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TaskService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ResponseModel> GetAllTasksAsync(int page = 1, int pageSize = 10)
        {
            var (items, totalCount) = await _unitOfWork.TaskHeaders.GetAllActivePagedAsync(page, pageSize);

            var result = new PagedResultDto<TaskHeaderResponseDto>
            {
                Items      = items.Select(MapToHeaderDto).ToList(),
                TotalCount = totalCount,
                Page       = page,
                PageSize   = pageSize
            };

            return Success(result);
        }

        public async Task<ResponseModel> GetTaskByIdAsync(Guid taskId)
        {
            var task = await _unitOfWork.TaskHeaders.GetByIdWithDetailsAsync(taskId);

            if (task == null)
                return NotFound("Task not found.");

            return Success(MapToHeaderDtoWithDetails(task));
        }

        public async Task<ResponseModel> CreateTaskAsync(CreateTaskHeaderDto dto)
        {
            if (await _unitOfWork.TaskHeaders.TaskCodeExistsAsync(dto.TaskCode))
                return Error($"Task code '{dto.TaskCode}' already exists.");

            var task = BuildTaskFromDto(dto);
            await _unitOfWork.TaskHeaders.Add(task);
            await _unitOfWork.SaveChangesAsync();

            return new ResponseModel
            {
                Status = APIStatus.Successful,
                Message = Messages.AddSucess,
                Data = new { task.TaskId, task.TaskCode }
            };
        }

        public async Task<ResponseModel> UpdateTaskAsync(Guid taskId, UpdateTaskHeaderDto dto)
        {
            var task = await _unitOfWork.TaskHeaders.GetByGuid(taskId);

            if (task == null || !task.IsActive)
                return NotFound("Task not found.");

            ApplyTaskUpdate(task, dto);
            _unitOfWork.TaskHeaders.Update(task);
            await _unitOfWork.SaveChangesAsync();

            return Success(Messages.UpdateSucess);
        }

        public async Task<ResponseModel> DeleteTaskAsync(Guid taskId)
        {
            var task = await _unitOfWork.TaskHeaders.GetByIdWithDetailsAsync(taskId);

            if (task == null)
                return NotFound("Task not found.");

            await SoftDeleteTaskWithChildrenAsync(taskId);
            await _unitOfWork.SaveChangesAsync();
            return Success(Messages.DeleteSucess);
        }

        public async Task<ResponseModel> DeleteTasksAsync(List<Guid> taskIds)
        {
            if (taskIds == null || taskIds.Count == 0)
                return Error("No task IDs provided.");

            var notFound = new List<Guid>();

            foreach (var taskId in taskIds)
            {
                var task = await _unitOfWork.TaskHeaders.GetByIdWithDetailsAsync(taskId);
                if (task == null)
                    notFound.Add(taskId);
                else
                    await SoftDeleteTaskWithChildrenAsync(taskId);
            }

            await _unitOfWork.SaveChangesAsync();

            if (notFound.Count == taskIds.Count)
                return NotFound("None of the provided task IDs were found.");

            if (notFound.Count > 0)
                return new ResponseModel
                {
                    Status = APIStatus.Successful,
                    Message = $"Deleted {taskIds.Count - notFound.Count} task(s). {notFound.Count} ID(s) not found.",
                    Data = new { NotFound = notFound }
                };

            return Success($"Deleted {taskIds.Count} task(s) successfully.");
        }

        public async Task<ResponseModel> AddTaskDetailAsync(Guid taskId, CreateTaskDetailDto dto)
        {
            var task = await _unitOfWork.TaskHeaders.GetByGuid(taskId);

            if (task == null || !task.IsActive)
                return NotFound("Task not found.");

            var detail = await BuildDetailFromDtoAsync(taskId, dto);
            await _unitOfWork.TaskDetails.Add(detail);
            await _unitOfWork.SaveChangesAsync();

            return new ResponseModel
            {
                Status = APIStatus.Successful,
                Message = Messages.AddSucess,
                Data = new { detail.TaskDetailId, detail.LineNo }
            };
        }

        public async Task<ResponseModel> UpdateTaskDetailAsync(Guid taskDetailId, UpdateTaskDetailDto dto)
        {
            var detail = await _unitOfWork.TaskDetails.GetByGuid(taskDetailId);

            if (detail == null || !detail.IsActive)
                return NotFound("Task detail not found.");

            ApplyDetailUpdate(detail, dto);
            _unitOfWork.TaskDetails.Update(detail);
            await _unitOfWork.SaveChangesAsync();

            return Success(Messages.UpdateSucess);
        }

        public async Task<ResponseModel> DeleteTaskDetailAsync(Guid taskDetailId)
        {
            var detail = await _unitOfWork.TaskDetails.GetByGuid(taskDetailId);

            if (detail == null || !detail.IsActive)
                return NotFound("Task detail not found.");

            detail.IsActive = false;
            _unitOfWork.TaskDetails.Update(detail);
            await _unitOfWork.SaveChangesAsync();

            return Success(Messages.DeleteSucess);
        }

        private static TaskHeaderResponseDto MapToHeaderDto(TaskHeader t) => new()
        {
            TaskId      = t.TaskId,
            TaskCode    = t.TaskCode,
            Title       = t.Title,
            Description = t.Description,
            Priority    = t.Priority,
            Status      = t.Status,
            DueDate     = t.DueDate,
            AssignedTo  = t.AssignedTo,
            CreatedBy   = t.CreatedBy,
            CreatedAt   = t.CreatedAt,
            UpdatedAt   = t.UpdatedAt
        };

        private static TaskHeaderResponseDto MapToHeaderDtoWithDetails(TaskHeader t)
        {
            var dto = MapToHeaderDto(t);
            dto.TaskDetails      = t.TaskDetails.Select(MapToDetailDto).ToList();
            dto.FileAttachments  = t.FileAttachments.Select(MapToFileDto).ToList();
            return dto;
        }

        private static TaskDetailResponseDto MapToDetailDto(TaskDetail d) => new()
        {
            TaskDetailId    = d.TaskDetailId,
            TaskId          = d.TaskId,
            LineNo          = d.LineNo,
            ItemTitle       = d.ItemTitle,
            ItemDescription = d.ItemDescription,
            IsCompleted     = d.IsCompleted,
            Remark          = d.Remark,
            CreatedAt       = d.CreatedAt
        };

        private static FileResponseDto MapToFileDto(FileAttachment f) => new()
        {
            FileId           = f.FileId,
            TaskId           = f.TaskId,
            OriginalFileName = f.OriginalFileName,
            ContentType      = f.ContentType,
            FileSize         = f.FileSize,
            UploadedAt       = f.UploadedAt
        };

        private static TaskHeader BuildTaskFromDto(CreateTaskHeaderDto dto)
        {
            var task = new TaskHeader
            {
                TaskId      = Guid.NewGuid(),
                TaskCode    = dto.TaskCode,
                Title       = dto.Title,
                Description = dto.Description,
                Priority    = dto.Priority,
                Status      = dto.Status,
                DueDate     = dto.DueDate,
                AssignedTo  = dto.AssignedTo,
                CreatedBy   = dto.CreatedBy,
                CreatedAt   = DateTime.UtcNow,
                IsActive    = true
            };

            int lineNo = 1;
            foreach (var detailDto in dto.TaskDetails)
            {
                task.TaskDetails.Add(new TaskDetail
                {
                    TaskDetailId    = Guid.NewGuid(),
                    TaskId          = task.TaskId,
                    LineNo          = lineNo++,
                    ItemTitle       = detailDto.ItemTitle,
                    ItemDescription = detailDto.ItemDescription,
                    Remark          = detailDto.Remark,
                    CreatedAt       = DateTime.UtcNow,
                    IsActive        = true
                });
            }

            return task;
        }

        private async Task<TaskDetail> BuildDetailFromDtoAsync(Guid taskId, CreateTaskDetailDto dto)
        {
            var lineNo = await _unitOfWork.TaskDetails.GetNextLineNoAsync(taskId);

            return new TaskDetail
            {
                TaskDetailId    = Guid.NewGuid(),
                TaskId          = taskId,
                LineNo          = lineNo,
                ItemTitle       = dto.ItemTitle,
                ItemDescription = dto.ItemDescription,
                Remark          = dto.Remark,
                CreatedAt       = DateTime.UtcNow,
                IsActive        = true
            };
        }

        private static void ApplyTaskUpdate(TaskHeader task, UpdateTaskHeaderDto dto)
        {
            task.Title       = dto.Title;
            task.Description = dto.Description;
            task.Priority    = dto.Priority;
            task.Status      = dto.Status;
            task.DueDate     = dto.DueDate;
            task.AssignedTo  = dto.AssignedTo;
            task.UpdatedAt   = DateTime.UtcNow;
        }

        private static void ApplyDetailUpdate(TaskDetail detail, UpdateTaskDetailDto dto)
        {
            detail.ItemTitle       = dto.ItemTitle;
            detail.ItemDescription = dto.ItemDescription;
            detail.IsCompleted     = dto.IsCompleted;
            detail.Remark          = dto.Remark;
        }

        private async Task SoftDeleteTaskWithChildrenAsync(Guid taskId)
        {
            var task = await _unitOfWork.TaskHeaders.GetByGuid(taskId);
            task!.IsActive  = false;
            task.UpdatedAt  = DateTime.UtcNow;
            _unitOfWork.TaskHeaders.Update(task);

            await SoftDeleteDetailsAsync(taskId);
            await SoftDeleteAttachmentsAsync(taskId);
        }

        private async Task SoftDeleteDetailsAsync(Guid taskId)
        {
            var details = await _unitOfWork.TaskDetails.GetActiveByTaskIdAsync(taskId);
            foreach (var detail in details)
            {
                detail.IsActive = false;
                _unitOfWork.TaskDetails.Update(detail);
            }
        }

        private async Task SoftDeleteAttachmentsAsync(Guid taskId)
        {
            var files = await _unitOfWork.FileAttachments.GetActiveByTaskIdAsync(taskId);
            foreach (var file in files)
            {
                file.IsActive = false;
                _unitOfWork.FileAttachments.Update(file);
            }
        }

        private static ResponseModel Success(object? data = null, string message = Messages.Successful) =>
            new() { Status = APIStatus.Successful, Message = message, Data = data };

        private static ResponseModel Success(string message) =>
            new() { Status = APIStatus.Successful, Message = message };

        private static ResponseModel NotFound(string message) =>
            new() { Status = APIStatus.NotFound, Message = message };

        private static ResponseModel Error(string message) =>
            new() { Status = APIStatus.Error, Message = message };
    }
}

