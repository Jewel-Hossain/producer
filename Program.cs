//In the name of Allah

global using MassTransit;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using SAGA.Models;
global using SAGA.Data;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("service-a-db"));


builder.Services.AddMassTransit(x =>
{
    const string configurationString = "3.1.78.110:6379,password=foobared";
    x.AddSaga<AddCitySaga>()
        .RedisRepository(r => {
            r.DatabaseConfiguration(configurationString);

            // Default is Optimistic
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

            // Optional, prefix each saga instance key with the string specified
            // resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736
            r.KeyPrefix = "dev";

            // Optional, to customize the lock key
            r.LockSuffix = "-lockage";

            // Optional, the default is 30 seconds
            r.LockTimeout = TimeSpan.FromSeconds(90);
        });


    x.UsingRabbitMq((context, config) =>
    {
        var connection = new Uri("amqp://admin:admin2023@18.138.164.11:5672");
        config.Host(connection);

        config.ReceiveEndpoint("service-a-queue", e =>
        {
            e.ConfigureSaga<AddCitySaga>(context);
        });
    });
});
// builder.Services.AddMassTransit(x =>
// {
//     // Configure Redis saga repository
//     const string configurationString = "3.1.78.110:6379,password=foobared";
//     x.AddSagaStateMachine<CitySaga, CitySagaState>()
//         .RedisRepository(r =>
//         {
//             r.DatabaseConfiguration(configurationString);
//             r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
//             r.KeyPrefix = "dev";
//             r.LockSuffix = "-lockage";
//             r.LockTimeout = TimeSpan.FromSeconds(90);
//         });

//     // Configure RabbitMQ transport
//     x.UsingRabbitMq((context, config) =>
//     {
//         var connection = new Uri("amqp://admin:admin2023@18.138.164.11:5672");
//         config.Host(connection);

//         // Configure receive endpoint
//         config.ReceiveEndpoint("service-a-queue", e =>
//         {
//             e.ConfigureSaga<CitySaga>(context);
//         });
//     });
// });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
