using MultiWarehouse.Shared.DTOs.PutawayDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Inventory
{
    /// <summary>
    /// Depoya girişi onaylanan ürünlerin raflara yerleştirilmesi (Putaway) operasyonlarını yöneten servis.
    /// </summary>
    public interface IPutawayService
    {
        /// <summary>
        /// Belirtilen depo için kapıda onaylanmış ve rafa dizilmeyi bekleyen işlemleri listeler.
        /// </summary>
        Task<IEnumerable<PutawayListDto>> GetPendingPutawaysAsync(Guid warehouseId);

        /// <summary>
        /// İlgili belgenin rafa dizilecek ürün detaylarını ve hedef raf bilgilerini getirir.
        /// </summary>
        Task<PutawayDetailDto> GetPutawayDetailAsync(Guid documentId, string documentType);

        /// <summary>
        /// Rafa dizme işlemini fiziksel olarak onaylar, stokları günceller ve belgenin durumunu tamamlandı (Completed) olarak işaretler.
        /// </summary>
        Task<bool> ExecutePutawayAsync(PutawayRequestDto requestDto);
    }
}