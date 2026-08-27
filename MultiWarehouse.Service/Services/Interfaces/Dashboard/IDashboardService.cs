using MultiWarehouse.Shared.DTOs.DashboardDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Dashboard
{
    public interface IDashboardService
    {
        /// <summary>
        /// Tüm özet bilgileri, grafikleri ve son hareketleri tek bir seferde hesaplayıp getirir (RLS Korumalı).
        /// </summary>
        Task<DashboardDto> GetDashboardDataAsync();
    }
}