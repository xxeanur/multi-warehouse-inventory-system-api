using System;
using System.Collections.Generic;

namespace MultiWarehouse.Shared.Pagination
{
    public class PagedResult<T>
    {
        public IEnumerable<T> Data { get; set; }//veri sadece ileri doğru gider bu dizi
        public int TotalCount { get; set; }//toplam veri
        public int TotalPages { get; set; }//toplam sayfa sayısı
        public int CurrentPage { get; set; }//şuanki sayfa cleintın istediği mevcut sayfa
        public int PageSize { get; set; }//bir sayfadaki kayıt sayısı

        public bool HasPrevious => CurrentPage > 1;//bir önceki
        public bool HasNext => CurrentPage < TotalPages;//bir sonraki sayfa

        public PagedResult(IEnumerable<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);//bir üst sayıya yuvarla
        }
    }
}