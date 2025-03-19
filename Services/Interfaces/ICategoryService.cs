using Shared.DTOs;

namespace Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync();
        Task<CategoryDto?> GetCategoryByIdAsync(int id);
        Task<CategoryDto> CreateCategoryAsync(CategoryDto categoryDto);

        Task<CategoryDto?> UpdateCategoryAsync(int id, CategoryDto categoryDto);

        Task<CategoryDto?> DeleteCategoryAsync(int id);
        Task ApproveCategory(int categoryId, bool approve);

    }
}