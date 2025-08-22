using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Consumer.Sagas;

public class TestSaga : MassTransitStateMachine<TestSagaState>
{
    public Event<InitHappened> InitHappened { get; set; }
    
    public Event<ThingOneHappened> ThingOneHappened { get; set; }
    
    public Event<ThingTwoHappened> ThingTwoHappened { get; set; }

    public Event Finalize { get; set; }
    
    public State AwaitingOtherThings { get; set; }
    
    public TestSaga(ILogger<TestSaga> logger)
    {
        InstanceState(x => x.CurrentState);
        
        Event
        (
            () => InitHappened,
            x => x.CorrelateBy((instance, context) => context.Message.Id == instance.Id).SelectId(context => context.MessageId ?? Guid.NewGuid())
        );
        
        Event
        (
            () => ThingOneHappened,
            x => x.CorrelateBy((instance, context) => context.Message.Id == instance.Id).SelectId(context => context.MessageId ?? Guid.NewGuid())
        );
        
        Event
        (
            () => ThingTwoHappened,
            x => x.CorrelateBy((instance, context) => context.Message.Id == instance.Id).SelectId(context => context.MessageId ?? Guid.NewGuid())
        );
        
        CompositeEvent(() => Finalize, x => x.FinalizeStatus, ThingOneHappened, ThingTwoHappened);
        
        Initially(
                When(InitHappened)
                    .Then(t => t.Saga.Id = t.Message.Id)
                    .Then(t => logger.LogInformation("ID: {id}, [InitHappened] -> AwaitingOtherThings", t.Message.Id))
                    .TransitionTo(AwaitingOtherThings)
        );
        
        DuringAny(
            When(Finalize)
                .Then(t => logger.LogInformation("ID: {id}, [Finalize]", t.Saga.Id))
                .Finalize(),
            When(ThingOneHappened)
                .Then(t =>
                {
                    if (t.Saga.FinalizeStatus == 3)
                        logger.LogInformation("ID: {id}, [ThingOneHappened] -> TriggeringCompositeEventFinalize", t.Message.Id);
                    else
                        logger.LogInformation("ID: {id}, [ThingOneHappened]", t.Message.Id);
                }
            ),
            When(ThingTwoHappened)
                .Then(t =>
                {
                    if (t.Saga.FinalizeStatus == 3)
                        logger.LogInformation("ID: {id}, [ThingTwoHappened] -> TriggeringCompositeEventFinalize", t.Message.Id);
                    else
                        logger.LogInformation("ID: {id}, [ThingTwoHappened]", t.Message.Id);
                }
            )
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