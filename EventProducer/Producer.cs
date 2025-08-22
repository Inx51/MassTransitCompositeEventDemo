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
        var i = 0;
        var rand = new Random();
        //Adding a delay to make sure that rabbitmq has started. (even if it shouldn't be necessary since it's already configured by Aspire)
        //Added to debug why the composite event isn't triggered for the first two instances.
        await Task.Delay(20*1000, stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            i++;
            
            var initHappened = new InitHappened{Id = i};
            var thingOneHappened = new ThingOneHappened{Id = i};
            var thingTwoHappened = new ThingTwoHappened{Id = i};

            var messageToSend = rand.Next(0, 10);

            //Simulate the inconsistent order of events.
            if (messageToSend >= 5)
            {
                await _publishEndpoint.Publish(initHappened, stoppingToken);
                await _publishEndpoint.Publish(thingOneHappened, stoppingToken);
                await _publishEndpoint.Publish(thingTwoHappened, stoppingToken);
                
                _logger.LogInformation("ID: {id}, [InitHappened, ThingOneHappened, ThingTwoHappened]", i);
            }
            else
            {
                await _publishEndpoint.Publish(initHappened, stoppingToken);
                await _publishEndpoint.Publish(thingTwoHappened, stoppingToken);
                await _publishEndpoint.Publish(thingOneHappened, stoppingToken);
                
                _logger.LogInformation("ID: {id}, [InitHappened, ThingTwoHappened, ThingOneHappened]", i);
            }

            await Task.Delay(5000, stoppingToken);
        }
    }
}