using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.CategoryDtos;

namespace MultiWarehouse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Sisteme giriş yapmış herkes (SuperAdmin, Manager, Staff) buraya erişebilir, ancak aşağıdaki metotlarda roller ayrışacak.
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        #region Write Operations (Only SuperAdmin)

        /// <summary>
        /// Yeni kategori oluşturur. Master Data olduğu için sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")] // YETKİ KORUMASI EKLENDİ
        public async Task<IActionResult> Create(CategoryCreateDto createDto)
        {
            var category = await _categoryService.CreateAsync(createDto);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Kategoriyi günceller. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpPut]
        [Authorize(Roles = "SuperAdmin")] // YETKİ KORUMASI EKLENDİ
        public async Task<IActionResult> Update(CategoryUpdateDto updateDto)
        {
            var category = await _categoryService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Belirtilen kategoriyi pasif (soft delete) duruma çeker. Sadece SuperAdmin yetkilidir.
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin")] // YETKİ KORUMASI EKLENDİ
        public async Task<IActionResult> Remove(Guid id)
        {
            await _categoryService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }

        #endregion

        #region Read Operations (All Authenticated Users)

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kategoriyi getirir. Tüm personeller görüntüleyebilir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Tüm aktif kategorileri listeler. Tüm personeller görüntüleyebilir.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<CategoryDto>>.SuccessResponse(categories));
        }

        #endregion
    }
}