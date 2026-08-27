using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs.CategoryDtos;

namespace MultiWarehouse.Service.Services.Implementations.Definitions
{
    public class CategoryService : ICategoryService
    {
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CategoryService(IGenericRepository<Category> categoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Create & Update

        public async Task<CategoryDto> CreateAsync(CategoryCreateDto createDto)
        {
            createDto.Name = createDto.Name?.Trim();
            createDto.Description = createDto.Description?.Trim();

            var isNameExists = await _categoryRepository.AnyAsync(x => x.Name.ToLower() == createDto.Name.ToLower() && x.IsActive);
            if (isNameExists)
                throw new ClientSideException("Bu isimde bir kategori zaten mevcut.");

            var category = _mapper.Map<Category>(createDto);

            await _categoryRepository.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<CategoryDto> UpdateAsync(CategoryUpdateDto updateDto)
        {
            updateDto.Name = updateDto.Name?.Trim();
            updateDto.Description = updateDto.Description?.Trim();
            var category = await _categoryRepository.Where(x => x.Id == updateDto.Id && x.IsActive).SingleOrDefaultAsync();
            if (category == null)
                throw new ClientSideException("Güncellenmek istenen kategori bulunamadı.");

            var isNameExists = await _categoryRepository.AnyAsync(x => x.Name.ToLower() == updateDto.Name.ToLower() && x.Id != updateDto.Id && x.IsActive);
            if (isNameExists)
                throw new ClientSideException("Bu isimde başka bir kategori zaten mevcut.");

            category.Name = updateDto.Name;
            category.Description = updateDto.Description;

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<CategoryDto>(category);
        }

        #endregion

        #region Read Operations

        public async Task<CategoryDto> GetByIdAsync(Guid id)
        {
            var category = await _categoryRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();

            if (category == null)
                throw new ClientSideException("Kategori bulunamadı.");

            return _mapper.Map<CategoryDto>(category);
        }

        public async Task<IEnumerable<CategoryDto>> GetAllAsync()
        {
            var categories = await _categoryRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<CategoryDto>>(categories);
        }

        #endregion

        #region Delete Operations

        public async Task RemoveAsync(Guid id)
        {
            var category = await _categoryRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();

            if (category == null)
                throw new ClientSideException("Silinmek istenen kategori bulunamadı.");

            category.IsActive = false;

            _categoryRepository.Update(category);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion
    }
}