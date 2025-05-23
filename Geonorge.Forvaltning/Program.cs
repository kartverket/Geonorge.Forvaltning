using Geonorge.Forvaltning;
using Geonorge.Forvaltning.HttpClients;
using Geonorge.Forvaltning.Models.Entity;
using Geonorge.Forvaltning.Services;
using Geonorge.Forvaltning.Services.Message;
using LoggingWithSerilog.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Reflection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

string configFile = "appsettings.json";

if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Local")
    configFile = "appsettings.Development.json";

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile(configFile)
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

services.AddResponseCaching();

services.AddMemoryCache();

services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    });

services.AddEndpointsApiExplorer();

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Forvaltning-API",
        Description = "Api to create and manage data",
        Contact = new OpenApiContact
        {
            Name = "Geonorge",
            Url = new Uri("https://www.geonorge.no/aktuelt/om-geonorge/")
        },
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Fyll inn \"Bearer\" [space] og en token i tekstfeltet under. Eksempel: \"Bearer b990274d-2082-34a5-9768-02b396f98d8d\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityDefinition("Apikey", new OpenApiSecurityScheme()
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "Apikey",
        Description = "Supabase ANON KEY",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Apikey" }
            },
            Array.Empty<string>()
        }
    });
});

services.Configure<DbConfiguration>(configuration.GetSection(DbConfiguration.SectionName));
services.Configure<AuthConfiguration>(configuration.GetSection(AuthConfiguration.SectionName));
services.Configure<EmailConfiguration>(configuration.GetSection(EmailConfiguration.SectionName));
services.Configure<SupabaseConfiguration>(configuration.GetSection(SupabaseConfiguration.SectionName));
services.Configure<OrganizationSearchSettings>(configuration.GetSection(OrganizationSearchSettings.SectionName));
services.Configure<PlaceSearchSettings>(configuration.GetSection(PlaceSearchSettings.SectionName));
services.Configure<RouteSearchSettings>(configuration.GetSection(RouteSearchSettings.SectionName));
services.Configure<AnalysisSettings>(configuration.GetSection(AnalysisSettings.SectionName));

services.AddDbContext<ApplicationContext>(opts => opts.UseNpgsql(configuration["ConnectionStrings:ForvaltningApiDatabase"]));
services.AddNpgsqlDataSource(configuration["ConnectionStrings:ForvaltningApiDatabase"]);

services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

services.AddTransient<IObjectService, ObjectService>();
services.AddTransient<IAnalysisService, AnalysisService>();

services.AddHttpClient<IAuthService, AuthService>();
services.AddHttpClient<IPlaceSearchHttpClient, PlaceSearchHttpClient>();
services.AddHttpClient<IOrganizationSearchHttpClient, OrganizationSearchHttpClient>();

services.AddHttpClient<IRouteSearchHttpClient, RouteSearchHttpClient>((serviceProvider, httpClient) =>
{
    var options = serviceProvider.GetRequiredService<IOptions<RouteSearchSettings>>();
    httpClient.DefaultRequestHeaders.Add("Authorization", options.Value.ApiKey);
});

services.AddSignalR();

services.AddCors(options =>
{
    options.AddPolicy("Development", builder =>
    {
        builder
            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
            .WithHeaders("*")
            .AllowCredentials()
            .SetIsOriginAllowed(origin => origin != null && 
                origin.StartsWith("https://localhost", StringComparison.CurrentCultureIgnoreCase));
    });
});

var app = builder.Build();

app.UseSwagger(options =>
{
    options.RouteTemplate = "docs/{documentName}/openapi.json";
});

app.UseSwaggerUI(options =>
{
    var url = $"{(!app.Environment.IsLocal() ? "/api" : "")}/docs/v1/openapi.json";
    options.SwaggerEndpoint(url, "Forvaltnings-api v1");
    options.RoutePrefix = "docs";
});

app.UseResponseCaching();

app.UseMiddleware(typeof(ExceptionHandlingMiddleware));

app.UseCors("Development");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapHub<MessageHub>("/hubs/message");

app.Run();

using (var scope = app.Services.CreateScope())
{
    var dataContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    dataContext.Database.Migrate();
}