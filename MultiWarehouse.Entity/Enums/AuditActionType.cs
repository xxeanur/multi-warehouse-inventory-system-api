using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Entity.Enums
{
    /// <summary>
    /// AuditLog tablosunda tutulan işlemin türünü belirler.
    /// </summary>
    public enum AuditActionType
    {
        Create, // Yeni bir kayıt eklendiğinde
        Update, // Var olan bir kayıt güncellendiğinde
        Delete, // Bir kayıt silindiğinde
        Login,  // Kullanıcı sisteme giriş yaptığında (İsteğe bağlı)
        Logout  // Kullanıcı sistemden çıkış yaptığında (İsteğe bağlı)
    }
}
