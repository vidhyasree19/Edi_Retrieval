using Microsoft.AspNetCore.Mvc;
using EdiRetrieval.Services;
using System.Collections.Generic;
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
        [HttpPost("transfer/{containerNumber}")]
        [Authorize]

        public async Task<IActionResult> TransferItems(string containerNumber)
        {
            try
            {
                await _cosmosDbService.TransferItemsToSqlAsync(containerNumber); 
                return Ok("Item transferred successfully."); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error transferring item: {ex.Message}");
                return StatusCode(500, "Internal server error while transferring item.");
            }
        }



    }
}
