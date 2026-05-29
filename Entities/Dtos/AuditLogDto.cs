using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class AuditLogDto : IDto
    {
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid UserGuid { get; set; }
        public string Message { get; set; } = null!;
        public string? Category { get; set; }
        public int? RequestId { get; set; }
        public string Type { get; set; }
        public string UserName { get; set; }
    }
}
