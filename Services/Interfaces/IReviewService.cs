using Data.Entities;
using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<IEnumerable<ReviewResponse>> GetProductReviewsByIdAsync(int productId);
        Task<int> GetReviewsRateByIdAsync(int id);
        Task<Review> CreateReviewAsync(ReviewRequest reviewRequest);

        Task<ReviewDto?> UpdateReviewAsync(int id, ReviewDto reviewDto);

        Task<ReviewDto?> DeleteReviewAsync(int id);
    }
}