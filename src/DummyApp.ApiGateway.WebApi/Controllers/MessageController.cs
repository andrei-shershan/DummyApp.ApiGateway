using DummyApp.ApiGateway.WebApi.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DummyApp.ApiGateway.WebApi.Controllers
{
    [ApiController]
    [Route("api/message")]
    public class MessageController : ControllerBase
    {
        private readonly ApiGatewaySettings _settings;

        public MessageController(IOptions<ApiGatewaySettings> settings)
        {
            _settings = settings.Value;
        }

        [HttpGet]
        public IActionResult GetMessage()
        {
            var message = _settings.TestMessage ?? "No message configured";
            return Ok(new { message });
        }
    }
}
