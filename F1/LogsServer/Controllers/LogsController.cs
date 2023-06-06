using LogsServer.Models;
using LogsServer.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LogsServer.Controllers;

[ApiController]
[Route("api/logs")]
public class LogMessagesController : ControllerBase
{
    private readonly ILogsRepository _logsRepository;

    public LogMessagesController(ILogsRepository logsRepository)
    {
        _logsRepository = logsRepository;
    }

    [HttpGet]
    public IActionResult GetLogMessages([FromQuery] string? categories = null, [FromQuery] string? search = null)
    {
        var logMessages = _logsRepository.GetLogs();

        if (!string.IsNullOrEmpty(categories))
        {
            var categoryList = categories.Split(',');

            logMessages = logMessages.Where(m => categoryList.Contains(m.Category.ToString())).ToList();
        }

        if (!string.IsNullOrEmpty(search))
        {
            logMessages = logMessages.Where(m => m.Message.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(logMessages.Select(lg => $"{lg.Category.ToString()}: {lg.Message}"));
    }



}