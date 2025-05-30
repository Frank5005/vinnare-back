using System.Security.Claims;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Builders;
using Services.Interfaces;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/purchases")]
    [ApiController]
    [Authorize(Roles = "Shopper")]
    public class PurchaseController : ControllerBase
    {
        private readonly ICartPurchaseBuilderFactory _purchaseFactory;
        private readonly IPurchaseService _purchaseService;
        private readonly IUserService _userService;
        private readonly ILogger<PurchaseController> _logger;
        public PurchaseController(IPurchaseService purchaseService, IUserService userService, ILogger<PurchaseController> logger, ICartPurchaseBuilderFactory cartPurchasesBuilderFactory)
        {
            _purchaseFactory = cartPurchasesBuilderFactory;
            _purchaseService = purchaseService;
            _userService = userService;
            _logger = logger;
        }

        // GET: api/purchases
        //ADD ORDER TOTAL
        [HttpGet]
        public async Task<IActionResult> GetAllUserPurchases()
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");
            var purchases = await _purchaseService.GetAllUserPurchases(userId);
            return Ok(purchases);
        }

        // POST: api/purchases
        [HttpPost]
        public async Task<IActionResult> Buy([FromBody] PurchaseRequest? purchaseDto)
        {
            try
            {
                var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");

                var builder = _purchaseFactory.Create(userId);
                var preview = (await (await
                    builder.LoadCartAsync())
                    .ValidateApproved()
                    .ValidateStock()
                    .CalcBasePrice()
                    .FindCoupon(purchaseDto?.coupon_code ?? null))
                    .CalcFinalPrice();

                try
                {
                    var result = (await (await (await preview
                        .BeginTransactionAsync())
                        .DecrementStock()
                        .CreatePurchase()
                        .ClearCart()
                        .PersistAllChangesAsync())
                        .CommitTransactionAsync())
                        .AddMetricsData()
                        .FormatOutput();
                    return Created("", result);
                }
                catch (NotFoundException ex)
                {
                    _logger.LogError(ex, "Resource not found: {Message}", ex.Message);
                    return NotFound(new { error = ex.Message });
                }
                catch (GoneException ex)
                {
                    _logger.LogError(ex, "Resource no longer available: {Message}", ex.Message);
                    return StatusCode(410, new { error = ex.Message });
                }
                catch (BadRequestException ex)
                {
                    _logger.LogError(ex, "Bad request: {Message}", ex.Message);
                    return BadRequest(new { error = ex.Message });
                }
                catch (Exception ex)
                {
                    await builder.RollbackTransactionAsync();
                    _logger.LogError(ex, "Error during purchase process: {Message}", ex.Message);
                    var innerException = ex.InnerException?.Message ?? "No inner exception";
                    var stackTrace = ex.InnerException?.StackTrace ?? ex.StackTrace;
                    return StatusCode(500, new { 
                        error = "Error during purchase process", 
                        details = ex.Message,
                        innerException = innerException,
                        stackTrace = stackTrace
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial setup: {Message}", ex.Message);
                return StatusCode(500, new { error = "Error during initial setup", details = ex.Message });
            }
        }

        // POST: api/purchases/preview
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] PurchaseRequest? purchaseDto)
        {
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = await _userService.GetIdByEmail(tokenUsername) ?? throw new BadRequestException("User does not exist");

            var builder = _purchaseFactory.Create(userId);
            var result = (await (await builder.LoadCartAsync())
                .ValidateApproved()
                .ValidateStock()
                .CalcBasePrice()
                .FindCoupon(purchaseDto?.coupon_code ?? null))
                .CalcFinalPrice()
                .FormatOutput();

            return Ok(result);
        }
    }
}
