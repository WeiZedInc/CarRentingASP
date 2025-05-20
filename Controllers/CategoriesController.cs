using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<List<CategoryDto>>> GetAllCategories()
        {
            var categories = await _categoryRepository.GetAllAsync();

            var categoryDtos = new List<CategoryDto>();
            foreach (var category in categories)
            {
                categoryDtos.Add(new CategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description
                });
            }

            return Ok(categoryDtos);
        }

        // GET: api/categories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CategoryDto>> GetCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return Ok(categoryDto);
        }

        // POST: api/categories
        [HttpPost]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CategoryCreateDto categoryDto)
        {
            try
            {
                // Check if category name already exists
                if (!await _categoryRepository.IsCategoryNameUniqueAsync(categoryDto.Name))
                {
                    return BadRequest("A category with this name already exists");
                }

                var category = new VehicleCategory
                {
                    Name = categoryDto.Name,
                    Description = categoryDto.Description
                };

                var createdCategory = await _categoryRepository.AddAsync(category);

                var createdCategoryDto = new CategoryDto
                {
                    Id = createdCategory.Id,
                    Name = createdCategory.Name,
                    Description = createdCategory.Description
                };

                return CreatedAtAction(nameof(GetCategory), new { id = createdCategoryDto.Id }, createdCategoryDto);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator,Manager")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(int id, CategoryCreateDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            // Check if the new name is unique (unless it's the same as the current name)
            if (category.Name != categoryDto.Name && !await _categoryRepository.IsCategoryNameUniqueAsync(categoryDto.Name))
            {
                return BadRequest("A category with this name already exists");
            }

            // Update category
            category.Name = categoryDto.Name;
            category.Description = categoryDto.Description;

            await _categoryRepository.UpdateAsync(category);

            var updatedCategoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };

            return Ok(updatedCategoryDto);
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);

            if (category == null)
                return NotFound();

            try
            {
                await _categoryRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Cannot delete this category: {ex.Message}");
            }
        }
    }
}