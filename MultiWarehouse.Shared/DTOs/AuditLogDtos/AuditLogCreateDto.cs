using MultiWarehouse.Entity.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouse.Shared.DTOs.AuditLogDtos
{
    public class AuditLogCreateDto
    {
        public Guid UserId { get; set; }
        public AuditActionType ActionType { get; set; }
        public string TableName { get; set; } = string.Empty;
        public string OldValues { get; set; } = string.Empty;
        public string NewValues { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
    }
}