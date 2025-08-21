using System.Text.Json;
using Contracts;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EventProducer;

public class Producer : BackgroundService
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<Producer> _logger;
    
    public Producer(IPublishEndpoint publishEndpoint, ILogger<Producer> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;
            var id = Guid.NewGuid().ToString();
            var thingOneHappened = new ThingOneHappened{Id = id};
            var thingTwoHappened = new ThingTwoHappened{Id = id};

            await _publishEndpoint.Publish(thingOneHappened, stoppingToken);
            
            await Task.Delay(1000, stoppingToken);
            
            await _publishEndpoint.Publish(thingTwoHappened, stoppingToken);

            // Task.WaitAll([pubOne, pubTwo], stoppingToken);
            
            _logger.LogInformation("Published events! - {i} times", i);

            await Task.Delay(5000, stoppingToken);
        }
    }
}