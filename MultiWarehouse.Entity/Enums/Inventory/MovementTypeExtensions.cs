namespace MultiWarehouse.Entity.Enums.Inventory
{
    public static class MovementTypeExtensions
    {
        // Hareketin Genel Yönünü Döndürür
        public static string GetDirectionName(this MovementType type)
        {
            return type switch
            {
                MovementType.Inbound or MovementType.CustomerReturn or MovementType.TransferIn => "GİRİŞ",
                MovementType.Outbound or MovementType.SupplierReturn or MovementType.Scrap or MovementType.TransferOut => "ÇIKIŞ",
                MovementType.ShelfTransfer => "TRANSFER",
                MovementType.AdjustmentIn or MovementType.Reversal => "DÜZELTME",
                _ => "BİLİNMEYEN"
            };
        }

        // Hareketin Kullanıcı Dostu İşlem Tipini Döndürür
        public static string GetTypeName(this MovementType type)
        {
            return type switch
            {
                MovementType.Inbound => "Mal Kabul",
                MovementType.CustomerReturn => "Müşteri İadesi",
                MovementType.Outbound => "Sevkiyat / Satış",
                MovementType.SupplierReturn => "Tedarikçi İadesi",
                MovementType.Scrap => "Fire / Hurda",
                MovementType.TransferIn => "Transfer Girişi",
                MovementType.TransferOut => "Transfer Çıkışı",
                MovementType.ShelfTransfer => "Raf Transferi",
                MovementType.AdjustmentIn => "Sayım Düzeltmesi",
                MovementType.Reversal => "Ters Kayıt / İptal",
                _ => type.ToString()
            };
        }

        // 3. Veritabanı Filtrelemesi İçin 
        public static List<MovementType> GetTypesByDirection(string direction)
        {
            return direction.ToUpper() switch
            {
                "GİRİŞ" => new List<MovementType> { MovementType.Inbound, MovementType.CustomerReturn, MovementType.TransferIn },
                "ÇIKIŞ" => new List<MovementType> { MovementType.Outbound, MovementType.SupplierReturn, MovementType.Scrap, MovementType.TransferOut },
                "TRANSFER" => new List<MovementType> { MovementType.ShelfTransfer },
                "DÜZELTME" => new List<MovementType> { MovementType.AdjustmentIn, MovementType.Reversal },
                _ => new List<MovementType>()
            };
        }
    }
}