using Geonorge.Forvaltning;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

string configFile = "appsettings.json";

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local")
    configFile = "appsettings.Development.json";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(configFile)
    .Build();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<DbTestConfiguration>(configuration.GetSection(DbTestConfiguration.SectionName));

builder.Services.AddDbContext<ApplicationContext>(opts => opts.UseNpgsql(builder.Configuration.GetConnectionString("ForvaltningApiDatabase")));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger(options =>
{
    options.RouteTemplate = "docs/{documentName}/openapi.json";
});
app.UseSwaggerUI(options =>
{
    var url = $"{(!app.Environment.IsLocal() ? "/api" : "")}/docs/v1/openapi.json";
    options.SwaggerEndpoint(url, "Forvaltnings-api v1");
    //url = $"{(!app.Environment.IsLocal() ? "/api" : "")}/custom.css";
    //options.InjectStylesheet(url);

    options.RoutePrefix = "docs";
});


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    dataContext.Database.Migrate();
}

app.Run();
