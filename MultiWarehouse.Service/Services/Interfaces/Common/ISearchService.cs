using MultiWarehouse.Shared.DTOs.SearchDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Common
{
    public interface ISearchService
    {
        #region Read Operations

        /// <summary>
        /// Kullanıcının girdiği kelimeye göre tüm modüllerde (yetki sınırları içinde) hızlı arama yapar.
        /// </summary>
        Task<IEnumerable<SearchResultItemDto>> SearchAcrossModulesAsync(string query);

        #endregion
    }
}