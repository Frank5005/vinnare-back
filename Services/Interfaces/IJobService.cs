using Shared.DTOs;

namespace Services.Interfaces

{
    public interface IJobService
    {
        Task<IEnumerable<JobDto>> GetAllJobsAsync();
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<JobDto> CreateJobAsync(JobDto jobDto);

        Task<JobDto?> UpdateJobAsync(int id, JobDto jobDto);

        Task<JobDto?> DeleteJobAsync(int id);
    }
}