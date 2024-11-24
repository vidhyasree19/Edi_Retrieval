using Microsoft.AspNetCore.Mvc;
using EdiRetrieval.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace EdiRetrieval.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        [HttpGet("get-all")]
        [Authorize]

        public async Task<ActionResult<List<CosmosItem>>> GetAllContainersInCart()
        {
            try
            {
                var containers = await _cartService.GetAllContainersInCartAsync();

                return Ok(containers); // Return a 200 OK with the list of containers
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("add")]
        [Authorize]

        public async Task<ActionResult> AddContainerToCart([FromBody] CosmosItem containerItem)
        {
            try
            {
                if (containerItem == null)
                {
                    return BadRequest("Container data is required.");
                }


                containerItem.CartAdded = true;

                if (string.IsNullOrEmpty(containerItem.Id))
                {
                    containerItem.Id = containerItem.ContainerNumber;
                }

                await _cartService.AddContainerToCartAsync(containerItem);

                return Ok($"Container with number {containerItem.ContainerNumber} has been added to the cart.");
            }
            catch (System.Exception ex)
            {
                // Handle errors gracefully
                return StatusCode(500, $"Error adding container: {ex.Message}");
            }
        }


        [HttpDelete("delete-duplicates")]
        [Authorize]

        public async Task<ActionResult> DeleteDuplicateContainers()
        {
            try
            {
                await _cartService.DeleteDuplicateContainersAsync();
                return Ok("Duplicate containers have been deleted.");
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"Error deleting duplicates: {ex.Message}");
            }
        }
        [HttpDelete("remove/{containerNumber}")]
        [Authorize]

        public async Task<ActionResult> RemoveContainerFromCart(string containerNumber)
        {
            try
            {
                if (string.IsNullOrEmpty(containerNumber))
                {
                    return BadRequest("Container number is required.");
                }

                // Call the service to remove the container
                await _cartService.RemoveContainerFromCartAsync(containerNumber);

                // Return success response
                return Ok($"Container with number {containerNumber} has been removed from the cart.");
            }
            catch (System.Exception ex)
            {
                // Handle errors gracefully
                return StatusCode(500, $"Error removing container: {ex.Message}");
            }
        }

    }
}
