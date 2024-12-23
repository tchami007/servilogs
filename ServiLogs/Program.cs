using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ServiLogs.Application.Services;
using ServiLogs.Infrastructure.Events;
using ServiLogs.Infrastructure.Persistence;
using ServiLogs.Infrastructure.RabbitMQ;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var configuration = builder.Configuration;

builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
    sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null
        );
    }
    ));

//--------------------

builder.Services.AddScoped<ILogService , LogService>();
builder.Services.AddScoped<ILogRepository, SqlLogRepository>();

var rabbitConfig = configuration.GetSection("RabbitMQ").Get<RabbitMQConfiguration>();
builder.Services.AddSingleton(rabbitConfig);
builder.Services.AddHostedService<RabbitMQConsumer>();

//--------------------

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Version = "v1",
        Title = "ServiLogs - Aplicacion de registro en Logs",
        Description = "APIs para implementar el log centralizado",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Cesar Romano (tchami007)",
            Email = "cesarromano2007@gmail.com",
            Url = new Uri("https://github.com/tchami007")
        }
    });

    // Opcional: Agregar comentarios de código
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        // Hace que solo sea documentacion
        // options.SupportedSubmitMethods(Array.Empty<Swashbuckle.AspNetCore.SwaggerUI.SubmitMethod>());
        // Hace que Swagger esté disponible en la raíz.
        // options.RoutePrefix = ""; 
    });

}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
