//In the name of Allah
namespace SAGA.Controllers;

[ApiController]
[Route("[controller]")]
public class CuisineController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CuisineController(ApplicationDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("GetWithFilter")]
    public async Task<IActionResult> GetWithFilter()
    {
        var cities = await _dbContext.Set<Cuisine>().ToListAsync();
        return Ok(cities);
    }

    [HttpGet("GetWithoutFilter")]
    public async Task<IActionResult> GetWithoutFilter()
    {
        var cities = await _dbContext.Set<Cuisine>().IgnoreQueryFilters().ToListAsync();
        return Ok(cities);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var Cuisine = await _dbContext.FindAsync<Cuisine>(id);
        if (Cuisine == null)
        {
            return NotFound();
        }
        return Ok(Cuisine);
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] Cuisine Cuisine)
    {
        Cuisine.Id = Guid.NewGuid();
        Cuisine.IsProcessed = false;

        _dbContext.Cuisines.Add(Cuisine);
        await _dbContext.SaveChangesAsync();

        var message = new CuisineAdded
        {
            CorrelationId = Guid.NewGuid(),
            Cuisine = Cuisine
        };

        await _publishEndpoint.Publish(message);

        return Ok(Cuisine);
    }
}

