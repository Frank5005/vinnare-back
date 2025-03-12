using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.Interfaces;
using Shared.DTOs;

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

        public async Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto)
        {
            var category = new Category
            {
                Name = categoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return categoryDto;
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryDto categoryDto)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            category.Name = categoryDto.Name;

            await _context.SaveChangesAsync();

            return categoryDto;
        }

        public async Task<CategoryDto?> DeleteCategoryAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null) return null;

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
