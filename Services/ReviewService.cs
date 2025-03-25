using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class ReviewService : IReviewService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(VinnareDbContext context, ILogger<ReviewService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    Rate = r.Rate,
                    Comment = r.Comment
                })
                .ToListAsync();
        }

        public async Task<ReviewDto?> GetReviewByIdAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return null;

            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                Rate = review.Rate,
                Comment = review.Comment
            };
        }

        public async Task<int>  GetReviewsRateByIdAsync(int id)
        {
            int rate = 0, sum = 0, count = 0;
            List<ReviewDto> reviews = await _context.Reviews
                .Select(r => new ReviewDto
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    Rate = r.Rate,
                    Comment = r.Comment
                })
                .ToListAsync();
            
            for(int i = 0; i < reviews.Count; i++)
            {
                if(id == reviews[i].Id){
                    count += 1;
                    sum += reviews[i].Rate;
                }
            }

            if (count > 0)
            {
                rate = sum / count;
            }

            return rate;
        }

        public async Task<ReviewDto> CreateReviewAsync(ReviewDto reviewDto)
        {
            var review = new Review
            {
                ProductId = reviewDto.ProductId,
                UserId = reviewDto.UserId,
                Rate = reviewDto.Rate,
                Comment = reviewDto.Comment
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return reviewDto;
        }

        public async Task<ReviewDto?> UpdateReviewAsync(int id, ReviewDto reviewDto)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return null;

            review.ProductId = reviewDto.ProductId;
            review.UserId = reviewDto.UserId;
            review.Rate = reviewDto.Rate;
            review.Comment = reviewDto.Comment;

            await _context.SaveChangesAsync();

            return reviewDto;
        }

        public async Task<ReviewDto?> DeleteReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return null;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            return new ReviewDto
            {
                Id = review.Id,
                ProductId = review.ProductId,
                UserId = review.UserId,
                Rate = review.Rate,
                Comment = review.Comment
            };
        }
    }
}
