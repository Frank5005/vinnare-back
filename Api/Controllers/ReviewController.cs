using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/reviews")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        // GET: api/reviews
        [HttpGet]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }

        // GET: api/reviews/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReviewById(int id)
        {
            var review = await _reviewService.GetReviewByIdAsync(id);
            if (review == null) return NotFound();
            return Ok(review);
        }

        // Get: api/reviews/store/{id}
        [HttpGet("store/{id:int}")]
        public async Task<IActionResult> GetReviewsById(int productId)
        {
            var reviews = await _reviewService.GetProductReviewsByIdAsync(productId);
            return Ok(reviews);
        }

        // POST: api/reviews
        [HttpPost("add")]
        public async Task<IActionResult> CreateReview([FromBody] ReviewRequest reviewRequest)
        {
            if (reviewRequest == null) return BadRequest("Review data is required.");

            var createdReview = await _reviewService.CreateReviewAsync(reviewRequest);
            //return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, createdReview);
            return Ok(new ReviewResponse { Username = createdReview.Username, ProductId = createdReview.ProductId, Comment = createdReview.Comment });
        }

        // UPDATE: api/reviews/update
        [HttpPut("update")]
        public async Task<IActionResult> UpdateReview([FromBody] ReviewDto reviewDto)
        {
            if (reviewDto == null) return BadRequest("Review data is required.");

            var updatedReview = await _reviewService.UpdateReviewAsync(reviewDto);
            if (updatedReview == null) return NotFound();
            return Ok(updatedReview);
        }

        // DELETE: api/reviews/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var deletedReview = await _reviewService.DeleteReviewAsync(id);
            if (deletedReview == null) return NotFound();
            return Ok(deletedReview);
        }
    }
}
