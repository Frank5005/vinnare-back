using Shared.DTOs;
using Data.Entities;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<IEnumerable<CategoryView>> GetAvailableCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<Category> CreateCategoryAsync(CategoryRequest categoryDto);

        Task<Category> CreateCategoryByEmployeeAsync(CategoryRequest categoryDto);

        Task<Category?> UpdateCategoryAsync(int id, CategoryUpdated categoryDto);

        Task<string> DeleteCategoryAsync(int id);
        Task<string> DeleteCategoryByEmployeeAsync(int id, string username);
        Task ApproveCategory(int categoryId, bool approve);
    }
}