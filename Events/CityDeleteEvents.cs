//In the name of Allah

namespace SAGA.Models;

public class DeleteCity : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public CityDelete CityDelete { get; set; }
}

public class CityDeleted : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public CityDelete CityDelete { get; set; }
}

public class CityDeleteFailedNotification : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public CityDeleteBackup CityDeleteBackup { get; set; }
}

public class CityDeleteSucceeded : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}

public class CityDeleteFailed : CorrelatedBy<Guid>
{
    public Guid CorrelationId { get; set; }
    public Guid CityId { get; set; }
}






