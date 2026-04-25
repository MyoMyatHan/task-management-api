using BAL.Services;
using MODEL.ApplicationConfig;
using MODEL.DTOs;
using MODEL.DTOs.TaskDetail;
using MODEL.DTOs.TaskHeader;
using MODEL.Entities;
using Moq;
using REPOSITORY.Repositories.IRepositories;
using REPOSITORY.UnitOfWork;
using Xunit;

namespace Tests
{
    public class TaskServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ITaskHeaderRepository> _taskHeaders;
        private readonly Mock<ITaskDetailRepository> _taskDetails;
        private readonly Mock<IFileAttachmentRepository> _fileAttachments;
        private readonly TaskService _service;

        public TaskServiceTests()
        {
            _taskHeaders    = new Mock<ITaskHeaderRepository>();
            _taskDetails    = new Mock<ITaskDetailRepository>();
            _fileAttachments = new Mock<IFileAttachmentRepository>();

            _unitOfWork = new Mock<IUnitOfWork>();
            _unitOfWork.Setup(u => u.TaskHeaders).Returns(_taskHeaders.Object);
            _unitOfWork.Setup(u => u.TaskDetails).Returns(_taskDetails.Object);
            _unitOfWork.Setup(u => u.FileAttachments).Returns(_fileAttachments.Object);

            _service = new TaskService(_unitOfWork.Object);
        }

        // ──────────────────────────────────────────────
        // GetAllTasksAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetAllTasksAsync_ReturnsSuccessful_WithPagedResult()
        {
            var tasks = new List<TaskHeader>
            {
                new TaskHeader
                {
                    TaskId = Guid.NewGuid(), TaskCode = "T-001", Title = "Test",
                    CreatedBy = "admin", Priority = "High", Status = "Pending",
                    IsActive = true,
                    TaskDetails = new List<TaskDetail>(),
                    FileAttachments = new List<FileAttachment>()
                }
            };
            _taskHeaders.Setup(r => r.GetAllActivePagedAsync(1, 10))
                        .ReturnsAsync((tasks, 1));

            var result = await _service.GetAllTasksAsync(1, 10);

            Assert.Equal(APIStatus.Successful, result.Status);
            var paged = Assert.IsType<PagedResultDto<TaskHeaderResponseDto>>(result.Data);
            Assert.Single(paged.Items);
            Assert.Equal(1, paged.TotalCount);
            Assert.Equal(1, paged.Page);
            Assert.Equal(10, paged.PageSize);
            Assert.Equal(1, paged.TotalPages);
        }

        // ──────────────────────────────────────────────
        // GetTaskByIdAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task GetTaskByIdAsync_TaskNotFound_ReturnsNotFound()
        {
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((TaskHeader?)null);

            var result = await _service.GetTaskByIdAsync(Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task GetTaskByIdAsync_TaskExists_ReturnsSuccessfulWithData()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskHeader
            {
                TaskId = taskId, TaskCode = "T-001", Title = "Test",
                CreatedBy = "admin", Priority = "High", Status = "Pending", IsActive = true,
                TaskDetails = new List<TaskDetail>(),
                FileAttachments = new List<FileAttachment>()
            };
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(taskId)).ReturnsAsync(task);

            var result = await _service.GetTaskByIdAsync(taskId);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.NotNull(result.Data);
        }

        // ──────────────────────────────────────────────
        // CreateTaskAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task CreateTaskAsync_DuplicateTaskCode_ReturnsError()
        {
            var dto = new CreateTaskHeaderDto
            {
                TaskCode = "T-001", Title = "Test",
                Priority = "High", Status = "Pending", CreatedBy = "admin"
            };
            _taskHeaders.Setup(r => r.TaskCodeExistsAsync("T-001")).ReturnsAsync(true);

            var result = await _service.CreateTaskAsync(dto);

            Assert.Equal(APIStatus.Error, result.Status);
            Assert.Contains("already exists", result.Message);
        }

        [Fact]
        public async Task CreateTaskAsync_ValidDto_SavesAndReturnsSuccessful()
        {
            var dto = new CreateTaskHeaderDto
            {
                TaskCode = "T-002", Title = "Test",
                Priority = "Medium", Status = "Pending", CreatedBy = "admin",
                TaskDetails = new List<CreateTaskDetailDto>()
            };
            _taskHeaders.Setup(r => r.TaskCodeExistsAsync("T-002")).ReturnsAsync(false);
            _taskHeaders.Setup(r => r.Add(It.IsAny<TaskHeader>())).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.CreateTaskAsync(dto);

            Assert.Equal(APIStatus.Successful, result.Status);
            _taskHeaders.Verify(r => r.Add(It.IsAny<TaskHeader>()), Times.Once);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task CreateTaskAsync_WithDetails_AssignsSequentialLineNumbers()
        {
            var dto = new CreateTaskHeaderDto
            {
                TaskCode = "T-003", Title = "Test",
                Priority = "Low", Status = "Pending", CreatedBy = "admin",
                TaskDetails = new List<CreateTaskDetailDto>
                {
                    new CreateTaskDetailDto { ItemTitle = "Step 1" },
                    new CreateTaskDetailDto { ItemTitle = "Step 2" },
                    new CreateTaskDetailDto { ItemTitle = "Step 3" }
                }
            };
            _taskHeaders.Setup(r => r.TaskCodeExistsAsync("T-003")).ReturnsAsync(false);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            TaskHeader? captured = null;
            _taskHeaders.Setup(r => r.Add(It.IsAny<TaskHeader>()))
                        .Callback<TaskHeader>(t => captured = t)
                        .Returns(Task.CompletedTask);

            await _service.CreateTaskAsync(dto);

            Assert.NotNull(captured);
            var lineNumbers = captured!.TaskDetails.Select(d => d.LineNo).ToList();
            Assert.Equal(new[] { 1, 2, 3 }, lineNumbers);
        }

        // ──────────────────────────────────────────────
        // UpdateTaskAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task UpdateTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            _taskHeaders.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((TaskHeader?)null);

            var result = await _service.UpdateTaskAsync(
                Guid.NewGuid(),
                new UpdateTaskHeaderDto { Title = "x", Priority = "Low", Status = "Pending" });

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_InactiveTask_ReturnsNotFound()
        {
            var task = new TaskHeader { TaskId = Guid.NewGuid(), IsActive = false };
            _taskHeaders.Setup(r => r.GetByGuid(task.TaskId)).ReturnsAsync(task);

            var result = await _service.UpdateTaskAsync(
                task.TaskId,
                new UpdateTaskHeaderDto { Title = "x", Priority = "Low", Status = "Pending" });

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateTaskAsync_ValidTask_UpdatesFieldsAndReturnsSuccessful()
        {
            var task = new TaskHeader
            {
                TaskId = Guid.NewGuid(), IsActive = true,
                Title = "Old Title", Priority = "Low", Status = "Pending",
                CreatedBy = "admin", TaskCode = "T-001"
            };
            var dto = new UpdateTaskHeaderDto
            {
                Title = "New Title", Priority = "High",
                Status = "Done", AssignedTo = "jane"
            };
            _taskHeaders.Setup(r => r.GetByGuid(task.TaskId)).ReturnsAsync(task);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UpdateTaskAsync(task.TaskId, dto);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.Equal("New Title", task.Title);
            Assert.Equal("High", task.Priority);
            Assert.Equal("Done", task.Status);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        // ──────────────────────────────────────────────
        // DeleteTaskAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DeleteTaskAsync_TaskNotFound_ReturnsNotFound()
        {
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((TaskHeader?)null);

            var result = await _service.DeleteTaskAsync(Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteTaskAsync_ValidTask_SoftDeletesTaskAndReturnsSuccessful()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskHeader
            {
                TaskId = taskId, IsActive = true,
                TaskDetails = new List<TaskDetail>(),
                FileAttachments = new List<FileAttachment>()
            };
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(taskId)).ReturnsAsync(task);
            _taskHeaders.Setup(r => r.GetByGuid(taskId)).ReturnsAsync(task);
            _taskDetails.Setup(r => r.GetActiveByTaskIdAsync(taskId)).ReturnsAsync(new List<TaskDetail>());
            _fileAttachments.Setup(r => r.GetActiveByTaskIdAsync(taskId)).ReturnsAsync(new List<FileAttachment>());
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteTaskAsync(taskId);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.False(task.IsActive);
        }

        [Fact]
        public async Task DeleteTaskAsync_ValidTask_CascadeSoftDeletesDetails()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskHeader
            {
                TaskId = taskId, IsActive = true,
                TaskDetails = new List<TaskDetail>(),
                FileAttachments = new List<FileAttachment>()
            };
            var detail = new TaskDetail { TaskDetailId = Guid.NewGuid(), TaskId = taskId, IsActive = true };

            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(taskId)).ReturnsAsync(task);
            _taskHeaders.Setup(r => r.GetByGuid(taskId)).ReturnsAsync(task);
            _taskDetails.Setup(r => r.GetActiveByTaskIdAsync(taskId)).ReturnsAsync(new List<TaskDetail> { detail });
            _taskDetails.Setup(r => r.GetByGuid(detail.TaskDetailId)).ReturnsAsync(detail);
            _fileAttachments.Setup(r => r.GetActiveByTaskIdAsync(taskId)).ReturnsAsync(new List<FileAttachment>());
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            await _service.DeleteTaskAsync(taskId);

            Assert.False(detail.IsActive);
        }

        // ──────────────────────────────────────────────
        // AddTaskDetailAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task AddTaskDetailAsync_TaskNotFound_ReturnsNotFound()
        {
            _taskHeaders.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((TaskHeader?)null);

            var result = await _service.AddTaskDetailAsync(
                Guid.NewGuid(),
                new CreateTaskDetailDto { ItemTitle = "x" });

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task AddTaskDetailAsync_ValidTask_AddsDetailAndReturnsSuccessful()
        {
            var taskId = Guid.NewGuid();
            var task = new TaskHeader { TaskId = taskId, IsActive = true };
            _taskHeaders.Setup(r => r.GetByGuid(taskId)).ReturnsAsync(task);
            _taskDetails.Setup(r => r.GetNextLineNoAsync(taskId)).ReturnsAsync(1);
            _taskDetails.Setup(r => r.Add(It.IsAny<TaskDetail>())).Returns(Task.CompletedTask);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.AddTaskDetailAsync(taskId, new CreateTaskDetailDto { ItemTitle = "New Step" });

            Assert.Equal(APIStatus.Successful, result.Status);
            _taskDetails.Verify(r => r.Add(It.IsAny<TaskDetail>()), Times.Once);
        }

        // ──────────────────────────────────────────────
        // UpdateTaskDetailAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task UpdateTaskDetailAsync_DetailNotFound_ReturnsNotFound()
        {
            _taskDetails.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((TaskDetail?)null);

            var result = await _service.UpdateTaskDetailAsync(
                Guid.NewGuid(),
                new UpdateTaskDetailDto { ItemTitle = "x", IsCompleted = false });

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task UpdateTaskDetailAsync_ValidDetail_UpdatesFieldsAndReturnsSuccessful()
        {
            var detail = new TaskDetail
            {
                TaskDetailId = Guid.NewGuid(), IsActive = true,
                ItemTitle = "Old", IsCompleted = false
            };
            var dto = new UpdateTaskDetailDto { ItemTitle = "New", IsCompleted = true, Remark = "Done" };
            _taskDetails.Setup(r => r.GetByGuid(detail.TaskDetailId)).ReturnsAsync(detail);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.UpdateTaskDetailAsync(detail.TaskDetailId, dto);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.Equal("New", detail.ItemTitle);
            Assert.True(detail.IsCompleted);
            Assert.Equal("Done", detail.Remark);
        }

        // ──────────────────────────────────────────────
        // DeleteTaskDetailAsync
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DeleteTaskDetailAsync_DetailNotFound_ReturnsNotFound()
        {
            _taskDetails.Setup(r => r.GetByGuid(It.IsAny<Guid>())).ReturnsAsync((TaskDetail?)null);

            var result = await _service.DeleteTaskDetailAsync(Guid.NewGuid());

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteTaskDetailAsync_ValidDetail_SoftDeletesAndReturnsSuccessful()
        {
            var detail = new TaskDetail { TaskDetailId = Guid.NewGuid(), IsActive = true };
            _taskDetails.Setup(r => r.GetByGuid(detail.TaskDetailId)).ReturnsAsync(detail);
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteTaskDetailAsync(detail.TaskDetailId);

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.False(detail.IsActive);
        }

        // ──────────────────────────────────────────────
        // DeleteTasksAsync (bulk)
        // ──────────────────────────────────────────────

        [Fact]
        public async Task DeleteTasksAsync_EmptyList_ReturnsError()
        {
            var result = await _service.DeleteTasksAsync(new List<Guid>());

            Assert.Equal(APIStatus.Error, result.Status);
        }

        [Fact]
        public async Task DeleteTasksAsync_AllNotFound_ReturnsNotFound()
        {
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(It.IsAny<Guid>()))
                        .ReturnsAsync((TaskHeader?)null);

            var result = await _service.DeleteTasksAsync(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });

            Assert.Equal(APIStatus.NotFound, result.Status);
        }

        [Fact]
        public async Task DeleteTasksAsync_AllValid_DeletesAllAndReturnsSuccessful()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();
            var task1 = new TaskHeader { TaskId = id1, IsActive = true, TaskDetails = new List<TaskDetail>(), FileAttachments = new List<FileAttachment>() };
            var task2 = new TaskHeader { TaskId = id2, IsActive = true, TaskDetails = new List<TaskDetail>(), FileAttachments = new List<FileAttachment>() };

            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(id1)).ReturnsAsync(task1);
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(id2)).ReturnsAsync(task2);
            _taskHeaders.Setup(r => r.GetByGuid(id1)).ReturnsAsync(task1);
            _taskHeaders.Setup(r => r.GetByGuid(id2)).ReturnsAsync(task2);
            _taskDetails.Setup(r => r.GetActiveByTaskIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<TaskDetail>());
            _fileAttachments.Setup(r => r.GetActiveByTaskIdAsync(It.IsAny<Guid>())).ReturnsAsync(new List<FileAttachment>());
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteTasksAsync(new List<Guid> { id1, id2 });

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.False(task1.IsActive);
            Assert.False(task2.IsActive);
            _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteTasksAsync_PartialFound_DeletesFoundAndReturnsSuccessfulWithNotFoundList()
        {
            var validId  = Guid.NewGuid();
            var invalidId = Guid.NewGuid();
            var task = new TaskHeader { TaskId = validId, IsActive = true, TaskDetails = new List<TaskDetail>(), FileAttachments = new List<FileAttachment>() };

            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(validId)).ReturnsAsync(task);
            _taskHeaders.Setup(r => r.GetByIdWithDetailsAsync(invalidId)).ReturnsAsync((TaskHeader?)null);
            _taskHeaders.Setup(r => r.GetByGuid(validId)).ReturnsAsync(task);
            _taskDetails.Setup(r => r.GetActiveByTaskIdAsync(validId)).ReturnsAsync(new List<TaskDetail>());
            _fileAttachments.Setup(r => r.GetActiveByTaskIdAsync(validId)).ReturnsAsync(new List<FileAttachment>());
            _unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

            var result = await _service.DeleteTasksAsync(new List<Guid> { validId, invalidId });

            Assert.Equal(APIStatus.Successful, result.Status);
            Assert.Contains("1 ID(s) not found", result.Message);
            Assert.False(task.IsActive);
        }
    }
}
