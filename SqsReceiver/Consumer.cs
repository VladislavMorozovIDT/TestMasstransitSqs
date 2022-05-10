using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using shared;

namespace SqsReceiver
{
    public class Consumer :
        IConsumer<SmthHappenEvent>,
        IConsumer<Fault<SmthHappenEvent>>
    {
        private readonly ILogger<Consumer> _logger;

        public Consumer(ILogger<Consumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<SmthHappenEvent> context)
        {
            _logger.LogInformation($"received {context.Message.Text}");
            throw new Exception("test");
            return Task.CompletedTask;
        }

        public Task Consume(ConsumeContext<Fault<SmthHappenEvent>> context)
        {
            return Task.CompletedTask;
        }
    }
}