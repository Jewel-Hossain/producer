//In the name of Allah
using SAGA.Utils;

public class AddCitySaga : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }
}//class

public class AddCityStateMachine : MassTransitStateMachine<AddCitySaga>
{
    public State Inserting { get; private set; }
    public Event<CityAdded> CityAdded { get; private set; }
    public Event<CityInsertionSucceeded> CityInsertionSucceeded { get; private set; }
    public Event<CityInsertionFailed> CityInsertionFailed { get; private set; }

    public AddCityStateMachine()
    {
        // Define the states of the saga
        InstanceState(x => x.CurrentState);

        // Define the events that trigger the saga
        Event(() => CityAdded, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CityInsertionSucceeded, x => x.CorrelateById(context => context.Message.CorrelationId));
        Event(() => CityInsertionFailed, x => x.CorrelateById(context => context.Message.CorrelationId));

        // Define the initial state and the behavior when the saga is initiated
        Initially(
            When(CityAdded)
                .Send(context => new Uri(QueueNames.CITY_ADD_CONSUMER), context => new InsertCity
                {
                    CorrelationId = context.Message.CorrelationId,
                    City = context.Message.City
                })
                .TransitionTo(Inserting)
        );

        // Define the behavior when the city insertion succeeds
        During(Inserting,
            When(CityInsertionSucceeded)
                .ThenAsync(async context =>
                {
                    // Mark the city as processed here
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CityId);
                    if (city != null)
                    {
                        city.IsProcessed = true;
                        await _dbContext.SaveChangesAsync();
                    }
                })
                .Finalize()
        );

        // Define the behavior when the city insertion fails
        During(Inserting,
            When(CityInsertionFailed)
                .ThenAsync(async context =>
                {
                    // Compensate for the first database insertion here
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CityId);
                    if (city != null)
                    {
                        _dbContext.Remove(city);
                        await _dbContext.SaveChangesAsync();

                        // Publish a CityDeleteFailedNotification event
                        await context.Publish(new CityAddFailedNotification
                        {
                            CorrelationId = context.Saga.CorrelationId,
                            City = city
                        });

                    }//if
                })
                .Finalize()
        );

        // Define the behavior when the saga is finalized
        SetCompletedWhenFinalized();
    }//constructor
}//class


