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
    
    public State Done { get; set; }
    
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
        
        CompositeEvent(() => Finalize, x => x.FinalizeStatus, ThingOneHappened, ThingTwoHappened);
        
        Initially(
                When(ThingOneHappened)
                    .Then(t => t.Saga.Id = t.Message.Id)
                    .Then(t => logger.LogInformation("ID: {id}, [ThingOneHappened] -> AwaitingThingTwo", t.Saga.Id))
                    .TransitionTo(AwaitingThingTwo),
                When(ThingTwoHappened)
                    .Then(t => t.Saga.Id = t.Message.Id)
                    .Then(t => logger.LogInformation("ID: {id}, [ThingTwoHappened] -> AwaitingThingOne", t.Saga.Id))
                    .TransitionTo(AwaitingThingOne)
        );
        
        During(
            AwaitingThingTwo,
            When(ThingTwoHappened)
                .Then(t => logger.LogInformation("ID: {id}, [AwaitingThingTwo] - [ThingTwoHappened] -> Done ", t.Saga.Id))
                .TransitionTo(Done)
        );

        During(
            AwaitingThingOne,
            When(ThingOneHappened)
                .Then(t => logger.LogInformation("ID: {id}, [AwaitingThingOne] - [ThingOneHappened] -> Done ", t.Saga.Id))
                .TransitionTo(Done)
        );
        
        DuringAny(
            When(Finalize)
                .Then(t => logger.LogInformation("ID: {id}, [Finalize]", t.Saga.Id))
                .Finalize());
    }
}

public class TestSagaState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }

    public int? Id { get; set; }

    public int FinalizeStatus { get; set; }
    
    public string CurrentState { get; set; }
}