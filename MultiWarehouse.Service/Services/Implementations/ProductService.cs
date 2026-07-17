using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _productRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductService(IGenericRepository<Product> productRepository, AppDbContext context, IMapper mapper)
        {
            _productRepository = productRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProductDto> CreateAsync(ProductCreateDto createDto)
        {
            // FK Kontrolleri
            var isCategoryExists = await _context.Set<Category>().AnyAsync(c => c.Id == createDto.CategoryId && c.IsActive);
            if (!isCategoryExists) throw new ClientSideException("Seçilen kategori sistemde bulunamadı.");

            var isSupplierExists = await _context.Set<Supplier>().AnyAsync(s => s.Id == createDto.SupplierId && s.IsActive);
            if (!isSupplierExists) throw new ClientSideException("Seçilen tedarikçi sistemde bulunamadı.");

            // Benzersizlik Kontrolleri
            var isSkuExists = await _productRepository.AnyAsync(p => p.Sku.ToLower() == createDto.Sku.ToLower() && p.IsActive);
            if (isSkuExists) throw new ClientSideException("Bu SKU koduna sahip bir ürün zaten mevcut.");

            var isBarcodeExists = await _productRepository.AnyAsync(p => p.Barcode == createDto.Barcode && p.IsActive);
            if (isBarcodeExists) throw new ClientSideException("Bu barkoda sahip bir ürün zaten mevcut.");

            var product = _mapper.Map<Product>(createDto);

            await _productRepository.AddAsync(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task<ProductDto> GetByIdAsync(Guid id)
        {
            var product = await _productRepository.Where(p => p.Id == id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Ürün bulunamadı.");

            return _mapper.Map<ProductDto>(product);
        }

        /// <summary>
        /// Barkod numarasına göre aktif ürünü getirir.
        /// El terminalleri ve barkod okuyucular için kullanılır.
        /// </summary>
        public async Task<ProductDto> GetByBarcodeAsync(string barcode)
        {
            var product = await _productRepository.Where(p => p.Barcode == barcode && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Bu barkoda sahip ürün bulunamadı.");

            return _mapper.Map<ProductDto>(product);
        }

        /// <summary>
        /// SKU (Stok Tutma Birimi) koduna göre aktif ürünü getirir.
        /// (Büyük/küçük harf duyarsız arama yapar)
        /// </summary>
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

        public async Task<ProductDto> UpdateAsync(ProductUpdateDto updateDto)
        {
            var product = await _productRepository.Where(p => p.Id == updateDto.Id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Güncellenmek istenen ürün bulunamadı.");

            // Kategori veya Tedarikçi değiştiyse varlıklarını kontrol et
            if (product.CategoryId != updateDto.CategoryId)
            {
                var isCategoryExists = await _context.Set<Category>().AnyAsync(c => c.Id == updateDto.CategoryId && c.IsActive);
                if (!isCategoryExists) throw new ClientSideException("Seçilen yeni kategori sistemde bulunamadı.");
            }

            if (product.SupplierId != updateDto.SupplierId)
            {
                var isSupplierExists = await _context.Set<Supplier>().AnyAsync(s => s.Id == updateDto.SupplierId && s.IsActive);
                if (!isSupplierExists) throw new ClientSideException("Seçilen yeni tedarikçi sistemde bulunamadı.");
            }

            // Benzersizlik kontrolleri (Kendisi hariç)
            var isSkuExists = await _productRepository.AnyAsync(p => p.Sku.ToLower() == updateDto.Sku.ToLower() && p.Id != updateDto.Id && p.IsActive);
            if (isSkuExists) throw new ClientSideException("Girdiğiniz SKU kodu başka bir ürün tarafından kullanılıyor.");

            var isBarcodeExists = await _productRepository.AnyAsync(p => p.Barcode == updateDto.Barcode && p.Id != updateDto.Id && p.IsActive);
            if (isBarcodeExists) throw new ClientSideException("Girdiğiniz barkod başka bir ürün tarafından kullanılıyor.");

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
            product.Unit = updateDto.Unit; // Artık UnitType Enum'u üzerinden çalışıyor
            product.UnitPrice = updateDto.UnitPrice;
            product.CostPrice = updateDto.CostPrice;
            product.CriticalLevel = updateDto.CriticalLevel;
            product.CategoryId = updateDto.CategoryId;
            product.SupplierId = updateDto.SupplierId;

            _productRepository.Update(product);
            await _context.SaveChangesAsync();

            return _mapper.Map<ProductDto>(product);
        }

        public async Task RemoveAsync(Guid id)
        {
            var product = await _productRepository.Where(p => p.Id == id && p.IsActive).SingleOrDefaultAsync();
            if (product == null) throw new ClientSideException("Silinmek istenen ürün bulunamadı.");

            product.IsActive = false; // Soft Delete
            _productRepository.Update(product);
            await _context.SaveChangesAsync();
        }
    }
}