using EdiRetrieval.Data;  // DbContext
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EdiRetrieval.Services
{
    public class CosmosDbService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;
        private readonly ILogger<CosmosDbService> _logger;
        private readonly ApplicationDbContext _context;

        public CosmosDbService(IConfiguration configuration, ILogger<CosmosDbService> logger, ApplicationDbContext context)
        {
            _context = context;

            var connectionString = configuration["CosmosDb:ConnectionString"];
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];

            _cosmosClient = new CosmosClient(connectionString);
            _container = _cosmosClient.GetContainer(databaseName, containerName);
            _logger = logger;
        }

        public async Task<List<CosmosItem>> GetAllItemsAsync()
        {
            var items = new List<CosmosItem>();
            var queryDefinition = new QueryDefinition("SELECT * FROM c");

            var feedIterator = _container.GetItemQueryIterator<dynamic>(queryDefinition);

            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                foreach (var cosmosItem in response)
                {
                    var cosmosItemModel = new CosmosItem
                    {
                        Id=cosmosItem.id,
                        ContainerNumber = cosmosItem.containerNumber,
                        TradeType = cosmosItem.TradeType,
                        Status = cosmosItem.Status,
                        VesselName = cosmosItem.VesselName,
                        VesselCode = cosmosItem.VesselCode,
                        Voyage = cosmosItem.Voyage,
                        Origin = cosmosItem.Origin,
                        Line = cosmosItem.Line,
                        Destination = cosmosItem.Destination,

                        SizeType = cosmosItem.SizeType,
                        Fees=cosmosItem.Fees
                    };

                    items.Add(cosmosItemModel);
                }
            }

            return items;
        }

        public async Task<CosmosItem> GetContainerByContainerNoAsync(string containerNumber)
        {
            var query = new QueryDefinition("SELECT * FROM c WHERE c.containerNumber = @ContainerNumber")
                        .WithParameter("@ContainerNumber", containerNumber);

            var iterator = _container.GetItemQueryIterator<CosmosItem>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                foreach (var item in response)
                {
                    return item; 
                }
            }

            return null;
        }
        
    
public async Task DeleteContainerByContainerNoAsync(string containerNumber)
        {
            try
            {
                // Fetch the container item first to ensure it exists
                var containerItem = await GetContainerByContainerNoAsync(containerNumber);
                if (containerItem != null)
                {
                    // Deleting the item from Cosmos DB by containerNumber
                    await _container.DeleteItemAsync<CosmosItem>(containerNumber, new PartitionKey(containerItem.ContainerNumber));
                    _logger.LogInformation($"Container with ContainerNumber {containerNumber} deleted successfully from Cosmos DB.");
                }
                else
                {
                    _logger.LogWarning($"Container with ContainerNumber {containerNumber} not found.");
                }
            }
            catch (CosmosException ex)
            {
                // Handle potential Cosmos exceptions, like NotFoundException or UnauthorizedException
                _logger.LogError($"Error deleting container with ContainerNumber {containerNumber}: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                _logger.LogError($"An error occurred while deleting container with ContainerNumber {containerNumber}: {ex.Message}");
            }
        }
    

        // public async Task TransferItemsToSqlAsync(string containerNumber)
        // {
        //     var items = await GetAllItemsAsync();

        //     var itemToTransfer = items.FirstOrDefault(item => item.ContainerNumber == containerNumber);

        //     if (itemToTransfer != null)
        //     {
        //         var existingItem = await _context.CosmosItems
        //             .FirstOrDefaultAsync(i => i.ContainerNumber == itemToTransfer.ContainerNumber);

        //         if (existingItem == null) 
        //         {
        //             _context.CosmosItems.Add(itemToTransfer); 
        //             await _context.SaveChangesAsync(); 
        //             Console.WriteLine("Item added successfully.");
        //         }
        //         else 
        //         {
        //             Console.WriteLine("Item with the specified ContainerNumber already exists.");
        //         }
        //     }
        //     else
        //     {
        //         throw new Exception("Item with the specified ContainerNumber not found in Cosmos DB.");
        //     }
        // }

        // public async Task<IEnumerable<CosmosItem>> GetItemsByContainerNumberAsync(string containerNumber)
        // {
        //     try
        //     {
        //         var items = await _context.CosmosItems
        //             .Where(i => i.ContainerNumber == containerNumber)
        //             .ToListAsync();

        //         return items;
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error retrieving items for container {containerNumber}: {ex.Message}");
        //         throw;
        //     }
        // }


    }
}
