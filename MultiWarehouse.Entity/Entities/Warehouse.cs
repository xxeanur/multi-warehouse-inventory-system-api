using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Entities
{
        /// <summary>
        /// Sistemin en tepe noktasıdır. Fiziksel depoları temsil eder (Örn: Konya Merkez Depo, İstanbul Şube).
        /// </summary>
        public class Warehouse : BaseEntity
        {
            public string Name { get; set; } = string.Empty; // Depo Adı
            public string Location { get; set; } = string.Empty; // Deponun bulunduğu şehir/ilçe veya adres

            // --- İLETİŞİM VE YÖNETİM (YENİ EKLENEN) ---


            // Operasyonel acil durumlarda veya sevkiyatlarda aranacak iletişim numarası
            public string Phone { get; set; } = string.Empty;

            // Depodan Sorumlu Yönetici (Artık adını, soyadını ve şahsi telefonunu User tablosundan çekeceğiz)
            public Guid? ManagerId { get; set; }
            public User? Manager { get; set; }

            public double MaxCapacity { get; set; }//max kapasite

            public double UsedCapacity { get; set; }//doluluk oranı
        // --- İLİŞKİLER (FOREIGN KEYS) ---

        // Bir deponun içinde birden fazla Blok (Zone) bulunur
        public List<WarehouseZone> WarehouseZones { get; set; } = new List<WarehouseZone>();
        }
    }

