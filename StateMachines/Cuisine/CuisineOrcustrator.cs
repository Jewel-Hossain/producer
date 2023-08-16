//In the name of Allah
using SAGA.Utils;

public class AddCuisineSaga : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }
}//class

public class AddCuisineStateMachine : MassTransitStateMachine<AddCuisineSaga>
{
    public State Inserting { get; private set; }
    public Event<CuisineAdded> CuisineAdded { get; private set; }
    public Event<CuisineInsertionSucceeded> CuisineInsertionSucceeded { get; private set; }
    public Event<CuisineInsertionFailed> CuisineInsertionFailed { get; private set; }

    public AddCuisineStateMachine()
    {
        // Define the states of the saga
        InstanceState(x => x.CurrentState);

        // Define the events that trigger the saga
        Event(() => CuisineAdded, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CuisineInsertionSucceeded, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CuisineInsertionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));

        // Define the initial state and the behavior when the saga is initiated
        Initially(
            When(CuisineAdded)
                .Send(context => new Uri(QueueNames.CUISINE_ADD_CONSUMER), context => new InsertCuisine
                {
                    CorrelationId = context.Message.CorrelationId,
                    Cuisine = context.Message.Cuisine
                })
                .TransitionTo(Inserting)
        );

        // Define the behavior when the Cuisine insertion succeeds
        During(Inserting,
            When(CuisineInsertionSucceeded)
                .ThenAsync(async context =>
                {
                    // Mark the Cuisine as processed here
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var Cuisine = await _dbContext.Cuisines.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CuisineId);
                    if (Cuisine != null)
                    {
                        Cuisine.IsProcessed = true;
                        await _dbContext.SaveChangesAsync();
                    }
                })
                .Finalize()
        );

        // Define the behavior when the Cuisine insertion fails
        During(Inserting,
            When(CuisineInsertionFailed)
                .ThenAsync(async context =>
                {
                    // Compensate for the first database insertion here
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var Cuisine = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CuisineId);
                    if (Cuisine != null)
                    {
                        _dbContext.Remove(Cuisine);
                        await _dbContext.SaveChangesAsync();
                    }
                })
                .Finalize()
        );

        // Define the behavior when the saga is finalized
        SetCompletedWhenFinalized();
    }//constructor
}//class
