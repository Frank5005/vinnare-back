using Api.Controllers;
using Api.DTOs;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;
using Xunit;

public class JobController_test
{
    private readonly Mock<IJobService> _mockJobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<ILogger<JobController>> _mockLogger;
    private readonly JobController _controller;

    public JobController_test()
    {
        _mockJobService = new Mock<IJobService>();
        _mockUserService = new Mock<IUserService>();
        _mockProductService = new Mock<IProductService>();
        _mockCategoryService = new Mock<ICategoryService>();
        _mockLogger = new Mock<ILogger<JobController>>();

        _controller = new JobController(
            _mockJobService.Object,
            _mockUserService.Object,
            _mockProductService.Object,
            _mockCategoryService.Object,
            _mockLogger.Object
        );
    }

    [Fact]
    public async Task ReviewJob_ShouldReturnSuccess_WhenProductJobIsApproved()
    {
        // Arrange
        var request = new ReviewJobRequest { Id = 1, Action = "Approve", Type = "Product" };
        var job = new JobDto
        {
            Id = 1,
            Type = JobType.Product,
            Operation = OperationType.Create,
            ProductId = 10
        };

        _mockJobService.Setup(s => s.GetJobByIdAsync(1)).ReturnsAsync(job);

        _mockProductService.Setup(s => s.ApproveProduct(10, true)).Returns(Task.CompletedTask);
        _mockJobService.Setup(s => s.RemoveJob(1)).ReturnsAsync(true);

        // Act
        var result = await _controller.ReviewJob(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Success Approve", ((DefaultResponse)result.Value!).message);
        _mockProductService.Verify(s => s.ApproveProduct(10, true), Times.Once);
        _mockJobService.Verify(s => s.RemoveJob(1), Times.Once);
    }

    [Fact]
    public async Task ReviewJob_ShouldReturnSuccess_WhenCategoryJobIsDeclined()
    {
        // Arrange
        var request = new ReviewJobRequest { Id = 2, Action = "Decline", Type = "Category"  };
        var job = new JobDto
        {
            Id = 2,
            Type = JobType.Category,
            Operation = OperationType.Create,
            CategoryId = 20
        };

        _mockJobService.Setup(s => s.GetJobByIdAsync(2)).ReturnsAsync(job);

        // Act
        var result = await _controller.ReviewJob(request) as OkObjectResult;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.Equal("Success Decline", ((DefaultResponse)result.Value!).message);
        _mockCategoryService.Verify(s => s.ApproveCategory(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
    }

    [Fact]
    public async Task ReviewJob_ShouldThrowBadRequestException_WhenTypeIsMissing()
    {
        // Arrange
        var request = new ReviewJobRequest { Id = 1, Action = "Approve"};

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _controller.ReviewJob(request));
    }

    [Fact]
    public async Task ReviewJob_ShouldThrowNotFoundException_WhenJobDoesNotExist()
    {
        // Arrange
        var request = new ReviewJobRequest { Id = 999, Action = "Approve", Type = "Product" };

        _mockJobService.Setup(s => s.GetJobByIdAsync(999)).ReturnsAsync((JobDto?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _controller.ReviewJob(request));
    }

    [Fact]
    public async Task ReviewJob_ShouldThrowBadRequestException_WhenTypeDoesNotMatchJob()
    {
        // Arrange
        var request = new ReviewJobRequest { Id = 1, Action = "Approve", Type = "Product"};
        var job = new JobDto { Id = 1, Type = JobType.Category };

        _mockJobService.Setup(s => s.GetJobByIdAsync(1)).ReturnsAsync(job);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _controller.ReviewJob(request));
    }
    //==== TEST FOR HANDLE PRODUCT JOB HELPER ====

    [Fact]
    public async Task HandleProductJob_ShouldApproveAndRemoveJob_WhenProductIsCreatedAndApproved()
    {
        // Arrange
        var job = new JobDto { Id = 1, ProductId = 10, Operation = OperationType.Create };

        // Act
        await _controller.HandleProductJob(job, accepted: true, isCreate: true);

        // Assert
        _mockProductService.Verify(s => s.ApproveProduct(10, true), Times.Once);
        _mockJobService.Verify(s => s.RemoveJob(1), Times.Once);
    }

    [Fact]
    public async Task HandleProductJob_ShouldDeleteProduct_WhenProductIsCreatedAndDeclined()
    {
        var job = new JobDto { Id = 2, ProductId = 20, Operation = OperationType.Create };

        await _controller.HandleProductJob(job, accepted: false, isCreate: true);

        _mockProductService.Verify(s => s.DeleteProductAsync(20), Times.Once);
    }

    [Fact]
    public async Task HandleProductJob_ShouldDeleteProduct_WhenProductDeletionIsApproved()
    {
        var job = new JobDto { Id = 3, ProductId = 30, Operation = OperationType.Delete };

        await _controller.HandleProductJob(job, accepted: true, isCreate: false);

        _mockProductService.Verify(s => s.DeleteProductAsync(30), Times.Once);
    }

    [Fact]
    public async Task HandleProductJob_ShouldRestoreProductAndRemoveJob_WhenProductDeletionIsDeclined()
    {
        var job = new JobDto { Id = 4, ProductId = 40, Operation = OperationType.Delete };

        await _controller.HandleProductJob(job, accepted: false, isCreate: false);

        _mockProductService.Verify(s => s.ApproveProduct(40, true), Times.Once);
        _mockJobService.Verify(s => s.RemoveJob(4), Times.Once);
    }



    //=== test for HandleCategoryJob
    [Fact]
    public async Task HandleCategoryJob_ShouldApproveAndRemoveJob_WhenCategoryIsCreatedAndApproved()
    {
        var job = new JobDto { Id = 5, CategoryId = 50, Operation = OperationType.Create };

        await _controller.HandleCategoryJob(job, accepted: true, isCreate: true);

        _mockCategoryService.Verify(s => s.ApproveCategory(50, true), Times.Once);
        _mockJobService.Verify(s => s.RemoveJob(5), Times.Once);
    }

    [Fact]
    public async Task HandleCategoryJob_ShouldDoNothing_WhenCategoryCreationIsDeclined() //
    {
        var job = new JobDto { Id = 6, CategoryId = 60, Operation = OperationType.Create };

        await _controller.HandleCategoryJob(job, accepted: false, isCreate: true);

        //_mockCategoryService.Verify(s => s.ApproveCategory(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
        _mockJobService.Verify(s => s.RemoveJob(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task HandleCategoryJob_ShouldDeleteCategory_WhenCategoryDeletionIsApproved()
    {
        var job = new JobDto { Id = 7, CategoryId = 70, Operation = OperationType.Delete };

        await _controller.HandleCategoryJob(job, accepted: true, isCreate: false);

        _mockCategoryService.Verify(s => s.DeleteCategoryAsync(70), Times.Once);
    }

    [Fact]
    public async Task HandleCategoryJob_ShouldRestoreCategoryAndRemoveJob_WhenCategoryDeletionIsDeclined() //
    {
        var job = new JobDto { Id = 8, CategoryId = 80, Operation = OperationType.Delete };

        await _controller.HandleCategoryJob(job, accepted: false, isCreate: false);

        //_mockCategoryService.Verify(s => s.ApproveCategory(80, true), Times.Once);
        _mockJobService.Verify(s => s.RemoveJob(8), Times.Once);
    }



}
