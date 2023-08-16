//In the name of Allah

global using MassTransit;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.AspNetCore.Mvc;
global using SAGA.Models;
global using SAGA.Data;
using SAGA.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("service-a-db"));


builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();

    Action<IRedisSagaRepositoryConfigurator> redisSagaConfigurator = configurator =>
        {
            const string configurationString = "3.1.78.110:6379,password=foobared";
            configurator.DatabaseConfiguration(configurationString);
            configurator.ConcurrencyMode = ConcurrencyMode.Pessimistic;// Default is Optimistic
            configurator.KeyPrefix = "dev";// Optional, prefix each saga instance key with the string specified, resulting dev:c6cfd285-80b2-4c12-bcd3-56a00d994736 
            configurator.LockSuffix = "-lockage";// Optional, to customize the lock key
            configurator.LockTimeout = TimeSpan.FromSeconds(90);// Optional, the default is 30 seconds
        };

    //city
    x.AddSagaStateMachine<AddCityStateMachine, AddCitySaga>().RedisRepository(redisSagaConfigurator);
    x.AddSagaStateMachine<UpdateCityStateMachine,UpdateCitySaga>().RedisRepository(redisSagaConfigurator);
    x.AddSagaStateMachine<DeleteCityStateMachine,DeleteCitySaga>().RedisRepository(redisSagaConfigurator);
    x.AddSagaStateMachine<AddCuisineStateMachine,AddCuisineSaga>().RedisRepository(redisSagaConfigurator);
    
    x.UsingRabbitMq((context, config) =>
    {
        var connection = new Uri("amqp://admin:admin2023@18.138.164.11:5672");
        config.Host(connection);
        config.ConfigureEndpoints(context);
    });
});

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


//city
//config.ReceiveEndpoint(QueueEndpoints.CITY_ADD_PRODUCER, e => e.ConfigureSaga<AddCitySaga>(context));
//config.ReceiveEndpoint(QueueEndpoints.CITY_UPDATE__PRODUCER, e => e.ConfigureSaga<UpdateCitySaga>(context));

//cuisine
//config.ReceiveEndpoint(QueueEndpoints.CUISINE_ADD_PRODUCER, e => e.ConfigureSaga<AddCuisineSaga>(context));

// x.AddSaga<AddCitySaga>().RedisRepository(redisSagaConfigurator);
// x.AddSaga<UpdateCitySaga>().RedisRepository(redisSagaConfigurator);

// //cuisine
// x.AddSaga<AddCuisineSaga>().RedisRepository(redisSagaConfigurator);