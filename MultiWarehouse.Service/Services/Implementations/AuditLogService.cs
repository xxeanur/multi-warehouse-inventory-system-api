using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations
{
    /// <summary>
    /// Sistemde gerçekleşen tüm veri hareketlerinin (Log) yönetildiği güvenlik servisidir.
    /// Not: Veri bütünlüğü için bu serviste kasıtlı olarak Update ve Delete metotları bulunmamaktadır.
    /// </summary>
    public class AuditLogService : IAuditLogService
    {
        private readonly IGenericRepository<AuditLog> _auditLogRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AuditLogService(IGenericRepository<AuditLog> auditLogRepository, AppDbContext context, IMapper mapper)
        {
            _auditLogRepository = auditLogRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<AuditLogDto> CreateAsync(AuditLogCreateDto createDto)
        {
            // Kullanıcı kontrolü (Eğer sistem dışı bir tetikleme değilse)
            var userExists = await _context.Set<User>().AnyAsync(u => u.Id == createDto.UserId && u.IsActive);
            if (!userExists) throw new ClientSideException("İşlemi yapan kullanıcı sistemde bulunamadı.");

            var auditLog = _mapper.Map<AuditLog>(createDto);

            await _auditLogRepository.AddAsync(auditLog);
            await _context.SaveChangesAsync();

            return _mapper.Map<AuditLogDto>(auditLog);
        }

        public async Task<AuditLogDto> GetByIdAsync(Guid id)
        {
            var log = await _auditLogRepository.Where(a => a.Id == id).SingleOrDefaultAsync();
            if (log == null) throw new ClientSideException("İlgili denetim kaydı bulunamadı.");
            return _mapper.Map<AuditLogDto>(log);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
            // Loglar genelde yeniden eskiye doğru izlenir. (En son yapılan işlem en üstte)
            var logs = await _auditLogRepository
                .Where(a => a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllByUserIdAsync(Guid userId)
        {
            var logs = await _auditLogRepository
                .Where(a => a.UserId == userId && a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllByTableNameAsync(string tableName)
        {
            // Büyük küçük harf duyarlılığını ortadan kaldırarak tablo ismine göre filtreleme yapıyoruz.
            var logs = await _auditLogRepository
                .Where(a => a.TableName.ToLower() == tableName.ToLower() && a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }

        //pagination 
        /// <summary>
        /// Sistemdeki tüm denetim loglarını sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<AuditLogDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var pagedEntities = await _auditLogRepository.GetPagedAsync(
                paginationParams,
                filter: a => a.IsActive
                // Not: Generic Repository'ye ileride OrderBy eklediğimizde buraya ".OrderByDescending(a => a.CreatedDate)" gelecek ki en son loglar ilk sayfada görünsün.
            );

            return _mapper.Map<PagedResult<AuditLogDto>>(pagedEntities);
        }

        /// <summary>
        /// Belirli bir kullanıcının sistemdeki ayak izlerini (loglarını) sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<AuditLogDto>> GetPagedByUserIdAsync(PaginationParams paginationParams, Guid userId)
        {
            var pagedEntities = await _auditLogRepository.GetPagedAsync(
                paginationParams,
                filter: a => a.IsActive && a.UserId == userId
            );

            return _mapper.Map<PagedResult<AuditLogDto>>(pagedEntities);
        }

        /// <summary>
        /// Sadece belirtilen bir tabloya ait değişiklik geçmişini sayfalayarak getirir.
        /// </summary>
        public async Task<PagedResult<AuditLogDto>> GetPagedByTableNameAsync(PaginationParams paginationParams, string tableName)
        {
            var pagedEntities = await _auditLogRepository.GetPagedAsync(
                paginationParams,
                filter: a => a.IsActive && a.TableName.ToLower() == tableName.ToLower()
            );

            return _mapper.Map<PagedResult<AuditLogDto>>(pagedEntities);
        }
    }
}