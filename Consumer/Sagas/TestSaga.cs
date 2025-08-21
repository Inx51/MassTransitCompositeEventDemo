using Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Consumer.Sagas;

public class TestSaga : MassTransitStateMachine<TestSagaState>
{
    public Event<ThingOneHappened> ThingOneHappened { get; set; }
    
    public Event<ThingTwoHappened> ThingTwoHappened { get; set; }

    public Event Finalize { get; set; }
    
    public State AwaitingThingOne { get; set; }
    
    public State AwaitingThingTwo { get; set; }
    
    public TestSaga(ILogger<TestSaga> logger)
    {
        InstanceState(x => x.CurrentState);
        
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
        
        Initially(
                When(ThingOneHappened)
                    .Then(t => t.Saga.Id = t.Message.Id)
                    .TransitionTo(AwaitingThingTwo),
                When(ThingTwoHappened)
                    .Then(t => t.Saga.Id = t.Message.Id)
                    .TransitionTo(AwaitingThingOne)
        );
        
        CompositeEvent(() => Finalize, x => x.FinalizeStatus, ThingOneHappened, ThingTwoHappened);
        
        DuringAny(
            When(Finalize)
                .Then(t => logger.LogInformation("{id} Both happened!", t.Saga.Id))
                .Finalize());
    }
}

public class TestSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public string? Id { get; set; }

    public int FinalizeStatus { get; set; }
    
    public string CurrentState { get; set; }
}