using Data;
using Data.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Services.Interfaces;
using Services.Utils;
using Shared.DTOs;
using Shared.Enums;
using Xunit;

public class JobService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<JobService>> _mockLogger;
    private readonly JobService _jobService;
    private readonly Mock<IUserService> _mockUserService;
    private readonly Mock<IServiceProvider> _mockServiceProvider;
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly Mock<IProductService> _mockProductService;

    public JobService_test()
    {
        _dbContext = TestDbContextFactory.Create();
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<JobService>>();
        _mockUserService = new Mock<IUserService>();
        _mockServiceProvider = new Mock<IServiceProvider>();
        _mockCategoryService = new Mock<ICategoryService>();
        _mockProductService = new Mock<IProductService>();

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(ICategoryService)))
            .Returns(_mockCategoryService.Object);

        _mockServiceProvider.Setup(sp => sp.GetService(typeof(IProductService)))
            .Returns(_mockProductService.Object);

        _jobService = new JobService(_dbContext, _mockLogger.Object, _mockUserService.Object, _mockServiceProvider.Object);
    }

    /*
    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnAllJobsWithDetails()
    {
        var creatorId = Guid.NewGuid();
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();

        _dbContext.Jobs.Add(new Job
        {
            Id = 1,
            Type = JobType.Product,
            Operation = OperationType.Create,
            ProductId = 10,
            CreatorId = creatorId,
            Product = new Product { Id = 10 },
            Category = new Category { Id = 5 }
        });

        _dbContext.SaveChanges();

        _mockCategoryService.Setup(c => c.GetCategoryNameByIdAsync(5)).ReturnsAsync("Electronics");
        _mockProductService.Setup(p => p.GetProductNameByIdAsync(10)).ReturnsAsync("Smartphone");
        _mockUserService.Setup(u => u.GetUsernameById(creatorId)).ReturnsAsync("creatoruser");

        var result = await _jobService.GetAllJobsAsync();

        Assert.NotNull(result);
        var job = Assert.Single(result);
        Assert.Equal(1, job.id);
        Assert.Equal("Product", job.JobType);
        Assert.Equal("Create", job.Operation);
        Assert.Equal(10, job.AssociatedId);
        Assert.Equal("Electronics", job.CategoryName);
        Assert.Equal("Smartphone", job.ProductName);
        Assert.Equal("creatoruser", job.CreatorName);
    }
    */

    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnJob_WhenJobExists()
    {
        // Arrange
        var job = new Job
        {
            Id = 2,
            Type = JobType.Product,
            Operation = OperationType.Create,
            CreatorId = Guid.NewGuid(),
            ProductId = 20,
            CategoryId = null
        };
        _dbContext.Jobs.Add(job);
        _dbContext.SaveChanges();

        // Act
        var result = await _jobService.GetJobByIdAsync(2);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
        Assert.Equal(JobType.Product, result.Type);
        Assert.Equal(OperationType.Create, result.Operation);
        Assert.Equal(20, result.ProductId);
    }

    [Fact]
    public async Task GetJobByIdAsync_ShouldReturnNull_WhenJobDoesNotExist()
    {
        // Act
        var result = await _jobService.GetJobByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateJobAsync_ShouldAddJobToDatabase()
    {
        // Arrange
        var newJob = new JobDto
        {
            Type = JobType.Category,
            Operation = OperationType.Delete,
            CreatorId = Guid.NewGuid(),
            ProductId = 15
        };

        // Act
        var createdJob = await _jobService.CreateJobAsync(newJob);
        var jobInDb = await _dbContext.Jobs.FindAsync(createdJob.Id);

        // Assert
        Assert.NotNull(createdJob);
        Assert.NotNull(jobInDb);
        Assert.Equal(newJob.Type, jobInDb.Type);
        Assert.Equal(newJob.Operation, jobInDb.Operation);
        Assert.Equal(newJob.ProductId, jobInDb.ProductId);
    }

    [Fact]
    public async Task RemoveJob_ShouldDeleteJob_WhenJobExists()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        var job = new Job
        {
            Id = 3,
            Type = JobType.Category,
            Operation = OperationType.Delete,
            ProductId = 25
        };
        _dbContext.Jobs.Add(job);
        _dbContext.SaveChanges();

        // Act
        var result = await _jobService.RemoveJob(3);
        var jobInDb = await _dbContext.Jobs.FindAsync(3);

        // Assert
        Assert.True(result);
        Assert.Null(jobInDb);
    }

    [Fact]
    public async Task RemoveJob_ShouldNotThrow_WhenJobDoesNotExist()
    {
        // Act
        var result = await _jobService.RemoveJob(999);

        // Assert
        Assert.False(result); // Your method doesn't check for null before calling Remove()
    }
}
