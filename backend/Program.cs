using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker; // Added for ConfigureFunctionsWebApplication extension method

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication() // Using HostBuilder pattern
    .ConfigureServices(services =>
    {
        // Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
        // services
        //     .AddApplicationInsightsTelemetryWorkerService()
        //     .ConfigureFunctionsApplicationInsights();

        // Add Cosmos DB client to the service collection
        services.AddSingleton(serviceProvider =>
        {
            string connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString") ??
                throw new InvalidOperationException("CosmosDbConnectionString is not configured.");

            return new CosmosClient(connectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                MaxRetryAttemptsOnRateLimitedRequests = 9,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
            });
        });
    })
    .Build();

host.Run();
