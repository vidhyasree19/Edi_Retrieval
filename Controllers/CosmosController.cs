using Microsoft.AspNetCore.Mvc;
using EdiRetrieval.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace EdiRetrieval.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CosmosDataController : ControllerBase
    {
        private readonly CosmosDbService _cosmosDbService;

        public CosmosDataController(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetItems()
        {
            var items = await _cosmosDbService.GetAllItemsAsync();

            foreach (var item in items)
            {
                Console.WriteLine($"Container Number: {item.ContainerNumber}");
            }

            return Ok(items);
        }

        [HttpGet("{containerNumber}")]
        [Authorize]
        public async Task<IActionResult> GetContainer(string containerNumber)
        {
            var container = await _cosmosDbService.GetContainerByContainerNoAsync(containerNumber);

            if (container == null)
            {
                return NotFound();
            }

            return Ok(container);
        }
        [HttpDelete("{containerNumber}")]
        [Authorize]

        public async Task<IActionResult> DeleteContainer(string containerNumber)
        {
            await _cosmosDbService.DeleteContainerByContainerNoAsync(containerNumber);
            return Ok($"Container {containerNumber} deleted successfully.");
        }

        // [HttpPost("transfer/{containerNumber}")]
        // // [Authorize]
        // public async Task<IActionResult> TransferItems(string containerNumber)
        // {
        //     try
        //     {
        //         await _cosmosDbService.TransferItemsToSqlAsync(containerNumber); 
        //         return Ok("Item transferred successfully."); 
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error transferring item: {ex.Message}");
        //         return StatusCode(500, "Internal server error while transferring item.");
        //     }
        // }

        // [HttpGet("items/{containerNumber}")]
        // // [Authorize]
        // public async Task<IActionResult> GetItemsByContainerNumber(string containerNumber)
        // {
        //     try
        //     {
        //         // Assuming GetItemsByContainerNumberAsync retrieves items related to a specific container
        //         var items = await _cosmosDbService.GetItemsByContainerNumberAsync(containerNumber);

        //         if (items == null || !items.Any())
        //         {
        //             return NotFound($"No items found for container number {containerNumber}.");
        //         }

        //         return Ok(items);  // Return items found
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error retrieving items for container {containerNumber}: {ex.Message}");
        //         return StatusCode(500, "Internal server error while retrieving items.");
        //     }
        // }
    }
}
