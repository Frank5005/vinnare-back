using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Enums;
using Shared.Exceptions;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<CategoryService> _logger;
        private readonly IJobService _jobService;
        private readonly IUserService _userService;
        public CategoryService(VinnareDbContext context, ILogger<CategoryService> logger, IJobService jobService, IUserService userService)
        {
            _context = context;
            _logger = logger;
            _jobService = jobService;
            _userService = userService;
            _jobService = jobService;
            _userService = userService;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    ImageUrl = c.ImageUrl
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
                    Name = c.Name,
                    ImageUrl = c.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<Category> CheckAvailableCategory(string categoryName)
        {
            //var answer = false;
            var categoryExists = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryName);
            if (categoryExists == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }
            //It's available?
            if (categoryExists.Approved == true)
            {
                return categoryExists;
            }
            else
            {
                throw new UnauthorizedException("The category is not available.");
            }
            //return categoryExists;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                ImageUrl = category.ImageUrl
            };
        }

        public async Task<string> GetCategoryNameByIdAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            return category.Name;
        }

        public async Task<Category> CreateCategoryByEmployeeAsync(CategoryRequest categoryDto)
        {
            Guid userId = (Guid)await _userService.GetIdByUsername(categoryDto.Username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
            }

            //Verify if we have a category with the same name
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryDto.Name);
            if (existingCategory != null)
            {
                throw new Exception("A category with this name already exists.");
            }

            // Get user role
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new NotFoundException("User not found.");
            }

            var category = new Category
            {
                Name = categoryDto.Name,
                Approved = user.Role == RoleType.Admin,  // Set Approved based on user role
                ImageUrl = categoryDto.ImageUrl
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Only create job if user is not admin
            if (user.Role != RoleType.Admin)
            {
                await _jobService.CreateJobAsync(new JobDto
                {
                    Type = JobType.Category,
                    Operation = OperationType.Create,
                    CreatorId = userId,
                    CategoryId = category.Id
                });
            }

            return category;
        }
        public async Task<Category> CreateCategoryAsync(CategoryRequest categoryDto)
        {
            //Verify if we have a category with the same name
            var existingCategory = await _context.Categories
                .FirstOrDefaultAsync(c => c.Name == categoryDto.Name);
            if (existingCategory != null)
            {
                throw new Exception("A category with this name already exists.");
            }
            
            var category = new Category
            {
                Name = categoryDto.Name,
                Approved = true,
                ImageUrl= categoryDto.ImageUrl
                
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return category;
        }
        public async Task<Category> UpdateCategoryAsync(int id, CategoryUpdated categoryDto)
        {

            //Verify if the category exists

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
            if (category == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }

            category.Name = categoryDto.Name ?? category.Name;
            //category.Approved = categoryDto.Approved ?? category.Approved;

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

            //Verify if the category has products associated
            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                throw new Exception("You can't delete the category because it has products associated.");
            }

            //Verify if the category it's approved
            if (category.Approved == false)
            {
                throw new Exception("You can't delete the category because it's not approved'.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task<string> DeleteCategoryByEmployeeAsync(int id, string username)
        {
            string message = "Job created, waiting the admin approve.";
            //Verify if the category exists
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                throw new NotFoundException("The category doesn't exists.");
            }

            //Verify if the category it's approved
            if (category.Approved == false)
            {
                throw new Exception("You can't delete the category because it's not approved'.");
            }

            var hasProducts = await _context.Products.AnyAsync(p => p.CategoryId == id);
            if (hasProducts)
            {
                throw new Exception("You can't delete the category because it has products associated.");
            }

            Guid userId = (Guid)await _userService.GetIdByUsername(username);

            if (userId == Guid.Empty)
            {
                throw new NotFoundException("User not found.");
            }

            await _context.SaveChangesAsync();

            await _jobService.CreateJobAsync(new JobDto
            {
                Type = JobType.Category,
                Operation = OperationType.Delete,
                CreatorId = userId,
                CategoryId = category.Id
            });

            //_context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return message;
        }

        public async Task ApproveCategory(int categoryId, bool approve)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            category!.Approved = approve;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CategoryView>> GetTopThreeCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.Approved)
                .Select(c => new
                {
                    Category = c,
                    ProductCount = _context.Products.Count(p => p.CategoryId == c.Id && p.Approved && p.Available > 0)
                })
                .OrderByDescending(x => x.ProductCount)
                .Take(3)
                .Select(x => new CategoryView
                {
                    Id = x.Category.Id,
                    Name = x.Category.Name,
                    ImageUrl = x.Category.ImageUrl
                })
                .ToListAsync();
        }
    }
}
