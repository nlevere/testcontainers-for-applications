using System.Data;
using Dapper;
using Npgsql;
using WeatherApp;

SqlMapper.AddTypeHandler(new SqlDateOnlyTypeHandler());

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IDbConnection>(sp => 
{
    var connection = new NpgsqlConnection(builder.Configuration["ConnectionStrings:DefaultConnection"]);
    connection.Open();
    return connection;
});
builder.Services.AddScoped<IWeatherReportRepository, WeatherReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Console.WriteLine($"Connection String: {app.Configuration.GetConnectionString("DefaultConnection")}");

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (IWeatherReportRepository weatherReportRepository) =>
{
    var forecast =  Enumerable.Range(1, 5).Select(async index =>
    {
        var forecast = new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        );
        try 
        {
            await weatherReportRepository.SaveForecastAsync(forecast);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        return forecast;
    })
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();
