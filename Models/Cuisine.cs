//In the name of Allah

namespace SAGA.Models;

public class Cuisine
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
}//class

public class InsertCuisine : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Cuisine Cuisine { get; set; }
}


public class CuisineAdded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Cuisine Cuisine { get; set; }
}

public class CuisineInsertionSucceeded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CuisineId { get; set; }
}

public class CuisineInsertionFailed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CuisineId { get; set; }
}


