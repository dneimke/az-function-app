using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Threading;

namespace Backend.Scripts
{
    public class SetupCosmosDb
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Setting up Cosmos DB...");

            // Check if Cosmos DB emulator is running
            bool isEmulatorRunning = await IsCosmosDbEmulatorRunning("https://cosmosdb:8081/");

            if (!isEmulatorRunning)
            {
                Console.WriteLine("Cosmos DB emulator doesn't seem to be running at cosmosdb:8081.");
                Console.WriteLine("Setting up mock data for development instead...");
                SetupMockData();
                return;
            }

            // Read connection string from environment variable or configuration
            string? connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to the emulator connection string if not set
                connectionString = "AccountEndpoint=https://cosmosdb:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                Console.WriteLine("Using default Cosmos DB Emulator connection string pointing to 'cosmosdb' service.");
            }

            var databaseName = "MyDatabase";
            var containerName = "MyContainer";

            // Create a Cosmos client with a handler that ignores SSL certificate errors (for emulator)
            using CosmosClient client = new CosmosClient(connectionString, new CosmosClientOptions
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                    };
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway
            });

            try
            {
                // Create the database if it doesn't exist
                Console.WriteLine($"Creating database: {databaseName}");
                DatabaseResponse database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
                Console.WriteLine($"Database {databaseName} created or already exists.");

                // Create container with a partition key
                Console.WriteLine($"Creating container: {containerName}");
                ContainerResponse containerResponse = await database.Database.CreateContainerIfNotExistsAsync(
                    id: containerName,
                    partitionKeyPath: "/id"
                );
                Console.WriteLine($"Container {containerName} created or already exists.");

                // Insert some sample data
                Container container = client.GetContainer(databaseName, containerName);

                var sampleItems = new[]
                {
                    new { id = "1", name = "Sample Item 1", description = "This is a sample item 1" },
                    new { id = "2", name = "Sample Item 2", description = "This is a sample item 2" },
                    new { id = "3", name = "Sample Item 3", description = "This is a sample item 3" }
                }; foreach (var item in sampleItems)
                {
                    try
                    {
                        var response = await container.UpsertItemAsync(item, new PartitionKey(item.id));
                        Console.WriteLine($"Upserted item {item.id}: {response.RequestCharge} RUs");
                    }
                    catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        Console.WriteLine($"Item {item.id} already exists.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error upserting item {item.id}: {ex.Message}");
                    }
                }
                Console.WriteLine("Setup completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during setup: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return;
            }
        }

        private static void SetupMockData()
        {
            Console.WriteLine("Creating mock data structure...");

            // Create a directory to store mock data status
            var mockDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "MockData");
            Directory.CreateDirectory(mockDataDir);

            // Write a status file to indicate mock data is set up
            File.WriteAllText(
                Path.Combine(mockDataDir, "status.json"),
                @"{
                    ""mockDataEnabled"": true,
                    ""items"": [
                        { ""id"": ""1"", ""name"": ""Sample Item 1"", ""description"": ""This is a sample item 1"" },
                        { ""id"": ""2"", ""name"": ""Sample Item 2"", ""description"": ""This is a sample item 2"" },
                        { ""id"": ""3"", ""name"": ""Sample Item 3"", ""description"": ""This is a sample item 3"" }
                    ]
                }");

            // Update local.settings.json to indicate mock mode
            try
            {
                string settingsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "local.settings.json");
                if (File.Exists(settingsPath))
                {
                    string settingsContent = File.ReadAllText(settingsPath);

                    // Very simple replacement to add UseMockData setting
                    // In a real app, use a JSON parser to modify the file
                    if (!settingsContent.Contains("\"UseMockData\""))
                    {
                        settingsContent = settingsContent.Replace(
                            "\"CosmosDbConnectionString\":",
                            "\"UseMockData\": \"true\",\r\n        \"CosmosDbConnectionString\":");

                        File.WriteAllText(settingsPath, settingsContent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not update local.settings.json: {ex.Message}");
            }

            Console.WriteLine("Mock data setup complete!");
            Console.WriteLine("Your function app will now use mock data instead of trying to connect to Cosmos DB.");
        }

        private static async Task<bool> IsCosmosDbEmulatorRunning(string emulatorEndpoint = "https://localhost:8081/")
        {
            using var httpClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true // Ignore SSL errors for emulator
            });

            try
            {
                // Try to access a well-known emulator endpoint
                // The emulator's main page or a specific health check endpoint if available
                Console.WriteLine($"Checking emulator status at: {emulatorEndpoint}");
                var response = await httpClient.GetAsync(emulatorEndpoint);
                // Check for a successful status code or specific content if necessary
                // For now, any response from the base URL is considered "running"
                // The emulator's main page returns 200 OK.
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Cosmos DB Emulator is running.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Cosmos DB Emulator returned status: {response.StatusCode}");
                    return false;
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Failed to connect to Cosmos DB Emulator at {emulatorEndpoint}: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while checking Cosmos DB Emulator status at {emulatorEndpoint}: {ex.Message}");
                return false;
            }
        }
    }
}
