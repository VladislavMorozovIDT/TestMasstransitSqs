using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using shared;

namespace SqsSender
{
    [Route("api")]
    [ApiController]
    public class MyController : ControllerBase
    {
        private readonly IBus _bus;

        public MyController(IBus bus)
        {
            _bus = bus;
        }
        
        [HttpGet]
        public async Task Test()
        {
            var endpoint = await _bus.GetSendEndpoint(new Uri("topic:custom-topic"));

            await endpoint.Send(new SmthHappenEvent("my test event"), context => context.Headers.Set("ProviderId", "my"));
            await endpoint.Send(new SmthHappenEvent("not my test event"), context => context.Headers.Set("ProviderId", "not my"));
        }
    }
}