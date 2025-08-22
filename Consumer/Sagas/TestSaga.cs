using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Consumer.Sagas;

public class TestSaga : MassTransitStateMachine<TestSagaState>
{
    public const string QueueName = "TestSaga";
    
    public Event<ThingOneHappened> ThingOneHappened { get; set; }
    
    public Event<ThingTwoHappened> ThingTwoHappened { get; set; }

    public Event? Finalize { get; set; }
    
    public TestSaga(ILogger<TestSaga> logger)
    {
        InstanceState(x => x.CurrentState);
        SetCompletedWhenFinalized();
        
        Event
        (
            () => ThingOneHappened,
            x =>
            {
                x.CorrelateBy((instance, context) => context.Message.Id == instance.Id)
                    .SelectId(context => context.MessageId ?? Guid.NewGuid());
            }
        );
        
        Event
        (
            () => ThingTwoHappened,
            x =>
            {
                x.CorrelateBy((instance, context) => context.Message.Id == instance.Id)
                    .SelectId(context => context.MessageId ?? Guid.NewGuid());
            }
        );

        CompositeEvent(() => Finalize, x => x.FinalizeStatus, CompositeEventOptions.IncludeInitial, ThingOneHappened, ThingTwoHappened);
        
        Initially(
            When(Finalize)
                .Then(t => logger.LogInformation("ID: {id}, Finalizing!", t.Saga.Id))
                .Finalize()
        );
    }
}

public class TestSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public int? Id { get; set; }

    public int FinalizeStatus { get; set; }
    
    public string CurrentState { get; set; }
}