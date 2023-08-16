//In the name of Allah


namespace SAGA.Data;
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<City> Cities { get; set; }
    public DbSet<Cuisine> Cuisines { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<City>().HasQueryFilter(c => c.IsProcessed);
        modelBuilder.Entity<Cuisine>().HasQueryFilter(c => c.IsProcessed);
    }//func

}//class