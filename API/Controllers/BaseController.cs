using Microsoft.AspNetCore.Mvc;
using MODEL.ApplicationConfig;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseController : ControllerBase
    {        
        protected IActionResult ToResponse(ResponseModel result, bool created = false)
        {
            return result.Status switch
            {
                APIStatus.Successful  => created ? StatusCode(201, result) : Ok(result),
                APIStatus.NotFound    => NotFound(result),
                APIStatus.Error       => BadRequest(result),
                APIStatus.SystemError => StatusCode(500, result),
                _                     => Ok(result)
            };
        }
    }
}
