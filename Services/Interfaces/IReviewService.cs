using Shared.DTOs;

namespace Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllReviewsAsync();
        Task<ReviewDto?> GetReviewByIdAsync(int id);
        Task<int> GetReviewsRateByIdAsync(int id);
        Task<ReviewDto> CreateReviewAsync(ReviewDto reviewDto);

        Task<ReviewDto?> UpdateReviewAsync(int id, ReviewDto reviewDto);

        Task<ReviewDto?> DeleteReviewAsync(int id);
    }
}