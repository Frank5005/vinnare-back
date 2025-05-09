using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services.Interfaces;
using Shared.DTOs;
using Shared.Exceptions;

namespace Api.Controllers
{
    [Route("api/category")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        // GET: api/category
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/category/store
        [HttpGet("store")]
        public async Task<IActionResult> GetAvailableCategories()
        {
            var categories = await _categoryService.GetAvailableCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/category/top
        [HttpGet("top")]
        public async Task<IActionResult> GetTopThreeCategories()
        {
            var categories = await _categoryService.GetTopThreeCategoriesAsync();
            return Ok(categories);
        }

        // GET: api/category/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null) return NotFound();
            return Ok(category);
        }

        // POST: api/category
        [Authorize(Roles = "Admin, Seller")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryRequest categoryDto)
        {
            if (categoryDto == null) throw new BadRequestException("Category data is required.");

            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (tokenRole == "Admin")
            {
                var createdCategory = await _categoryService.CreateCategoryAsync(categoryDto);
                createdCategory.Approved = true;
                return Ok(new CategoryResponse { Id = createdCategory.Id, Message = "Category created successfully" });
            }
            else
            {
                var createdCategory = await _categoryService.CreateCategoryByEmployeeAsync(categoryDto);
                //createdCategory.Approved = false;
                return Ok(new CategoryResponse { Id = createdCategory.Id, Message = "Waiting to approve" });
            }
            //return BadRequest("Product not created");
        }

        // UPDATE: api/category/{id}
        [Authorize(Roles = "Admin, Seller")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdated categoryDto)
        {
            if (categoryDto == null) throw new BadRequestException("Category data is required.");

            var updatedCategory = await _categoryService.UpdateCategoryAsync(id, categoryDto);
            if (updatedCategory == null) throw new NotFoundException("Category not found");
            return Ok(new CategoryResponse { Id = updatedCategory.Id, Message = "Category updated successfully" });
        }

        // DELETE: api/category/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id, [FromHeader] string username)
        {
            var deletedCategory = "";
            var tokenRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (tokenRole == "Admin")
            {
                deletedCategory = await _categoryService.DeleteCategoryAsync(id);
                return Ok(new CategoryDelete { Message = deletedCategory });
            }
            deletedCategory = await _categoryService.DeleteCategoryByEmployeeAsync(id, username);
            return Ok(new ProductDelete { message = deletedCategory });
        }
    }
}
