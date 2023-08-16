//In the name of Allah


using StackExchange.Redis;
using System.Text.Json;

//In the name of Allah
using SAGA.Utils;

public class DeleteCitySaga : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }
}//class

public class DeleteCityStateMachine : MassTransitStateMachine<DeleteCitySaga>
{
    public State Deleting { get; private set; }

    public Event<CityDeleted> CityDeleted { get; private set; }
    public Event<CityDeleteSucceeded> CityDeleteSucceeded { get; private set; }
    public Event<CityDeleteFailed> CityDeleteFailed { get; private set; }

    public DeleteCityStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CityDeleted, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CityDeleteSucceeded, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CityDeleteFailed, x => x.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(CityDeleted)
                .ThenAsync(async context =>
                {
                    // Connect to the Redis server
                    var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
                    var db = connection.GetDatabase();

                    // Store the original state of the city in Redis with a time to live of 10 minutes
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CityDelete.Id);
                    if (city != null)
                    {
                        var cityDeleteBackup = new CityDeleteBackup
                        {
                            CityDeleteId = city.Id,
                            IsActive = city.IsActive
                        };
                        var serializedCityBackup = JsonSerializer.Serialize(cityDeleteBackup);
                        db.StringSet(context.Saga.CorrelationId.ToString(), serializedCityBackup, TimeSpan.FromDays(7));
                    }

                    // Send the DeleteCity command to ServiceB using a direct exchange and a specific queue name
                    var endpoint = await context.GetSendEndpoint(new Uri(QueueNames.CITY_DELETE_CONSUMER));
                    var message = new DeleteCity
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        CityDelete = context.Message.CityDelete,
                    };
                    await endpoint.Send(message);
                })
                .TransitionTo(Deleting));

        During(Deleting,
            When(CityDeleteSucceeded)
                .ThenAsync(async context =>
                {
                    // Connect to the Redis server
                    var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
                    var db = connection.GetDatabase();

                    // Compensate for the failed Delete by restoring the original state of the city from Redis
                    var value = db.StringGet(context.Saga.CorrelationId.ToString());
                    if (value.HasValue)
                    {
                        var cityDeleteBackup = JsonSerializer.Deserialize<CityDeleteBackup>(value);
                        if (cityDeleteBackup != null)
                        {
                            var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                            if (_dbContext is null) return;
                            var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == cityDeleteBackup.CityDeleteId);
                            if (city != null)
                            {
                                city.IsActive = cityDeleteBackup.IsActive;
                                await _dbContext.SaveChangesAsync();
                            }//if
                        }//if
                    }//if
                })
                .Finalize());

        During(Deleting,
    When(CityDeleteFailed)
        .ThenAsync(async context =>
        {
            // Connect to the Redis server
            var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
            var db = connection.GetDatabase();

            // Compensate for the failed Delete by restoring the original state of the city from Redis
            var value = db.StringGet(context.Saga.CorrelationId.ToString());
            if (value.HasValue)
            {
                var cityDeleteBackup = JsonSerializer.Deserialize<CityDeleteBackup>(value);

                // Publish a CityDeleteFailedNotification event
                await context.Publish(new CityDeleteFailedNotification
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CityDeleteBackup = cityDeleteBackup
                });
            }//if
        })
        .Finalize());

        // Define the behavior when the saga is finalized
        SetCompletedWhenFinalized();
    }//constructor

}//class
