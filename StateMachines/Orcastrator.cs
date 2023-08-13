//In the name of Allah

public class AddCitySaga : ISaga,ISagaVersion,InitiatedBy<CityAdded>,Orchestrates<CityInsertionSucceeded>,Orchestrates<CityInsertionFailed>
{
    public Guid CorrelationId { get; set; }
    public string? CurrentState { get; set; }
    public int Version { get; set; }

    public async Task Consume(ConsumeContext<CityAdded> context)
    {
        // Send the InsertCity command to ServiceB using a direct exchange and a specific queue name
        var endpoint = await context.GetSendEndpoint(new Uri("queue:service-b-queue"));
        var message = new InsertCity
        {
            CorrelationId = context.Message.CorrelationId,
            City = context.Message.City
        };
        await endpoint.Send(message);
    }

    public async Task Consume(ConsumeContext<CityInsertionSucceeded> context) 
    {
        // Mark the city as processed here
        var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
        if(_dbContext is null) return;
        var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CityId);
        if (city != null)
        {
            city.IsProcessed = true;
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task Consume(ConsumeContext<CityInsertionFailed> context)
    {
        // Compensate for the first database insertion here
        var _dbContext = context.GetPayload<IServiceProvider>().GetService<ApplicationDbContext>();
        if(_dbContext is null) return;
        var city = await _dbContext.Cities.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == context.Message.CityId);
        if (city != null)
        {
            _dbContext.Remove(city);
            await _dbContext.SaveChangesAsync();
        }
    }
}//class


