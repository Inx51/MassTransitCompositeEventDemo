using Projects;

var builder = DistributedApplication.CreateBuilder(args);


var rabbit = builder.AddRabbitMQ
    (
        name:"rabbitmq",
        userName: builder.AddParameter("username", "guest"),
        password: builder.AddParameter("password", "guest")
    )
    .WithManagementPlugin();

var postgres = builder.AddPostgres("postgres");

var consumer = builder.AddProject<Consumer>("consumer")
    .WaitFor(rabbit)
    .WithReference(rabbit);

var eventProducer = builder.AddProject<EventProducer>("event-producer")
    .WaitFor(rabbit)
    .WaitFor(consumer)
    .WithReference(rabbit);

builder.Build().Run();