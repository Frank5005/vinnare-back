using Shared.DTOs;

namespace Services.Interfaces

{
    public interface IJobService
    {
        Task<IEnumerable<ViewJobResponse>> GetAllJobsAsync();
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<JobDto> CreateJobAsync(JobDto jobDto);
        Task<bool> RemoveJob(int jobId);

    }
}