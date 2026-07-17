// MultiWarehouse.Service/Services/Implementations/CategoryService.cs
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.CategoryDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CategoryService(IGenericRepository<Category> categoryRepository, AppDbContext context, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Yeni kategori oluşturur.
        /// </summary>
        public async Task<CategoryDto> CreateAsync(CategoryCreateDto createDto)
        {
            var isNameExists = await _categoryRepository.AnyAsync(x => x.Name.ToLower() == createDto.Name.ToLower() && x.IsActive);
            if (isNameExists)
            {
                throw new ClientSideException("Bu isimde bir kategori zaten mevcut.");
            }

            var category = _mapper.Map<Category>(createDto);

            await _categoryRepository.AddAsync(category);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kategoriyi getirir.
        /// </summary>
        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();

            if (category == null)
            {
                throw new ClientSideException("Kategori bulunamadı.");
            }

            return _mapper.Map<CategoryDto>(category);
        }

        /// <summary>
        /// Tüm aktif kategorileri listeler.
        /// </summary>
        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        /// <summary>
        /// Kategoriyi günceller.
        /// </summary>
        public async Task<CategoryDto> UpdateAsync(CategoryUpdateDto updateDto)
        {
            var category = await _categoryRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (category == null)
            {
                throw new ClientSideException("Güncellenmek istenen kategori bulunamadı.");
            }

            // Aynı isimde başka bir kategori var mı kontrolü
            var isNameExists = await _categoryRepository.AnyAsync(x => x.Name.ToLower() == updateDto.Name.ToLower() && x.Id != updateDto.Id && x.IsActive);
            if (isNameExists)
            {
                throw new ClientSideException("Bu isimde başka bir kategori zaten mevcut.");
            }

            category.Name = updateDto.Name;
            category.Description = updateDto.Description;

            _categoryRepository.Update(category);
            await _context.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        /// <summary>
        /// Belirtilen kategoriyi pasif (soft delete) duruma çeker.
        /// </summary>
        public async Task RemoveAsync(Guid id)
        {
            var category = await _categoryRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (category == null)
            {
                throw new ClientSideException("Silinmek istenen kategori bulunamadı.");
            }

            category.IsActive = false;

            _categoryRepository.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}