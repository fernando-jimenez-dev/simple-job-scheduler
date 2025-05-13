using Serilog;
using WebAPI.Minimal.Shared;
using WebAPI.Minimal.StartUp;
using WebAPI.Minimal.StartUp.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("serilog.json", optional: false, reloadOnChange: true);
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .Enrich.With<ShortSourceContextEnricher>()
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureWebApiDependencies();

var app = builder.Build();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.RegisterWebApiEndpoints();
app.Run();