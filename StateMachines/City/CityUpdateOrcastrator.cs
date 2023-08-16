//In the name of Allah


using StackExchange.Redis;
using System.Text.Json;
using SAGA.Utils;

public class UpdateCitySaga : SagaStateMachineInstance, ISagaVersion
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }
}//class

public class UpdateCityStateMachine : MassTransitStateMachine<UpdateCitySaga>
{
    public State Updating { get; private set; }

    public Event<CityUpdated> CityUpdated { get; private set; }
    public Event<CityUpdateSucceeded> CityUpdateSucceeded { get; private set; }
    public Event<CityUpdateFailed> CityUpdateFailed { get; private set; }

    public UpdateCityStateMachine()
    {
        InstanceState(x => x.CurrentState);

        Event(() => CityUpdated, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CityUpdateSucceeded, x => x.CorrelateById(m => m.Message.CorrelationId));
        Event(() => CityUpdateFailed, x => x.CorrelateById(m => m.Message.CorrelationId));

        Initially(
            When(CityUpdated)
                .ThenAsync(async context =>
                {
                    // Connect to the Redis server
                    var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
                    var db = connection.GetDatabase();

                    // Store the original state of the city in Redis with a time to live of 10 minutes
                    var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                    if (_dbContext is null) return;
                    var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.City.Id);
                    if (city != null)
                    {
                        var cityBackup = new CityBackup
                        {
                            CityId = city.Id,
                            Name = city.Name,
                            CreatedAt = city.CreatedAt,
                            IsProcessed = city.IsProcessed,
                            IsActive = city.IsActive
                        };
                        var serializedCityBackup = JsonSerializer.Serialize(cityBackup);
                        db.StringSet(context.Saga.CorrelationId.ToString(), serializedCityBackup, TimeSpan.FromDays(7));
                    }

                    // Send the UpdateCity command to ServiceB using a direct exchange and a specific queue name
                    var endpoint = await context.GetSendEndpoint(new Uri(QueueNames.CITY_UPDATE_CONSUMER));
                    var message = new UpdateCity
                    {
                        CorrelationId = context.Saga.CorrelationId,
                        City = context.Message.City,
                    };
                    await endpoint.Send(message);
                })
                .TransitionTo(Updating));

        During(Updating,
            When(CityUpdateSucceeded)
                .ThenAsync(async context =>
                {
                    // Connect to the Redis server
                    var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
                    var db = connection.GetDatabase();

                    // Compensate for the failed update by restoring the original state of the city from Redis
                    var value = db.StringGet(context.Saga.CorrelationId.ToString());
                    if (value.HasValue)
                    {
                        var cityBackup = JsonSerializer.Deserialize<CityBackup>(value);
                        if (cityBackup != null)
                        {
                            var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
                            if (_dbContext is null) return;
                            var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == cityBackup.CityId);
                            if (city != null)
                            {
                                city.Name = cityBackup.Name;
                                city.CreatedAt = cityBackup.CreatedAt;
                                await _dbContext.SaveChangesAsync();
                            }//if
                        }//if
                    }//if
                })
                .Finalize());

        During(Updating,
    When(CityUpdateFailed)
        .ThenAsync(async context =>
        {
            // Connect to the Redis server
            var connection = ConnectionMultiplexer.Connect("3.1.78.110:6379,password=foobared");
            var db = connection.GetDatabase();

            // Compensate for the failed update by restoring the original state of the city from Redis
            var value = db.StringGet(context.Saga.CorrelationId.ToString());
            if (value.HasValue)
            {
                var cityBackup = JsonSerializer.Deserialize<CityBackup>(value);

                // Publish a CityUpdateFailedNotification event
                await context.Publish(new CityUpdateFailedNotification
                {
                    CorrelationId = context.Saga.CorrelationId,
                    CityBackup = cityBackup
                });
            }//if
        })
        .Finalize());

        // Define the behavior when the saga is finalized
        SetCompletedWhenFinalized();
    }//constructor

}//class
