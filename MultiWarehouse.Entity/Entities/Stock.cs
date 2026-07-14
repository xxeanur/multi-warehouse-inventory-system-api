
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
    /// <summary>
    /// Hangi ürünün, hangi deponun, hangi rafında, tam olarak kaç adet bulunduğunu tutan ana stok tablosudur.
    /// Multi-Warehouse mimarisinin kalbidir.
    /// </summary>
    public class Stock : BaseEntity
    {
        // Hangi ürün?
        public Guid ProductId { get; set; }
        public Product Product { get; set; } = null!;

        // Hangi depoda?
        public Guid WarehouseId { get; set; }
        public Warehouse Warehouse { get; set; } = null!;

        // O deponun hangi rafında?
        public Guid ShelfId { get; set; }
        public Shelf Shelf { get; set; } = null!;

        // Belirtilen raftaki miktar
        public int Quantity { get; set; }

        // EKLENEN KISIM
        // Siparişi alınmış ama henüz kargolanmamış, başkasına satılamayacak stok miktarı
        public int ReservedQuantity { get; set; }

        public DateTime LastMovementDate { get; set; }
    }
}