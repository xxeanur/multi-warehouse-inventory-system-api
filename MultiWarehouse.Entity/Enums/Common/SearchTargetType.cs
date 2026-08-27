namespace MultiWarehouse.Entity.Enums.Common
{
    /// <summary>
    /// Arama sonucuna tıklandığında Frontend'in hangi modüle/sayfaya yönleneceğini belirtir.
    /// </summary>
    public enum SearchTargetType
    {
        None = 0,
        Product = 1,
        InboundOrder = 2,
        OutboundOrder = 3,
        TransferOrder = 4,
        Warehouse = 5
    }
}