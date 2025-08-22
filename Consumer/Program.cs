using Consumer.Sagas;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder();

builder.Services.AddMassTransit(c =>
{
    c.UsingRabbitMq((b, f) =>
    {
        f.Host(b.GetRequiredService<IConfiguration>().GetConnectionString("rabbitmq"));
        
        f.ReceiveEndpoint(TestSaga.QueueName, e =>
        {
            e.PrefetchCount = 1;
            e.ConfigureSaga<TestSagaState>(b);
        });
        
        f.ConfigureEndpoints(b);
    });

    c.AddSagaStateMachine<TestSaga, TestSagaState>()
        .InMemoryRepository();
});

var app = builder.Build();
app.Run();