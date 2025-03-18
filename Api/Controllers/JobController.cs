using System.Security.Claims;
using Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly IJobService _jobService;
        private readonly IUserService _userService;

        public JobController(IJobService jobService, IUserService userService)
        {
            _jobService = jobService;
            _userService = userService;
        }

        // GET: api/jobs
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var jobs = await _jobService.GetAllJobsAsync();

            return Ok(jobs);
        }

        // POST: api/jobs
        [Authorize(Roles = "Admin,Seller")]
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest jobRequest)
        {
            if (jobRequest == null)
            {
                throw new BadRequestException("Job details is empty");
            }
            var tokenUsername = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(tokenUsername))
            {
                throw new UnauthorizedException("Token does not contain a username.");
            }

            var userId = await _userService.GetIdByUsername(tokenUsername) ?? throw new BadRequestException("User does not exist");
            JobDto jobDto;
            if (jobRequest.JobType.Equals(JobType.Product.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                jobDto = new JobDto
                {
                    Type = jobRequest.GetJobType(),
                    Operation = jobRequest.GetOperationType(),
                    CreatorId = userId,
                    ProductId = jobRequest.AssociatedId,
                };
            }
            else if ((jobRequest.JobType.Equals(JobType.Category.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                jobDto = new JobDto
                {
                    Type = jobRequest.GetJobType(),
                    Operation = jobRequest.GetOperationType(),
                    CreatorId = userId,
                    CategoryId = jobRequest.AssociatedId,
                };
            }
            else
            {
                throw new BadRequestException("Job type is wrong");
            }
            var createdJob = await _jobService.CreateJobAsync(jobDto);
            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (!string.IsNullOrEmpty(tokenRole) && tokenRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {

                if (jobRequest.JobType.Equals(JobType.Product.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (createdJob.ProductId != null)
                    {
                        await _jobService.ReviewJobProduct((int)createdJob.ProductId, true);
                        await _jobService.RemoveJob(createdJob.Id);
                    }
                    else
                    {
                        throw new BadRequestException("How did we get here?");
                    }

                }
                else if ((jobRequest.JobType.Equals(JobType.Category.ToString(), StringComparison.OrdinalIgnoreCase)))
                {
                    if (createdJob.CategoryId != null)
                    {
                        await _jobService.ReviewJobCategory((int)createdJob.CategoryId, true);
                        await _jobService.RemoveJob(createdJob.Id);

                    }
                    else
                    {
                        throw new BadRequestException("How did we get here?");
                    }
                }
                else
                {
                    throw new BadRequestException("How did we get here?");
                }
            }

            var createJobResponse = new CreateJobResponse
            {
                JobType = createdJob.Type.ToString(),
                Operation = createdJob.Operation.ToString(),
            };
            return Created("", createJobResponse);
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

        [Authorize(Roles = "Admin")]
        [HttpPost("/Api/reviewJob")]
        public async Task<IActionResult> ReviewJob([FromQuery] string type, [FromBody] ReviewJobRequest request)
        {
            request.Validate();
            if (string.IsNullOrEmpty(type))
            {
                throw new BadRequestException("Type is required.");
            }


            bool accepted;
            if (request.Action.Equals(ActionType.Approve.ToString()))
            {
                accepted = true;
            }
            else if (request.Action.Equals(ActionType.Decline.ToString()))
            {
                accepted = false;
            }
            else
            {
                throw new BadRequestException("Incorrect action");
            }

            var job = await _jobService.GetJobByIdAsync(request.Id);
            if (job == null)
            {
                throw new NotFoundException("Job not found");
            };
            if (type.Equals(JobType.Product.ToString(), StringComparison.OrdinalIgnoreCase) && job.ProductId != null)
            {

                await _jobService.ReviewJobProduct((int)job.ProductId, accepted);
                await _jobService.RemoveJob(job.Id);

            }
            else if (type.Equals(JobType.Category.ToString(), StringComparison.OrdinalIgnoreCase) && job.CategoryId != null)
            {
                await _jobService.ReviewJobCategory((int)job.CategoryId, accepted);
                await _jobService.RemoveJob(job.Id);

            }


            return Ok(new DefaultResponse());
        }
    }
}
