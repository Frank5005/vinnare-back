using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Shared.DTOs;
using Shared.Enums;
using Xunit;

public class JobService_test
{
    private readonly VinnareDbContext _dbContext;
    private readonly Mock<ILogger<JobService>> _mockLogger;
    private readonly JobService _jobService;

    public JobService_test()
    {
        var options = new DbContextOptionsBuilder<VinnareDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _dbContext = new VinnareDbContext(options);
        _dbContext.Database.EnsureCreated();

        _mockLogger = new Mock<ILogger<JobService>>();
        _jobService = new JobService(_dbContext, _mockLogger.Object);
    }

    [Fact]
    public async Task GetAllJobsAsync_ShouldReturnAllJobs()
    {
        // Arrange
        _dbContext.Database.EnsureDeleted();
        _dbContext.Database.EnsureCreated();
        _dbContext.Jobs.Add(new Job
        {
            Id = 1,
            Type = JobType.Product,
            Operation = OperationType.Create,
            ProductId = 10
        });
        _dbContext.SaveChanges();

        // Act
        var jobs = await _jobService.GetAllJobsAsync();

        // Assert
        Assert.NotNull(jobs);
        Assert.Single(jobs);
        Assert.Equal(1, jobs.First().id);
        Assert.Equal("Product", jobs.First().JobType);
        Assert.Equal("Create", jobs.First().Operation);
        Assert.Equal(10, jobs.First().AssociatedId);
    }

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
