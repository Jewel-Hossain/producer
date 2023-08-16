//In the name of Allah

namespace SAGA.Models;

public class City
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsProcessed { get; set; }
}//class


public class CityBackup
{
    public Guid CityId { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsProcessed { get; set; }
}//class

public class CityDelete
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}//class

public class CityDeleteBackup
{
    public Guid CityDeleteId { get; set; }
    public bool IsActive { get; set; }
}//class
