using Shared.DTOs;

namespace Services.Interfaces

{
    public interface IJobService
    {
        Task<IEnumerable<ViewJobResponse>> GetAllJobsAsync();
        Task<JobDto?> GetJobByIdAsync(int id);
        Task<JobDto> CreateJobAsync(JobDto jobDto);

        Task<JobDto?> UpdateJobAsync(int id, JobDto jobDto);

        Task<JobDto?> DeleteJobAsync(int id);
        Task<bool> ReviewJobCategory(int categoryId, bool approve);
        Task<bool> ReviewJobProduct(int productId, bool approve);
        Task<bool> RemoveJob(int jobId);

    }
}