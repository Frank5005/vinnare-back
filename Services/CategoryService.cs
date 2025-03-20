using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<CategoryService> _logger;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        private VinnareDbContext dbContext;
        private ILogger<CategoryService> @object;

        public CategoryService(VinnareDbContext context, ILogger<CategoryService> logger, JobService jobService, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _jobService = jobService;
            _userService = userService;
        }

        public CategoryService(VinnareDbContext dbContext, ILogger<CategoryService> @object)
        {
            this.dbContext = dbContext;
            this.@object = @object;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CategoryView>> GetAvailableCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Approved == true)
                .Select(c => new CategoryView
                {
                    Id = c.Id,
                    Name = c.Name
                })
                .ToListAsync();
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public async Task<Category> CreateCategoryByEmployeeAsync(CategoryRequest categoryDto, string userToken)
        {
            Guid userId = await _userService.GetUserIdFromToken(userToken);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
                //Console.WriteLine($"User ID: {userId}");
            }

            var category = new Category
            {
                Name = categoryDto.Name
            };

            _context.Categories.Add(category);
            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Category,
                Operation = OperationType.Create,
                CreatorId = userId,
                CategoryId = category.Id
            });
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> CreateCategoryAsync(CategoryRequest categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name,
                Approved = true
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> UpdateCategoryAsync(int id, CategoryUpdated categoryDto)
        {

            //Verify if the category exists
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }

            //Verify if we have a category with the same name
            var existingCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Name == categoryDto.Name);
            if (existingCategory != null)
            {
                throw new Exception("A category with this name already exists.");
            }

            category.Name = categoryDto.Name ?? category.Name;
            category.Approved = categoryDto.Approved ?? category.Approved;

            await _context.SaveChangesAsync();

            return category;
        }

        public async Task<string> DeleteCategoryAsync(int id)
        {
            string message = "Category deleted successfully";
            //Verify if the category exists
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                throw new Exception("You can't delete the category because it has products associated.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<string> DeleteCategoryByEmployeeAsync(int id, string userToken)
        {
            string message = "Category deleted successfully";
            //Verify if the category exists
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                throw new Exception("You can't delete the category because it has products associated.");
            }

            Guid userId = await _userService.GetUserIdFromToken(userToken);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
            }
            
            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Category,
                Operation = OperationType.Delete,
                CreatorId = userId,
                CategoryId = category.Id
            });

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task ApproveCategory(int categoryId, bool approve)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            category!.Approved = approve;
            await _context.SaveChangesAsync();
        }
    }
}
