using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities.Common;
using MultiWarehouse.Entity.Entities.Identity;
using MultiWarehouse.Entity.Enums.Common;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces.Common;
using MultiWarehouse.Shared.DTOs.AuditLogDtos;
using MultiWarehouse.Shared.Pagination;

namespace MultiWarehouse.Service.Services.Implementations.Common
{
    public class AuditLogService : IAuditLogService
    {
        #region Dependencies

        private readonly IGenericRepository<AuditLog> _auditLogRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AuditLogService(
            IGenericRepository<AuditLog> auditLogRepository,
            IGenericRepository<User> userRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _auditLogRepository = auditLogRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        #endregion

        #region Write Operations

        public async Task<AuditLogDto> CreateAsync(AuditLogCreateDto createDto)
        {
            var userExists = await _userRepository.Where(u => u.Id == createDto.UserId && u.IsActive).AnyAsync();
            if (!userExists) throw new ClientSideException("İşlemi yapan kullanıcı sistemde bulunamadı.");

            var auditLog = _mapper.Map<AuditLog>(createDto);
            auditLog.CreatedDate = DateTime.UtcNow;
            auditLog.IsActive = true;

            await _auditLogRepository.AddAsync(auditLog);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<AuditLogDto>(auditLog);
        }

        #endregion

        #region Read Operations

        public async Task<AuditLogDto> GetByIdAsync(Guid id)
        {
            var log = await _auditLogRepository.Where(a => a.Id == id).SingleOrDefaultAsync();
            if (log == null) throw new ClientSideException("İlgili denetim kaydı bulunamadı.");
            return _mapper.Map<AuditLogDto>(log);
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
        {
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
            var logs = await _auditLogRepository
                .Where(a => a.TableName.ToLower() == tableName.ToLower() && a.IsActive)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(logs);
        }



        public async Task<PagedResult<AuditLogDto>> GetPagedAsync(PaginationParams paginationParams)
        {
            var query = _auditLogRepository.Where(a => a.IsActive).OrderByDescending(a => a.CreatedDate);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<AuditLogDto>(_mapper.Map<IEnumerable<AuditLogDto>>(items), totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<PagedResult<AuditLogDto>> GetPagedByUserIdAsync(PaginationParams paginationParams, Guid userId)
        {
            var query = _auditLogRepository.Where(a => a.UserId == userId && a.IsActive).OrderByDescending(a => a.CreatedDate);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<AuditLogDto>(_mapper.Map<IEnumerable<AuditLogDto>>(items), totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<PagedResult<AuditLogDto>> GetPagedByTableNameAsync(PaginationParams paginationParams, string tableName)
        {
            var query = _auditLogRepository.Where(a => a.TableName.ToLower() == tableName.ToLower() && a.IsActive).OrderByDescending(a => a.CreatedDate);
            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
                .Take(paginationParams.PageSize)
                .ToListAsync();

            return new PagedResult<AuditLogDto>(_mapper.Map<IEnumerable<AuditLogDto>>(items), totalCount, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public async Task<IEnumerable<AuditLogDto>> GetRecentSecurityLogsByUserIdAsync(Guid userId, int count = 10)
        {
            var securityLogs = await _auditLogRepository
                .Where(a => a.UserId == userId && a.IsActive && a.ActionType >= AuditActionType.Login)
                .OrderByDescending(a => a.CreatedDate)
                .Take(count)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AuditLogDto>>(securityLogs);
        }

        #endregion
    }
}