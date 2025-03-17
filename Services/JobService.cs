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

        public async Task<IEnumerable<JobDto>> GetAllJobsAsync()
        {
            _logger.LogInformation("TESING");
            return await _context.Jobs
                .Select(p => new JobDto
                {
                    Id = p.Id,
                    Type = p.Type,
                    Operation = p.Operation,
                    Action = p.Action,
                    CreatorId = p.CreatorId,
                    Date = p.Date
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
                Action = job.Action,
                CreatorId = job.CreatorId,
                Date = job.Date
            };
        }

        public async Task<JobDto> CreateJobAsync(JobDto jobDto)
        {
            var job = new Job
            {
                Type = jobDto.Type,
                Operation = jobDto.Operation,
                Action = jobDto.Action,
                CreatorId = jobDto.CreatorId,
                Date = jobDto.Date
            };

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                Action = job.Action,
                CreatorId = job.CreatorId,
                Date = job.Date
            };
        }

        public async Task<JobDto?> UpdateJobAsync(int id, JobDto jobDto)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return null;
            }

            job.Type = jobDto.Type;
            job.Operation = jobDto.Operation;
            job.Action = jobDto.Action;
            job.CreatorId = jobDto.CreatorId;
            job.Date = jobDto.Date;

            await _context.SaveChangesAsync();

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                Action = job.Action,
                CreatorId = job.CreatorId,
                Date = job.Date
            };
        }

        public async Task<JobDto?> DeleteJobAsync(int id)
        {
            var job = await _context.Jobs.FindAsync(id);

            if (job == null)
            {
                return null;
            }

            _context.Jobs.Remove(job);
            await _context.SaveChangesAsync();

            return new JobDto
            {
                Id = job.Id,
                Type = job.Type,
                Operation = job.Operation,
                Action = job.Action,
                CreatorId = job.CreatorId,
                Date = job.Date
            };
        }
    }
}
