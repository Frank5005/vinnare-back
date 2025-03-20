using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

namespace Services
{
    public class JobService : IJobService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<JobService> _logger;

        public JobService(VinnareDbContext context, ILogger<JobService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ViewJobResponse>> GetAllJobsAsync()
        {
            _logger.LogInformation("TESING");
            return await _context.Jobs
                .Select(p => new ViewJobResponse
                {
                    id = p.Id,
                    JobType = p.Type.ToString(),
                    Operation = p.Operation.ToString(),
                    AssociatedId = (int)(p.ProductId ?? p.CategoryId),
                })
                .ToListAsync();
        }

        public async Task<JobDto?> GetJobByIdAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return null;
            }

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                CreatorId = job.CreatorId,
                Date = job.Date,
                ProductId = job.ProductId,
                CategoryId = job.CategoryId,
            };
        }

        public async Task<JobDto> CreateJobAsync(JobDto jobDto)
        {
            var job = new Job
            {
                Type = jobDto.Type,
                Operation = jobDto.Operation,
                CreatorId = jobDto.CreatorId,
                CategoryId = jobDto.CategoryId,
                ProductId = jobDto.ProductId,
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                CreatorId = job.CreatorId,
                CategoryId = job.CategoryId,
                ProductId = job.ProductId,
                Date = job.Date

            };
        }


        public async Task<bool> RemoveJob(int jobId)
        {
            var job = await _context.Jobs.FindAsync(jobId);

            if (job == null)
            {
                return false;
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
