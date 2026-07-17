// MultiWarehouse.API/Controllers/CategoriesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.CategoryDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    // Kategori yönetimine yetkisi olanlar erişebilir (Rol ismini kendi yapına göre ayarlayabilirsin)
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Yeni kategori oluşturur.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(CategoryCreateDto createDto)
        {
            var category = await _categoryService.CreateAsync(createDto);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kategoriyi getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryService.GetByIdAsync(id);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Tüm aktif kategorileri listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(CustomResponseDto<IEnumerable<CategoryDto>>.SuccessResponse(categories));
        }

        /// <summary>
        /// Kategoriyi günceller.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> Update(CategoryUpdateDto updateDto)
        {
            var category = await _categoryService.UpdateAsync(updateDto);
            return Ok(CustomResponseDto<CategoryDto>.SuccessResponse(category));
        }

        /// <summary>
        /// Belirtilen kategoriyi pasif (soft delete) duruma çeker.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _categoryService.RemoveAsync(id);
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}