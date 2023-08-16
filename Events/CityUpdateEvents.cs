//In the name of Allah

namespace SAGA.Models;

public class UpdateCity : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public City City { get; set; }
}

public class CityUpdated : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public City City { get; set; }
}

public class CityUpdateFailedNotification : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public CityBackup CityBackup { get; set; }
}

public class CityUpdateSucceeded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}

public class CityUpdateFailed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}






