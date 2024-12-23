using Microsoft.AspNetCore.Mvc;
using ServiLogs.Application.Models;
using ServiLogs.Application.Services;
using ServiLogs.Infrastructure.Events;

namespace ServiLogs.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ILogService _logService;
        private readonly RabbitMQConsumer _rabbit;

        public LogsController(ILogService logService, RabbitMQConsumer rabbit)
        {
            _logService = logService;
            _rabbit = rabbit;
        }

        /// <summary>
        /// RegisterLog: Crea un registro en el log.
        /// </summary>
        /// <param name="logEntry">Objeto con la informacion a registrar en el log</param>
        /// <returns>Status 200</returns>
        [HttpPost]
        public async Task<IActionResult> RegisterLog([FromBody] LogEntry logEntry)
        {
            await _logService.RegisterLogAsync(logEntry);
            return Ok();
        }

        /// <summary>
        /// StopConsumer: Detiene el consumo de eventos desde el bus
        /// </summary>
        /// <returns>Status 200</returns>
        [HttpPost("stopConsumer")]
        public IActionResult StopConsumer()
        {
            _rabbit.StopConsume();
            return Ok("Consumer stopped.");
        }

        /// <summary>
        /// RunConsumer: Arranca el consumo de eventos desde el bus
        /// </summary>
        /// <returns>Status 200</returns>
        [HttpPost("runConsumer")]
        public IActionResult RunConsumer()
        {
            _rabbit.RunConsume();
            return Ok("Consumer Running.");
        }

    }
}
