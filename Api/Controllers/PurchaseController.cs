using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/purchases")]
    [ApiController]
    public class PurchaseController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchaseController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        // GET: api/purchases
        [HttpGet]
        public async Task<IActionResult> GetAllPurchases()
        {
            var purchases = await _purchaseService.GetAllPurchasesAsync();
            return Ok(purchases);
        }

        // GET: api/purchases/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetPurchaseById(int id)
        {
            var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
            if (purchase == null) return NotFound();
            return Ok(purchase);
        }

        // POST: api/purchases
        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseDto purchaseDto)
        {
            if (purchaseDto == null) return BadRequest("Purchase data is required.");

            var createdPurchase = await _purchaseService.CreatePurchaseAsync(purchaseDto);
            return CreatedAtAction(nameof(GetPurchaseById), new { id = createdPurchase.Id }, createdPurchase);
        }

        // UPDATE: api/purchases/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdatePurchase(int id, [FromBody] PurchaseDto purchaseDto)
        {
            if (purchaseDto == null) return BadRequest("Purchase data is required.");

            var updatedPurchase = await _purchaseService.UpdatePurchaseAsync(id, purchaseDto);
            if (updatedPurchase == null) return NotFound();
            return Ok(updatedPurchase);
        }

        // DELETE: api/purchases/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeletePurchase(int id)
        {
            var deletedPurchase = await _purchaseService.DeletePurchaseAsync(id);
            if (deletedPurchase == null) return NotFound();
            return Ok(deletedPurchase);
        }
    }
}
