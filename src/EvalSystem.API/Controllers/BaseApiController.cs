using EvalSystem.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EvalSystem.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Respond<T>(ApiResponse<T> response)
        => StatusCode(response.StatusCode, response);

    protected IActionResult Respond(ApiResponse response)
        => StatusCode(response.StatusCode, response);

    protected Guid GetUserId()
        => Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
}
