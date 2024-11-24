using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EdiRetrieval.Services
{
    public class CartService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<CartService> _logger;

        public CartService(IConfiguration configuration, ILogger<CartService> logger)
        {
            var connectionString = configuration["CosmosDb1:ConnectionString"];
            var databaseName = configuration["CosmosDb1:DatabaseName"];
            var containerName = configuration["CosmosDb1:ContainerName"];

            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            _logger = logger;
        }

        public async Task AddContainerToCartAsync(CosmosItem containerItem)
        {
            try
            {
                if (string.IsNullOrEmpty(containerItem.Id))
                {
                    containerItem.Id = containerItem.ContainerNumber;
                }
                containerItem.CartAdded = true;

                var partitionKey = containerItem.ContainerNumber;

                _logger.LogInformation($"Checking if container with ContainerNumber: {containerItem.ContainerNumber} exists.");

                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.containerNumber = @containerNumber")
                    .WithParameter("@containerNumber", containerItem.ContainerNumber);

                var feedIterator = _container.GetItemQueryIterator<CosmosItem>(
                    queryDefinition,
                    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
                );

                var existingContainer = await feedIterator.ReadNextAsync();

                if (existingContainer.Any())
                {
                    _logger.LogInformation($"Container with ContainerNumber: {containerItem.ContainerNumber} already exists in the cart. Skipping insertion.");
                    return;
                }

                try
                {
                    await _container.CreateItemAsync(containerItem, new PartitionKey(partitionKey));
                    _logger.LogInformation($"Container {containerItem.ContainerNumber} added to the cart.");
                }
                catch (CosmosException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        _logger.LogWarning($"Conflict: Container with ContainerNumber: {containerItem.ContainerNumber} already exists.");
                    }
                    else
                    {
                        _logger.LogError($"Error adding container to Cosmos DB: {ex.Message}");
                        throw;
                    }
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error adding container to Cosmos DB: {ex.Message}");
                throw new Exception($"Error adding container to Cosmos DB: {ex.Message}");
            }
        }

        public async Task<List<CosmosItem>> GetAllContainersInCartAsync()
        {
            var containers = new List<CosmosItem>();

            try
            {
                // Query only containers where CartAdded is true
                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.CartAdded = @cartAdded")
                    .WithParameter("@cartAdded", true);

                var feedIterator = _container.GetItemQueryIterator<CosmosItem>(queryDefinition);

                while (feedIterator.HasMoreResults)
                {
                    var response = await feedIterator.ReadNextAsync();
                    containers.AddRange(response);
                }

                _logger.LogInformation($"Retrieved {containers.Count} containers from the cart.");
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error retrieving containers from Cosmos DB: {ex.Message}");
                throw new Exception($"Error retrieving containers from Cosmos DB: {ex.Message}");
            }

            return containers;
        }

        public async Task DeleteDuplicateContainersAsync()
        {
            try
            {
                var containers = await GetAllContainersInCartAsync();

                var duplicates = containers
                    .GroupBy(c => c.ContainerNumber)
                    .Where(g => g.Count() > 1)
                    .ToList();

                foreach (var duplicateGroup in duplicates)
                {
                    var containersToDelete = duplicateGroup.Skip(1);

                    foreach (var container in containersToDelete)
                    {
                        try
                        {
                            _logger.LogInformation($"Deleting duplicate container with ContainerNumber: {container.ContainerNumber}. Id: {container.Id}");
                            await _container.DeleteItemAsync<CosmosItem>(container.Id, new PartitionKey(container.ContainerNumber));
                        }
                        catch (CosmosException ex)
                        {
                            _logger.LogError($"Error deleting container with ContainerNumber: {container.ContainerNumber}. Id: {container.Id}: {ex.Message}");
                        }
                    }
                }

                _logger.LogInformation("Duplicate containers have been deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting duplicate containers: {ex.Message}");
                throw;
            }
        }
        public async Task RemoveContainerFromCartAsync(string containerNumber)
        {
            try
            {
                // Partition key is the ContainerNumber in this case
                var partitionKey = containerNumber;

                _logger.LogInformation($"Checking if container with ContainerNumber: {containerNumber} exists for removal.");

                // Query to find the container by its ContainerNumber
                var queryDefinition = new QueryDefinition("SELECT * FROM c WHERE c.containerNumber = @containerNumber")
                    .WithParameter("@containerNumber", containerNumber);

                var feedIterator = _container.GetItemQueryIterator<CosmosItem>(
                    queryDefinition,
                    requestOptions: new QueryRequestOptions { PartitionKey = new PartitionKey(partitionKey) }
                );

                var existingContainer = await feedIterator.ReadNextAsync();

                // If the container exists, proceed to delete it
                if (existingContainer.Any())
                {
                    var containerItem = existingContainer.First();
                    _logger.LogInformation($"Container with ContainerNumber: {containerItem.ContainerNumber} found. Proceeding to delete.");

                    // Delete the item from Cosmos DB using its ID and PartitionKey
                    await _container.DeleteItemAsync<CosmosItem>(containerItem.Id, new PartitionKey(partitionKey));
                    _logger.LogInformation($"Container {containerItem.ContainerNumber} removed from the cart.");
                }
                else
                {
                    _logger.LogWarning($"Container with ContainerNumber: {containerNumber} not found in the cart.");
                }
            }
            catch (CosmosException ex)
            {
                _logger.LogError($"Error removing container with ContainerNumber: {containerNumber} from Cosmos DB: {ex.Message}");
                throw new Exception($"Error removing container with ContainerNumber: {containerNumber} from Cosmos DB: {ex.Message}");
            }
        }

    }
}
