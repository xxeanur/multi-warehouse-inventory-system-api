using MultiWarehouse.Shared.DTOs.DashboardDtos;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Services.Interfaces
{
    public interface IDashboardService
    {
        /// <summary>
        /// Tüm özet bilgileri, grafikleri ve son hareketleri tek bir seferde hesaplayıp getirir.
        /// </summary>
        Task<DashboardDto> GetDashboardDataAsync();
    }
}