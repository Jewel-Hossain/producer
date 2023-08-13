//In the name of Allah

namespace SAGA.Data;
public class InMemoryDbContext : DbContext
{
    public InMemoryDbContext(DbContextOptions<InMemoryDbContext> options) : base(options){}

    public DbSet<Product> Products { get; set; }
    public DbSet<City> Cities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>().HasQueryFilter(c => c.IsProcessed);
    }//func
    
}//class