using MultiWarehouse.Entity.Entities;
using System.Threading.Tasks;

namespace MultiWarehouse.Service.Repositories.Interfaces
{
    // IGenericRepository'den miras alarak standart Ekle/Sil/Getir yeteneklerini de kazanıyor
    public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
    {
        // Sadece bu tabloya özel olan, User bilgisiyle birlikte token getiren metot
        Task<RefreshToken?> GetByTokenWithUserAsync(string token);
    }
}