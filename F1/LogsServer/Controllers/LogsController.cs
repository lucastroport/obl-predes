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
    public IActionResult GetLogMessages([FromQuery] MessageType? category)
    {
        var logMessages = _logsRepository.GetLogs();

        if (category.HasValue)
        {
            logMessages = logMessages.Where(m => m.Category == category.Value).ToList();
        }

        return Ok(logMessages.Select(lg => $"{lg.Category.ToString()}: {lg.Message}"));
    }
}