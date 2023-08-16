//In the name of Allah

namespace SAGA.Controllers;

[ApiController]
[Route("[controller]")]
public class CityController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CityController(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("GetWithFilter")]
    public async Task<IActionResult> GetWithFilter()
    {
        var cities = await _dbContext.Set<City>().ToListAsync();
        return Ok(cities);
    }

    [HttpGet("GetWithoutFilter")]
    public async Task<IActionResult> GetWithoutFilter()
    {
        var cities = await _dbContext.Set<City>().IgnoreQueryFilters().ToListAsync();
        return Ok(cities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var city = await _dbContext.FindAsync<City>(id);
        if (city == null)
        {
            return NotFound();
        }
        return Ok(city);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] City city)
    {
        city.Id = Guid.NewGuid();
        city.IsProcessed = false;

        _dbContext.Cities.Add(city);
        await _dbContext.SaveChangesAsync();

        var message = new CityAdded
        {
            CorrelationId = Guid.NewGuid(),
            City = city
        };
        await _publishEndpoint.Publish(message);

        return Ok(city);
    }//func

    [HttpPut]
    public async Task<IActionResult> Put([FromQuery] Guid id, [FromBody] City city)
    {
        // Find the existing city in the database
        var existingCity = await _dbContext.Cities.FindAsync(id);
        if (existingCity == null)
        {
            return NotFound();
        }

        // Update the existing city with the new values
        existingCity.Name = city.Name;
        existingCity.CreatedAt = city.CreatedAt;
        existingCity.IsProcessed = true;

        // Save the changes to the database
        _dbContext.Cities.Update(existingCity);
        await _dbContext.SaveChangesAsync();

        // Create a new CityUpdated message
        var message = new CityUpdated
        {
            CorrelationId = Guid.NewGuid(),
            City = existingCity
        };

        // Publish the message
        await _publishEndpoint.Publish(message);

        // Return the updated city
        return Ok(existingCity);
    }//func

    [HttpDelete]
    public async Task<IActionResult> Delete([FromQuery] Guid id, [FromQuery] bool IsActive)
    {
        // Find the existing city in the database
        var existingCity = await _dbContext.Cities.FindAsync(id);
        if (existingCity == null)
        {
            return NotFound();
        }

        // Update the existing city with the new values
        existingCity.IsActive = IsActive;

        var cityDelete = new CityDelete
        {
            Id = existingCity.Id,
            IsActive = existingCity.IsActive
        };


        // Save the changes to the database
        _dbContext.Cities.Update(existingCity);
        await _dbContext.SaveChangesAsync();

        // Create a new CityUpdated message
        var message = new CityDeleted
        {
            CorrelationId = Guid.NewGuid(),
            CityDelete = cityDelete
        };

        // Publish the message
        await _publishEndpoint.Publish(message);

        // Return the updated city
        return Ok(existingCity);
    }//func

}//class

