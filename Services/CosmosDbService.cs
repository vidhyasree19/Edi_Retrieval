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
                        ContainerNumber = cosmosItem.containerNumber,
                        TradeType = cosmosItem.TradeType,
                        Status = cosmosItem.Status,
                        VesselName = cosmosItem.VesselName,
                        VesselCode = cosmosItem.VesselCode,
                        Voyage = cosmosItem.Voyage,
                        Origin = cosmosItem.Origin,
                        Line = cosmosItem.Line,
                        Destination = cosmosItem.Destination,
                        SizeType = cosmosItem.SizeType
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
                    return item; // Return first match from Cosmos DB
                }
            }

            return null;
        }

        public async Task TransferItemsToSqlAsync(string containerNumber)
        {
            // Get items from Cosmos DB
            var items = await GetAllItemsAsync();

            // Find the specific item based on the container number
            var itemToTransfer = items.FirstOrDefault(item => item.ContainerNumber == containerNumber);

            if (itemToTransfer != null)
            {
                // Check if the item already exists in MSSQL
                var existingItem = await _context.CosmosItems
                    .FirstOrDefaultAsync(i => i.ContainerNumber == itemToTransfer.ContainerNumber);

                if (existingItem == null) // If it doesn't exist, add it to MSSQL
                {
                    _context.CosmosItems.Add(itemToTransfer); // Add the item to the DbSet
                    await _context.SaveChangesAsync(); // Save the changes to MSSQL
                    Console.WriteLine("Item added successfully.");
                }
                else // If the item already exists, show a message
                {
                    Console.WriteLine("Item with the specified ContainerNumber already exists.");
                }
            }
            else
            {
                // If no item with the specified container number is found in Cosmos DB
                throw new Exception("Item with the specified ContainerNumber not found in Cosmos DB.");
            }
        }



    }
}
