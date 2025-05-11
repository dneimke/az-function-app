using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Functions
{
    public class MyFunction
    {
        private readonly ILogger _logger;
        private readonly CosmosClient? _cosmosClient;
        private readonly bool _cosmosDbAvailable;
        private readonly bool _useMockData;

        // Database and container names
        private const string DatabaseName = "MyDatabase";
        private const string ContainerName = "MyContainer";

        public MyFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<MyFunction>();

            // Check if we should use mock data
            _useMockData = string.Equals(Environment.GetEnvironmentVariable("UseMockData"), "true", StringComparison.OrdinalIgnoreCase);

            if (_useMockData)
            {
                _logger.LogInformation("Using mock data mode as configured in environment variables.");
                _cosmosDbAvailable = false;
                return;
            }

            // Get connection string from configuration
            // In production, consider using Managed Identity instead of connection strings
            string? connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("CosmosDbConnectionString is not configured. Will use mock data.");
                _cosmosDbAvailable = false;
                return;
            }

            try
            {
                // Initialize Cosmos client with connection pooling and retry options
                _cosmosClient = new CosmosClient(connectionString, new CosmosClientOptions
                {
                    ConnectionMode = ConnectionMode.Direct,
                    MaxRetryAttemptsOnRateLimitedRequests = 9,
                    MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.FromSeconds(30)
                });

                _cosmosDbAvailable = true;
                _logger.LogInformation("Successfully initialized Cosmos DB client.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to initialize Cosmos DB client. Will use mock data.");
                _cosmosDbAvailable = false;
            }
        }

        [Function("myfunction")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "myfunction")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                // Create response with CORS headers
                var response = req.CreateResponse(HttpStatusCode.OK);

                // Add CORS headers explicitly
                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                response.Headers.Add("Access-Control-Allow-Headers", "Content-Type, Authorization");

                // Prepare data first, before writing to the response
                object data;

                // If Cosmos DB is not available or using mock data, prepare mock data
                if (_useMockData || !_cosmosDbAvailable || _cosmosClient == null)
                {
                    _logger.LogInformation("Using mock data for response.");
                    data = GetMockData();
                }
                else
                {
                    // Get data from Cosmos DB
                    data = await GetDataFromCosmosDb();
                }

                // Write to response only once
                await response.WriteAsJsonAsync(data);

                // Return response
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing request");

                var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
                await errorResponse.WriteAsJsonAsync(new { error = "An error occurred while processing your request." });
                return errorResponse;
            }
        }
        private async Task<object> GetDataFromCosmosDb()
        {
            try
            {
                // Try to get container reference - this validates connection
                if (_cosmosClient == null)
                {
                    throw new InvalidOperationException("Cosmos DB client is not initialized");
                }
                var container = _cosmosClient.GetContainer(DatabaseName, ContainerName);

                // Query items from the container
                QueryDefinition query = new QueryDefinition("SELECT * FROM c");
                var iterator = container.GetItemQueryIterator<dynamic>(query);

                List<dynamic> items = new List<dynamic>();
                while (iterator.HasMoreResults)
                {
                    var results = await iterator.ReadNextAsync();
                    items.AddRange(results);
                }

                return items;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning($"Container '{ContainerName}' in database '{DatabaseName}' not found. Using mock data.");
                return GetMockData();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying Cosmos DB. Using mock data.");
                return GetMockData();
            }
        }

        private object[] GetMockData()
        {
            return new[]
            {
                new { id = "1", name = "Sample Item 1", description = "This is a sample item 1" },
                new { id = "2", name = "Sample Item 2", description = "This is a sample item 2" },
                new { id = "3", name = "Sample Item 3", description = "This is a sample item 3" }
            };
        }
    }
}
