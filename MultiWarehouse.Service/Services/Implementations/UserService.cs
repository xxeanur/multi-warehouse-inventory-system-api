using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MultiWarehouse.Entity.Entities;
using MultiWarehouse.Service.Context;
using MultiWarehouse.Service.Exceptions;
using MultiWarehouse.Service.Repositories.Interfaces;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IGenericRepository<User> _userRepository;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public UserService(IGenericRepository<User> userRepository, AppDbContext context, IMapper mapper)
        {
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// Yeni kullanıcı oluşturur.
        /// </summary>
        public async Task<UserDto> CreateUserAsync(UserCreateDto createDto)
        {
            var isEmailExists = await _userRepository.AnyAsync(x => x.Email == createDto.Email);
            if (isEmailExists)
            {
                throw new ClientSideException("Bu e-posta adresi zaten sistemde kayıtlı.");
            }

            var user = _mapper.Map<User>(createDto);

            user.PasswordHash = createDto.Password;

            await _userRepository.AddAsync(user);
            await _context.SaveChangesAsync();

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kullanıcıyı getirir.
        /// </summary>
        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            var user = await _userRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();

            if (user == null)
            {
                throw new ClientSideException("Belirtilen ID'ye sahip aktif kullanıcı bulunamadı.");
            }

            return _mapper.Map<UserDto>(user);
        }

        /// <summary>
        /// Sistemdeki tüm aktif kullanıcıları listeler.
        /// </summary>
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.Where(x => x.IsActive).ToListAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        /// <summary>
        /// Belirtilen kullanıcıyı pasif (soft delete) duruma çeker.
        /// </summary>
        public async Task RemoveUserAsync(Guid id)
        {
            var user = await _userRepository.Where(x => x.Id == id && x.IsActive).SingleOrDefaultAsync();
            if (user == null)
            {
                throw new ClientSideException("Silinmek istenen aktif kullanıcı bulunamadı.");
            }

            user.IsActive = false;

            _userRepository.Update(user);
            await _context.SaveChangesAsync();
        }

        public Task<UserDto> GetUserByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task RemoveUserAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}