using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Xunit;

public class ReviewService_test
{
    private readonly VinnareDbContext _context;
    private readonly Mock<ILogger<ReviewService>> _mockLogger;
    private readonly Mock<IUserService> _mockUserService;
    private readonly ReviewService _reviewService;

    public ReviewService_test()
    {
        _context = TestDbContextFactory.Create();
        _mockLogger = new Mock<ILogger<ReviewService>>();
        _mockUserService = new Mock<IUserService>();
        _reviewService = new ReviewService(_context, _mockLogger.Object, _mockUserService.Object);
    }

    [Fact]
    public async Task GetAllReviewsAsync_ShouldReturnAll()
    {
        _context.Reviews.Add(new Review { ProductId = 1, UserId = Guid.NewGuid(), Rate = 5, Comment = "Great", Username = "testuser2", });
        await _context.SaveChangesAsync();

        var result = await _reviewService.GetAllReviewsAsync();

        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetReviewByIdAsync_ShouldReturnCorrectReview()
    {
        var review = new Review { Id = 10, ProductId = 2, UserId = Guid.NewGuid(), Rate = 4, Comment = "Nice", Username = "testuser3", };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var result = await _reviewService.GetReviewByIdAsync(10);

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
    }

    [Fact]
    public async Task GetReviewByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _reviewService.GetReviewByIdAsync(999);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductReviewsByIdAsync_ShouldReturnMatchingReviews()
    {
        _context.Reviews.AddRange(
            new Review { ProductId = 5, Username = "A", Comment = "Nice" },
            new Review { ProductId = 5, Username = "B", Comment = "Cool" }
        );
        await _context.SaveChangesAsync();

        var result = await _reviewService.GetProductReviewsByIdAsync(5);

        Assert.Equal(2, result.Count());
        Assert.All(result, r => Assert.Equal(5, r.ProductId));
    }

    [Fact]
    public async Task GetReviewsRateByIdAsync_ShouldReturnCorrectAverage()
    {
        var userId = Guid.NewGuid();
        _context.Reviews.AddRange(
            new Review { Id = 1, ProductId = 15, UserId = userId, Rate = 3, Username = "testuser2" },
            new Review { Id = 2, ProductId = 15, UserId = userId, Rate = 5, Username = "testuser3" }
        );
        await _context.SaveChangesAsync();

        var result = await _reviewService.GetReviewsRateByIdAsync(15);

        Assert.Equal(4, result); // (3 + 5) / 2
    }

    [Fact]
    public async Task CreateReviewAsync_ShouldCreateAndReturnReview()
    {
        var userId = Guid.NewGuid();
        _context.Products.Add(new Product { Id = 99, Title = "X", Price = 5 });
        await _context.SaveChangesAsync();

        _mockUserService.Setup(s => s.GetIdByUsername("testuser")).ReturnsAsync(userId);

        var request = new ReviewRequest
        {
            ProductId = 99,
            Username = "testuser",
            Rate = 4,
            Comment = "Solid"
        };

        var exception = await Assert.ThrowsAsync<Exception>(() =>
        _reviewService.CreateReviewAsync(request));

        Assert.Equal("The user hasn't bought the product.", exception.Message);
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldModifyReview()
    {
        var review = new Review { Id = 7, ProductId = 3, UserId = Guid.NewGuid(), Rate = 2, Comment = "Meh", Username = "testuser4" };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var dto = new ReviewDto
        {
            Id = 7,
            ProductId = 3,
            UserId = review.UserId,
            Rate = 5,
            Comment = "Improved"
        };

        var result = await _reviewService.UpdateReviewAsync(dto);

        Assert.NotNull(result);
        Assert.Equal("Improved", result!.Comment);
        Assert.Equal(5, result.Rate);
    }

    [Fact]
    public async Task UpdateReviewAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _reviewService.UpdateReviewAsync(new ReviewDto());
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteReviewAsync_ShouldRemoveAndReturnDto()
    {
        var review = new Review { Id = 11, ProductId = 4, UserId = Guid.NewGuid(), Rate = 3, Comment = "Okay", Username = "testuser", };
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var result = await _reviewService.DeleteReviewAsync(11);

        Assert.NotNull(result);
        Assert.Equal(11, result!.Id);
        Assert.Null(await _context.Reviews.FindAsync(11));
    }

    [Fact]
    public async Task DeleteReviewAsync_ShouldReturnNull_WhenNotFound()
    {
        var result = await _reviewService.DeleteReviewAsync(404);
        Assert.Null(result);
    }
}
