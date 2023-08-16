//In the name of Allah

namespace SAGA.Models;

public class InsertCity : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public City City { get; set; }
}


public class CityAdded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public City City { get; set; }
}

public class CityInsertionSucceeded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}

public class CityInsertionFailed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}