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

var eventProducer = builder.AddProject<EventProducer>("event-producer")
    .WithReference(rabbit);

var consumer = builder.AddProject<Consumer>("consumer")
    .WithReference(rabbit);

builder.Build().Run();