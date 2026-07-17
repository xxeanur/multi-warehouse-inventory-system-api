namespace MultiWarehouse.Shared.Pagination
{
    public class PaginationParams
    {
        private const int MaxPageSize = 100; // Sistemin güvenliği için tek seferde çekilebilecek maksimum kayıt.
        private int _pageSize = 10;//bir sayfadaki kayıt sayısı.frontend değer göndermezse default olarak bunu kullanacak

        public int PageNumber { get; set; } = 1;//clientın görmek istediği sayfayı tutar. Eğer sayfa numarası belirtilmezse sistem ona default 1. sayfayı verir.

        public int PageSize//encapsulation
        {
            get => _pageSize;
            set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
        }
    }
}