using AgentAssignment.Server.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace AgentAssignment.Server.Controllers;

[ApiController]
public class ChatController : ControllerBase
{
    private readonly IChatCompletionService _service;

    public ChatController(IChatCompletionService service) => _service = service;

    [HttpPost("/v1/chat/completions")]
    public async Task<IActionResult> CompleteAsync([FromBody] ChatCompletionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (request?.Messages is null || !request.Messages.Any())
            {
                return BadRequest(new ApiErrorResponse(new ApiErrorBody("Messages must be provided", "invalid_request_error")));
            }

            if (!_service.IsReady)
            {
                return StatusCode(503, new ApiErrorResponse(new ApiErrorBody("Service is not ready", "service_unavailable")));
            }

            var response = await _service.CompleteAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (OperationCanceledException)
        {
            throw; // Let the framework handle cancellation
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiErrorResponse(new ApiErrorBody(ex.Message, "internal_error")));
        }
    }

    [HttpGet("/health")]
    public IActionResult Health()
    {
        if (_service.IsReady)
            return Ok(new { status = "ready" });

        return StatusCode(503, new { status = "starting" });


    }

        [HttpGet("/")]
    public IActionResult Homepage()
    {
        if (_service.IsReady)
            return Ok(new { status = "Welcome to the Chat API, Please note this is a simple implementation." });

        return StatusCode(503, new { status = "starting" });
    }
}
