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

builder.Services.AddDbContext<InMemoryDbContext>(options =>options.UseInMemoryDatabase("service-a-db"));

builder.Services.AddMassTransit(x =>
{
    x.AddSaga<AddCitySaga>().InMemoryRepository();

    x.UsingRabbitMq((context,config) => 
    {
        var connection = new Uri("amqp://admin:admin2023@18.138.164.11:5672");
        config.Host(connection);

        config.ReceiveEndpoint("service-a-queue", e =>
        {
            e.ConfigureSaga<AddCitySaga>(context);
        });
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
