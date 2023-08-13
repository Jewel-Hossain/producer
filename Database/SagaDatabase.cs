// public class AppDbContext : SagaDbContext
// {
//     public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
//         : base(options)
//     {
//     }

//     protected override IEnumerable<ISagaClassMap> Configurations
//     {
//         get { yield return new AddCitySagaMap(); }
//     }
// }

// public class AddCitySagaMap : SagaClassMap<AddCitySaga>
// {
//     protected override void Configure(EntityTypeBuilder<AddCitySaga> entity, ModelBuilder model)
//     {
//         entity.Property(x => x.CurrentState).HasMaxLength(64);
//     }
// }
