using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.UserDtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiWarehouse.API.Controllers
{
    // Sisteme sadece SuperAdmin yetkisine sahip olanlar personel ekleyip silebilir.
    [Authorize(Roles = "SuperAdmin")]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Yeni kullanıcı oluşturur.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto createDto)
        {
            var user = await _userService.CreateUserAsync(createDto);
            // Sarmalama işlemini tam istediğin gibi burada yapıyoruz.
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Belirtilen ID'ye sahip aktif kullanıcıyı getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(CustomResponseDto<UserDto>.SuccessResponse(user));
        }

        /// <summary>
        /// Sistemdeki tüm aktif kullanıcıları listeler.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(CustomResponseDto<IEnumerable<UserDto>>.SuccessResponse(users));
        }

        /// <summary>
        /// Belirtilen kullanıcıyı pasif (soft delete) duruma çeker.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            await _userService.RemoveUserAsync(id);
            // Veri dönmeyeceği için T almayan saf SuccessResponse dönüyoruz.
            return Ok(CustomResponseDto.SuccessResponse());
        }
    }
}