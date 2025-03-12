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

        // POST: api/reviews
        [HttpPost]
        public async Task<IActionResult> CreateReview([FromBody] ReviewDto reviewDto)
        {
            if (reviewDto == null) return BadRequest("Review data is required.");

            var createdReview = await _reviewService.CreateReviewAsync(reviewDto);
            return CreatedAtAction(nameof(GetReviewById), new { id = createdReview.Id }, createdReview);
        }

        // UPDATE: api/reviews/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateReview(int id, [FromBody] ReviewDto reviewDto)
        {
            if (reviewDto == null) return BadRequest("Review data is required.");

            var updatedReview = await _reviewService.UpdateReviewAsync(id, reviewDto);
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
