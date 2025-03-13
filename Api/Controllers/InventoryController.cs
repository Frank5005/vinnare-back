using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/inventories")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: api/inventories
        [HttpGet]
        public async Task<IActionResult> GetAllInventories()
        {
            var inventories = await _inventoryService.GetAllInventoriesAsync();
            return Ok(inventories);
        }

        // GET: api/inventories/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetInventoryById(int id)
        {
            var inventory = await _inventoryService.GetInventoryByIdAsync(id);
            if (inventory == null) return NotFound();
            return Ok(inventory);
        }

        // POST: api/inventories
        [HttpPost]
        public async Task<IActionResult> CreateInventory([FromBody] InventoryDto inventoryDto)
        {
            if (inventoryDto == null) return BadRequest("Inventory data is required.");

            var createdInventory = await _inventoryService.CreateInventoryAsync(inventoryDto);
            return CreatedAtAction(nameof(GetInventoryById), new { id = createdInventory.Id }, createdInventory);
        }

        // UPDATE: api/inventories/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateInventory(int id, [FromBody] InventoryDto inventoryDto)
        {
            if (inventoryDto == null) return BadRequest("Inventory data is required.");

            var updatedInventory = await _inventoryService.UpdateInventoryAsync(id, inventoryDto);
            if (updatedInventory == null) return NotFound();
            return Ok(updatedInventory);
        }

        // DELETE: api/inventories/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            var deletedInventory = await _inventoryService.DeleteInventoryAsync(id);
            if (deletedInventory == null) return NotFound();
            return Ok(deletedInventory);
        }
    }
}
