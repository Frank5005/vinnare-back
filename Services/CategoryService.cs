using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Services
{
    public class CategoryService : ICategoryService
    {
        private readonly VinnareDbContext _context;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(VinnareDbContext context, ILogger<CategoryService> logger)
        {
            _context = context;
            _logger = logger;
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

        public async Task<Category> CreateCategoryAsync(CategoryRequest categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name
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
    }
}
