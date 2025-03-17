using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;

namespace Api.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobController(IJobService jobService)
        {
            _jobService = jobService;
        }

        // GET: api/jobs
        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();
            return Ok(jobs);
        }

        // GET: api/jobs/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null) return NotFound();
            return Ok(job);
        }

        // POST: api/jobs
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobDto jobDto)
        {
            if (jobDto == null) return BadRequest("Job data is required.");

            var createdJob = await _jobService.CreateJobAsync(jobDto);
            return CreatedAtAction(nameof(GetJobById), new { id = createdJob.Id }, createdJob);
        }

        // UPDATE: api/jobs/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] JobDto jobDto)
        {
            if (jobDto == null) return BadRequest("Job data is required.");

            var updatedJob = await _jobService.UpdateJobAsync(id, jobDto);
            if (updatedJob == null) return NotFound();
            return Ok(updatedJob);
        }

        // DELETE: api/jobs/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var deletedJob = await _jobService.DeleteJobAsync(id);
            if (deletedJob == null) return NotFound();
            return Ok(deletedJob);
        }
    }
}
