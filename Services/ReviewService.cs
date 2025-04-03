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
        private readonly IUserService _userService;

        public ReviewService(VinnareDbContext context, ILogger<ReviewService> logger, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _userService = userService;
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

        public async Task<IEnumerable<ReviewResponse>> GetProductReviewsByIdAsync(int productId)
        {
            return await _context.Reviews
            .Where(r => r.ProductId == productId)
            .Select(r => new ReviewResponse
            {
                Username = r.Username,
                ProductId = r.ProductId,
                Comment = r.Comment
            })
            .ToListAsync();
        }

        public async Task<int> GetReviewsRateByIdAsync(int id)
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

            for (int i = 0; i < reviews.Count; i++)
            {
                if (id == reviews[i].ProductId)
                {
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

        public async Task<Review> CreateReviewAsync(ReviewRequest reviewRequest)
        {
            // Verify if the product exists
            var productExists = await _context.Products.AnyAsync(u => u.Id == reviewRequest.ProductId);
            if (!productExists)
            {
                throw new Exception("The product doesn't exists.");
            }

            // Verify if the user exists
            var userExists = await _userService.GetIdByUsername(reviewRequest.Username) != null;
            if (!userExists)
            {
                throw new Exception("The user doesn't exists.");
            }

            // Verify if the user has already bought the product
            var userId = await _userService.GetIdByUsername(reviewRequest.Username);
            var hasBoughtProduct = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.Products.Contains(reviewRequest.ProductId));
            if (!hasBoughtProduct)
            {
                throw new Exception("The user hasn't bought the product.");
            }

            var review = new Review
            {
                ProductId = reviewRequest.ProductId,
                UserId = (Guid)await _userService.GetIdByUsername(reviewRequest.Username),
                Rate = reviewRequest.Rate,
                Comment = reviewRequest.Comment,
                Username = reviewRequest.Username
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            return review;
        }

        public async Task<ReviewDto?> UpdateReviewAsync(ReviewDto reviewDto)
        {
            var review = await _context.Reviews.FindAsync(reviewDto.Id);
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
