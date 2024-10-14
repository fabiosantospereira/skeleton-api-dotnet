using {{COMPANY_NAME}}.DotNet.Core.Application.Abstractions.Factories;
using {{COMPANY_NAME}}.DotNet.Core.WebAPI.ExtensionMethods;
using {{COMPANY_NAME}}.DotNet.Middlewares.ApiKeys.Application.Abstractions.Services;
using {{COMPANY_NAME}}.DotNet.Middlewares.ApiKeys.Domain.Entities.ApiKeys;
using {{COMPANY_NAME}}.DotNet.Middlewares.ApiKeys.ExtensionMethods;
using {{COMPANY_NAME}}.DotNet.Middlewares.OAuth2.ExtensionMethods;
using {{COMPANY_NAME}}.DotNet.Middlewares.ScimV2.Domain.Entities.Groups;
using {{COMPANY_NAME}}.DotNet.Middlewares.ScimV2.Domain.Entities.Schemas;
using {{COMPANY_NAME}}.DotNet.Middlewares.ScimV2.Domain.Entities.Users;
using {{COMPANY_NAME}}.DotNet.Middlewares.ScimV2.ExtensionMethods;
using {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.{{SERVICE_NAME}}.Domain.Entities.{{RESOURCE_NAME_PLURAL}};
using {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.{{SERVICE_NAME}}.Infra.IoC;
using {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.WebApi.Factories;
using {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.WebApi.Routes;
using {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.WebApi.Routes.{{SERVICE_NAME}};
using {{COMPANY_NAME}}.DotNet.Services.ScimV2.InMemory.ExtensionMethods;
using MassTransit;
using {{COMPANY_NAME}}.DotNet.Middlewares.ScimV2.Application.Abstractions.Services;
using {{COMPANY_NAME}}.DotNet.Services.ApiKeys.InMemory.ExtensionMethods;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace {{COMPANY_NAME}}.{{MICROSERVICE_NAME}}.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddEnvironmentVariables()
                .Build();

            var builder = WebApplication.CreateBuilder(args);
            
            RegisterServices(builder.Services);

            builder.Services.AddHttpClient();
            builder.Services.AddHealthChecks()
                .AddCheck<HealthCheck>("Default");
            
            ConfigureLogging(builder, configuration);
            ConfigureResponseCache(builder);
            //ConfigureTelemetry(builder);
            builder.Services.AddMassTransit(config =>
            {
                // Configure the in-memory message broker
                config.UsingInMemory((context, cfg) => { });

                // config.AddConsumers(typeof(...).Assembly);
            });

            var app = builder.Build();
            
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var result = JsonConvert.SerializeObject(new
                    {
                        status = report.Status.ToString(),
                        results = report.Entries.Select(e => new
                        {
                            key = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description,
                            data = e.Value.Data,
                            exception = e.Value.Exception?.Message // Include exception details
                        })
                    });
                    await context.Response.WriteAsync(result);
                }
            });
            
            app.UseTokenRoute(["AuthorizationService.CreateAccessToken"]);
            app.UseApiKeyRoutes(options: DefaultScimV2RouteOptions.CreateFor<IApiKeyService>());
            app.UseUserRoutes(options: DefaultScimV2RouteOptions.CreateFor<IUserService>());
            app.UseGroupRoutes(options: DefaultScimV2RouteOptions.CreateFor<IGroupService>());
            app.Use{{MICROSERVICE_NAME}}Routes();
            
            AddSchemas();

            app.UseHttpsRedirection();

            app.Run();
        }

        private static void RegisterServices(IServiceCollection services)
        {
            services.Add{{SERVICE_NAME}}Services();
            
            services.AddCoreServices();
            services.AddApiKeyServices();
            services.AddScimV2Services();
            services.AddOAuth2Services();
            services.AddApiKeyInMemoryServices();
            services.AddScimV2InMemoryServices();

            services.AddTransient<IContextFactory, ContextFactory>();
        }

        private static void ConfigureLogging(WebApplicationBuilder builder, IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
            builder.Logging.AddSerilog();
        }

        private static void ConfigureResponseCache(WebApplicationBuilder builder)
        {
            builder.Services.AddResponseCaching();
        }

        private static void ConfigureTelemetry(WebApplicationBuilder builder)
        {
            const string serviceName = "{{TELEMETRY_SERVICE_NAME}}";
            builder.Logging.AddOpenTelemetry(options =>
            {
                options
                    .SetResourceBuilder(
                        ResourceBuilder.CreateDefault()
                            .AddService(serviceName))
                    .AddConsoleExporter();
            });
            builder.Services.AddOpenTelemetry()
                  .ConfigureResource(resource => resource.AddService(serviceName))
                  .WithTracing(tracing => tracing
                      .AddAspNetCoreInstrumentation()
                      .AddConsoleExporter())
                  .WithMetrics(metrics => metrics
                      .AddAspNetCoreInstrumentation()
                      .AddConsoleExporter());
        }

        private static void AddSchemas()
        {
            Schema.Add<User>(File.ReadAllText("/schemas/User.1.0.schema.json"));
            Schema.Add<Group>(File.ReadAllText("/schemas/Group.1.0.schema.json"));
            Schema.Add<ApiKey>(File.ReadAllText("/schemas/ApiKey.1.0.schema.json"));
            Schema.Add<{{RESOURCE_NAME}}>(File.ReadAllText("/schemas/{{RESOURCE_NAME}}.1.0.schema.json"));
        }
    }
}