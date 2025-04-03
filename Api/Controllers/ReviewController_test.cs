using Api.Controllers;
using Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Xunit;

public class ReviewController_test
{
    private readonly Mock<IReviewService> _mockReviewService;
    private readonly ReviewController _controller;

    public ReviewController_test()
    {
        _mockReviewService = new Mock<IReviewService>();
        _controller = new ReviewController(_mockReviewService.Object);
    }

    [Fact]
    public async Task GetAllReviews_ShouldReturnOk()
    {
        _mockReviewService.Setup(s => s.GetAllReviewsAsync()).ReturnsAsync(new List<ReviewDto>());

        var result = await _controller.GetAllReviews();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ReviewDto>>(ok.Value);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnOk_WhenExists()
    {
        var review = new ReviewDto { Id = 1 };
        _mockReviewService.Setup(s => s.GetReviewByIdAsync(1)).ReturnsAsync(review);

        var result = await _controller.GetReviewById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ReviewDto>(ok.Value);
        Assert.Equal(1, returned.Id);
    }

    [Fact]
    public async Task GetReviewById_ShouldReturnNotFound_WhenMissing()
    {
        _mockReviewService.Setup(s => s.GetReviewByIdAsync(99)).ReturnsAsync((ReviewDto?)null);

        var result = await _controller.GetReviewById(99);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task GetReviewsById_ShouldReturnOk()
    {
        _mockReviewService.Setup(s => s.GetProductReviewsByIdAsync(1)).ReturnsAsync(new List<ReviewResponse>());

        var result = await _controller.GetReviewsById(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<IEnumerable<ReviewResponse>>(ok.Value);
    }

    [Fact]
    public async Task CreateReview_ShouldReturnOk_WhenValid()
    {
        var request = new ReviewRequest { Comment = "Nice!", ProductId = 1, Rate = 5, Username = "user" };
        var created = new Review { Comment = "Nice!", ProductId = 1, Rate = 5, Username = "user" };

        _mockReviewService.Setup(s => s.CreateReviewAsync(request)).ReturnsAsync(created);

        var result = await _controller.CreateReview(request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ReviewResponse>(ok.Value);
        Assert.Equal("Nice!", response.Comment);
        Assert.Equal(1, response.ProductId);
    }

    [Fact]
    public async Task UpdateReview_ShouldReturnOk_WhenUpdated()
    {
        var dto = new ReviewDto { Id = 1, Comment = "Updated comment" };
        _mockReviewService.Setup(s => s.UpdateReviewAsync(dto)).ReturnsAsync(dto);

        var result = await _controller.UpdateReview(dto);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ReviewDto>(ok.Value);
        Assert.Equal("Updated comment", returned.Comment);
    }

    [Fact]
    public async Task UpdateReview_ShouldReturnNotFound_WhenNull()
    {
        var dto = new ReviewDto { Id = 99, Comment = "Not found" };
        _mockReviewService.Setup(s => s.UpdateReviewAsync(dto)).ReturnsAsync((ReviewDto?)null);

        var result = await _controller.UpdateReview(dto);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturnOk_WhenDeleted()
    {
        var deleted = new ReviewDto { Id = 1 };
        _mockReviewService.Setup(s => s.DeleteReviewAsync(1)).ReturnsAsync(deleted);

        var result = await _controller.DeleteReview(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var returned = Assert.IsType<ReviewDto>(ok.Value);
        Assert.Equal(1, returned.Id);
    }

    [Fact]
    public async Task DeleteReview_ShouldReturnNotFound_WhenNull()
    {
        _mockReviewService.Setup(s => s.DeleteReviewAsync(99)).ReturnsAsync((ReviewDto?)null);

        var result = await _controller.DeleteReview(99);

        Assert.IsType<NotFoundResult>(result);
    }
}
