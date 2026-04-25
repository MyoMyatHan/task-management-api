using BAL.IServices;
using Microsoft.AspNetCore.Mvc;
using MODEL.ApplicationConfig;
using MODEL.DTOs.TaskDetail;
using MODEL.DTOs.TaskHeader;

namespace API.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : BaseController
    {
        private readonly ITaskService _taskService;
        private readonly ILogService _logService;
        private const string ControllerName = "TasksController";

        public TasksController(ITaskService taskService, ILogService logService)
        {
            _taskService = taskService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                if (page < 1 || pageSize < 1 || pageSize > 100)
                    return BadRequest(new ResponseModel { Message = "Page must be >= 1 and PageSize must be between 1 and 100.", Status = APIStatus.Error });

                var result = await _taskService.GetAllTasksAsync(page, pageSize);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.GetAll", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _taskService.GetTaskByIdAsync(id);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.GetById", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateTaskHeaderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseModel { Message = Messages.InvalidPostedData, Status = APIStatus.Error });
                var result = await _taskService.CreateTaskAsync(dto);
                if (result.Status == APIStatus.Successful)
                    await _logService.LogInfoAsync($"{ControllerName}.Create", $"Task created: {dto.TaskCode}");
                return ToResponse(result, created: true);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Create", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTaskHeaderDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseModel { Message = Messages.InvalidPostedData, Status = APIStatus.Error });
                var result = await _taskService.UpdateTaskAsync(id, dto);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Update", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _taskService.DeleteTaskAsync(id);
                if (result.Status == APIStatus.Successful)
                    await _logService.LogInfoAsync($"{ControllerName}.Delete", $"Task soft-deleted: {id}");
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.Delete", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpDelete("bulk")]
        public async Task<IActionResult> DeleteBulk([FromBody] List<Guid> ids)
        {
            try
            {
                if (ids == null || ids.Count == 0)
                    return BadRequest(new ResponseModel { Message = "No task IDs provided.", Status = APIStatus.Error });

                var result = await _taskService.DeleteTasksAsync(ids);
                if (result.Status == APIStatus.Successful)
                    await _logService.LogInfoAsync($"{ControllerName}.DeleteBulk", $"Bulk soft-deleted {ids.Count} task(s).");
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.DeleteBulk", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpPost("{id:guid}/details")]
        public async Task<IActionResult> AddDetail(Guid id, [FromBody] CreateTaskDetailDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseModel { Message = Messages.InvalidPostedData, Status = APIStatus.Error });
                var result = await _taskService.AddTaskDetailAsync(id, dto);
                return ToResponse(result, created: true);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.AddDetail", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpPut("details/{detailId:guid}")]
        public async Task<IActionResult> UpdateDetail(Guid detailId, [FromBody] UpdateTaskDetailDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new ResponseModel { Message = Messages.InvalidPostedData, Status = APIStatus.Error });
                var result = await _taskService.UpdateTaskDetailAsync(detailId, dto);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.UpdateDetail", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }

        [HttpDelete("details/{detailId:guid}")]
        public async Task<IActionResult> DeleteDetail(Guid detailId)
        {
            try
            {
                var result = await _taskService.DeleteTaskDetailAsync(detailId);
                return ToResponse(result);
            }
            catch (Exception ex)
            {
                await _logService.LogErrorAsync($"{ControllerName}.DeleteDetail", ex.Message, ex.StackTrace);
                return StatusCode(500, new ResponseModel { Message = ex.Message, Status = APIStatus.SystemError });
            }
        }
    }
}
