using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Definitions;
using MultiWarehouse.Entity.Entities.Inventory;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Definations;
using MultiWarehouse.Shared.DTOs.ProductDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations.Definitions
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly IGenericRepository<Category> _categoryRepository;
        private readonly IGenericRepository<Supplier> _supplierRepository;
        private readonly IGenericRepository<Stock> _stockRepository;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ProductService(
            IGenericRepository<Product> productRepository,
            IGenericRepository<Category> categoryRepository,
            IGenericRepository<Supplier> supplierRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IGenericRepository<Stock> stockRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _supplierRepository = supplierRepository;
            _stockRepository = stockRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #region Write Operations

        public async Task<ProductDto> CreateAsync(ProductCreateDto createDto)
        {
            var isCategoryExists = await _categoryRepository.AnyAsync(c => c.Id == createDto.CategoryId && c.IsActive);
            if (!isCategoryExists) throw new ClientSideException("Seçilen kategori sistemde bulunamadı.");

            if (createDto.SupplierId != Guid.Empty)
            {
                var isSupplierExists = await _supplierRepository.AnyAsync(s => s.Id == createDto.SupplierId && s.IsActive);
                if (!isSupplierExists) throw new ClientSideException("Seçilen tedarikçi sistemde bulunamadı.");
            }

            var isSkuExists = await _productRepository.AnyAsync(p => p.Sku.ToLower() == createDto.Sku.ToLower() && p.IsActive);
            if (isSkuExists) throw new ClientSideException("Bu SKU koduna sahip bir ürün zaten mevcut.");

            if (!string.IsNullOrWhiteSpace(createDto.Barcode))
            {
                var isBarcodeExists = await _productRepository.AnyAsync(p => p.Barcode == createDto.Barcode && p.IsActive);
                if (isBarcodeExists) throw new ClientSideException("Bu barkoda sahip bir ürün zaten mevcut.");
            }

            var product = _mapper.Map<Product>(createDto);

            product.Barcode = createDto.Barcode;

            await _productRepository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> UpdateAsync(ProductUpdateDto updateDto)
        {
            var product = await _productRepository.Where(p => p.Id == updateDto.Id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Güncellenmek istenen ürün bulunamadı.");

            if (product.CategoryId != updateDto.CategoryId)
            {
                var isCategoryExists = await _categoryRepository.AnyAsync(c => c.Id == updateDto.CategoryId && c.IsActive);
                if (!isCategoryExists) throw new ClientSideException("Seçilen yeni kategori sistemde bulunamadı.");
            }

            // GÜVENLİK YAMASI 4: Tedarikçi Opsiyoneldir!
            if (product.SupplierId != updateDto.SupplierId && updateDto.SupplierId != Guid.Empty)
            {
                var isSupplierExists = await _supplierRepository.AnyAsync(s => s.Id == updateDto.SupplierId && s.IsActive);
                if (!isSupplierExists) throw new ClientSideException("Seçilen yeni tedarikçi sistemde bulunamadı.");
            }

            var isSkuExists = await _productRepository.AnyAsync(p => p.Sku.ToLower() == updateDto.Sku.ToLower() && p.Id != updateDto.Id && p.IsActive);
            if (isSkuExists) throw new ClientSideException("Girdiğiniz SKU kodu başka bir ürün tarafından kullanılıyor.");

            if (!string.IsNullOrWhiteSpace(updateDto.Barcode))
            {
                var isBarcodeExists = await _productRepository.AnyAsync(p => p.Barcode == updateDto.Barcode && p.Id != updateDto.Id && p.IsActive);
                if (isBarcodeExists) throw new ClientSideException("Girdiğiniz barkod başka bir ürün tarafından kullanılıyor.");
            }

            // Mapleme yerine performanslı manuel atama
            product.Sku = updateDto.Sku;
            product.Name = updateDto.Name;
            product.Brand = updateDto.Brand;
            product.ImageUrl = updateDto.ImageUrl;
            product.Width = updateDto.Width;
            product.Height = updateDto.Height;
            product.Depth = updateDto.Depth;
            product.Weight = updateDto.Weight;
            product.Barcode = updateDto.Barcode;
            product.Unit = updateDto.Unit;
            product.UnitPrice = updateDto.UnitPrice;
            product.CostPrice = updateDto.CostPrice;
            product.CriticalLevel = updateDto.CriticalLevel;
            product.CategoryId = updateDto.CategoryId;

            // Eğer Guid.Empty geldiyse DB'ye null/boş Guid olarak kaydet
            product.SupplierId = updateDto.SupplierId != Guid.Empty ? updateDto.SupplierId : product.SupplierId;

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(); // UoW üzerinden kayıt

            return _mapper.Map<ProductDto>(product);
        }

        public async Task RemoveAsync(Guid id)
        {
            var product = await _productRepository.Where(p => p.Id == id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Silinmek istenen ürün bulunamadı.");

            var hasActiveStock = await _stockRepository
                .Where(s => s.ProductId == id && s.IsActive && s.Quantity > 0)
                .AnyAsync();

            if (hasActiveStock)
            {
                throw new ClientSideException("Bu ürün silinemez! Depoda fiziksel stoğu bulunan ürünler pasife alınamaz. Lütfen önce stok çıkışı (Outbound) veya sayım ile stokları sıfırlayın.");
            }

            product.IsActive = false;

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        #endregion

        #region Read Operations

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.Where(p => p.Id == id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Ürün bulunamadı.");

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetByBarcodeAsync(string barcode)
        {
            var product = await _productRepository.Where(p => p.Barcode == barcode && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Bu barkoda sahip ürün bulunamadı.");

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetBySkuAsync(string sku)
        {
            var product = await _productRepository.Where(p => p.Sku.ToLower() == sku.ToLower() && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Bu SKU koduna sahip ürün bulunamadı.");

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            var products = await _productRepository.Where(p => p.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<PagedResult<ProductDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var pagedEntities = await _productRepository.GetPagedAsync(
                paginationParams,
                filter: p => p.IsActive
            );

            return _mapper.Map<PagedResult<ProductDto>>(pagedEntities);
        }

        public async Task<IEnumerable<ProductDto>> GetAllByCategoryIdAsync(Guid categoryId)
        {
            var products = await _productRepository.Where(p => p.CategoryId == categoryId && p.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> GetAllBySupplierIdAsync(Guid supplierId)
        {
            var products = await _productRepository.Where(p => p.SupplierId == supplierId && p.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        public async Task<IEnumerable<ProductDto>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<ProductDto>();

            var lowerQuery = query.ToLower();

            var products = await _productRepository
                .Where(p => p.IsActive &&
                           (p.Name.ToLower().Contains(lowerQuery) || p.Sku.ToLower().Contains(lowerQuery)))
                .Take(20)
                .ToListAsync();

            return _mapper.Map<IEnumerable<ProductDto>>(products);
        }

        #endregion
    }
}