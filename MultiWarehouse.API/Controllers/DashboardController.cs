using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouse.Service.Services.Interfaces.Dashboard;
using MultiWarehouse.Shared.DTOs;
using MultiWarehouse.Shared.DTOs.DashboardDtos;

namespace MultiWarehouse.API.Controllers
{
    /// <summary>
    /// Sistemin kokpit (vitrin) ekranı için gerekli özet verileri sağlayan API.
    /// Depo sınırları servis katmanında güvence altına alınmıştır.
    /// </summary>
    [Authorize(Roles = "SuperAdmin,WarehouseManager")]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Dashboard ekranı için gerekli tüm özet bilgileri (kartlar, grafikler, son hareketler) tek bir JSON olarak döner.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {
            var data = await _dashboardService.GetDashboardDataAsync();
            return Ok(CustomResponseDto<DashboardDto>.SuccessResponse(data));
        }
    }
}