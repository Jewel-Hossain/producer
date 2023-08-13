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

        // var message = new CityAdded
        // {
        //     CityId = city.Id,
        //         Name = city.Name
        // };

        await _publishEndpoint.Publish(message);

        return Ok(city);
    }
}

