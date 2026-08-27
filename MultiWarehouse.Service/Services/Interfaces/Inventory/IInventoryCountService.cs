using MultiWarehouse.Shared.DTOs.CountDtos;

namespace MultiWarehouse.Service.Services.Interfaces.Inventory
{
    public interface IInventoryCountService
    {
        #region Command Operations

        /// <summary>
        /// Operatörün girdiği fiziki sayım sonucunu işler. 
        /// Stok miktarlarını, raf/depo kapasitelerini (Formülize edilerek) günceller ve denetim logu oluşturur.
        /// </summary>
        Task<InventoryCountResultDto> PerformCountAsync(InventoryCountCreateDto countDto);

        #endregion
    }
}