// public class CitySagaState : SagaStateMachineInstance
// {
//     public Guid CorrelationId { get; set; }
//     public bool IsDatabase1Processed { get; set; }
//     public bool IsDatabase2Processed { get; set; }
//     public string? CurrentState { get; set; }
// }

// public class CitySaga : MassTransitStateMachine<CitySagaState>
// {
//     public CitySaga()
//     {
//         InstanceState(x => x.CurrentState);

//         Event(() => CityInserted1, x => x.CorrelateById(context => context.Message.CityId));
//         Event(() => CityInserted2, x => x.CorrelateById(context => context.Message.CityId));

//         Initially(
//             When(CityInserted1)
//                 .Then(context =>
//                 {
//                     // Handle database 1 insertion
//                     context.Instance.IsDatabase1Processed = true;
//                 })
//                 .TransitionTo(ProcessingDatabase2)
//         );

//         During(ProcessingDatabase2,
//             When(CityInserted2)
//                 .Then(context =>
//                 {
//                     // Handle database 2 insertion
//                     context.Instance.IsDatabase2Processed = true;
//                 })
//                 .Finalize()
//         );

//         SetCompletedWhenFinalized();
//     }

//     public State ProcessingDatabase2 { get; private set; }

//     public Event<CityInserted> CityInserted1 { get; private set; }
//     public Event<CityInserted> CityInserted2 { get; private set; }
// }

// public interface CityInserted
// {
//     Guid CityId { get; }
// }

 
