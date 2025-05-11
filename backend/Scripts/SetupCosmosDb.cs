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
            bool isEmulatorRunning = await IsCosmosDbEmulatorRunning();

            if (!isEmulatorRunning)
            {
                Console.WriteLine("Cosmos DB emulator doesn't seem to be running.");
                Console.WriteLine("Setting up mock data for development instead...");
                SetupMockData();
                return;
            }

            // Read connection string from environment variable or configuration
            string connectionString = Environment.GetEnvironmentVariable("CosmosDbConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                // Fallback to the emulator connection string if not set
                connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
                Console.WriteLine("Using default Cosmos DB Emulator connection string.");
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
                }; 
                
                foreach (var item in sampleItems)
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

        private static async Task<bool> IsCosmosDbEmulatorRunning()
        {
            // Use HttpClient with a handler that ignores the self-signed certificate
            using var httpClientHandler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };

            using var httpClient = new HttpClient(httpClientHandler);
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                // Try to connect to the emulator's status endpoint
                var response = await httpClient.GetAsync("https://localhost:8081/");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}
