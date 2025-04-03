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
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<JobController> _logger;

        public JobController(IJobService jobService, IUserService userService, IProductService productService, ICategoryService categoryService, ILogger<JobController> logger)
        {
            _jobService = jobService;
            _userService = userService;
            _productService = productService;
            _categoryService = categoryService;
            _logger = logger;
        }

        // GET: api/jobs
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllJobs()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var products = await _productService.GetAllProductsAsync();
            
            var jobs = await _jobService.GetAllJobsAsync(categories, products);

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
            jobRequest.Validate();
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
            bool isCreate;
            if (jobRequest.GetOperationType() == OperationType.Create)
            {
                isCreate = true;
            }
            else if (jobRequest.GetOperationType() == OperationType.Delete)
            {
                isCreate = false;
            }
            else
            {
                throw new BadRequestException("How did we get here");
            }

            if (!string.IsNullOrEmpty(tokenRole) && tokenRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {

                if (jobRequest.JobType.Equals(JobType.Product.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    if (createdJob.ProductId != null)
                    {
                        if (isCreate)
                        {
                            await _productService.ApproveProduct((int)createdJob.ProductId, isCreate);
                            await _jobService.RemoveJob(createdJob.Id);
                        }
                        else
                        {
                            await _productService.DeleteProductAsync((int)createdJob.ProductId);
                        }
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
                        if (isCreate)
                        {
                            await _categoryService.ApproveCategory((int)createdJob.CategoryId, isCreate);
                            await _jobService.RemoveJob(createdJob.Id);
                        }
                        else
                        {
                            await _categoryService.DeleteCategoryAsync((int)createdJob.CategoryId);
                        }
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

        [Authorize(Roles = "Admin")]
        [HttpPost("/api/reviewJob")]
        public async Task<IActionResult> ReviewJob([FromQuery] string type, [FromBody] ReviewJobRequest request)
        {
            request.Validate();

            if (string.IsNullOrEmpty(type))
            {
                throw new BadRequestException("Type is required.");
            }

            var actionType = request.GetActionType();
            var accepted = actionType == ActionType.Approve;

            var job = await _jobService.GetJobByIdAsync(request.Id)
                       ?? throw new NotFoundException("Job not found");

            if (!type.Equals(job.Type.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new BadRequestException("Type should match the job type.");
            }

            var isCreate = job.Operation == OperationType.Create;

            if (job.Type == JobType.Product && job.ProductId.HasValue)
            {
                await HandleProductJob(job, accepted, isCreate);
            }
            else if (job.Type == JobType.Category && job.CategoryId.HasValue)
            {
                await HandleCategoryJob(job, accepted, isCreate);
            }

            return Ok(new DefaultResponse { message = $"Success {request.Action}" });
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task HandleProductJob(JobDto job, bool accepted, bool isCreate)
        {
            if (isCreate)
            {
                if (accepted)
                {
                    await _productService.ApproveProduct((int)job.ProductId, true);
                    await _jobService.RemoveJob(job.Id);
                }
                else
                {
                    await _productService.DeleteProductAsync((int)job.ProductId);
                }
            }
            else
            {
                if (accepted)
                {
                    await _productService.DeleteProductAsync((int)job.ProductId);
                }
                else
                {
                    await _productService.ApproveProduct((int)job.ProductId, true);
                    await _jobService.RemoveJob(job.Id);
                }
            }
        }
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task HandleCategoryJob(JobDto job, bool accepted, bool isCreate)
        {
            if (isCreate)
            {
                if (accepted)
                {
                    await _categoryService.ApproveCategory((int)job.CategoryId, true);
                    await _jobService.RemoveJob(job.Id);
                }
                else
                {
                    await _categoryService.DeleteCategoryAsync((int)job.CategoryId);
                }
            }
            else
            {
                if (accepted)
                {
                    await _categoryService.DeleteCategoryAsync((int)job.CategoryId);
                }
                else
                {
                    await _categoryService.ApproveCategory((int)job.CategoryId, true);
                    await _jobService.RemoveJob(job.Id);
                }
            }
        }



    }
}
