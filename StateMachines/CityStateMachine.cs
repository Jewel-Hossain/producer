// public class CitySaga : MassTransitStateMachine<CitySagaState>
// {
//     public CitySaga()
//     {
//         InstanceState(x => x.CurrentState);

//         Event(() => CityAdded, x => x.CorrelateById(context => context.Message.CityId));
//         Event(() => CityProcessed, x => x.CorrelateById(context => context.Message.CityId));

//         Initially(
//             When(CityAdded)
//                 .Then(context =>
//                 {
//                     context.Saga.CityId = context.Saga.CityId;
//                     context.Saga.Name = context.Saga.Name;
//                 })
//                 .SendAsync(new Uri("queue:service-b-queue"), context => context.Init<AddCity>(new
//                 {
//                     CityId = context.Saga.CorrelationId,
//                     Name = context.Saga.Name
//                 }))
//                 .TransitionTo(AddingCity));

//         During(AddingCity,
//             When(CityProcessed)
//                 .Then(context =>
//                 {
//                     if (context.Message.Success)
//                     {
//                         // Mark city as processed in ServiceA's database
//                         // ...
//                     }
//                     else
//                     {
//                         // Compensate by removing city from ServiceA's database
//                         // ...
//                     }
//                 })
//                 .Finalize());

//         SetCompletedWhenFinalized();
//     }

//     public State AddingCity { get; private set; }

//     public Event<CityAdded> CityAdded { get; private set; }
//     public Event<CityProcessed> CityProcessed { get; private set; }

    
// }


// public class CitySagaState : SagaStateMachineInstance,ISagaVersion
// {
//     public Guid CorrelationId { get; set; }
//     public string CurrentState { get; set; }

//     public Guid CityId { get; set; }
//     public string Name { get; set; }
//     public int Version { get; set; }
// }

// public class CityAdded
// {
//     public Guid CityId { get; set; }
//     public string Name { get; set; }
// }

// public class CityProcessed
// {
//     public Guid CityId { get; set; }
//     public bool Success { get; set; }
// }

// public class AddCity
// {
//     public Guid CityId { get; set; }
//     public string Name { get; set; }
// }