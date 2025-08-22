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
        var rand = new Random();
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;
            var thingOneHappened = new ThingOneHappened{Id = i};
            var thingTwoHappened = new ThingTwoHappened{Id = i};

            var messageToSend = rand.Next(0, 10);

            //Simulate the inconsistent order of events.
            if (messageToSend >= 5)
            {
                await _publishEndpoint.Publish(thingOneHappened, stoppingToken);
                await _publishEndpoint.Publish(thingTwoHappened, stoppingToken);
                
                _logger.LogInformation("ID: {id}, [ThingOneHappened, ThingTwoHappened]", i);
            }
            else
            {
                await _publishEndpoint.Publish(thingTwoHappened, stoppingToken);
                await _publishEndpoint.Publish(thingOneHappened, stoppingToken);
                
                _logger.LogInformation("ID: {id}, [ThingTwoHappened, ThingOneHappened]", i);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}