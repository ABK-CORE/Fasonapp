using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPAuditLog")]
    public class AuditLog:IEntity
    {
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid UserGuid { get; set; }
        public string Message { get; set; } = null!;
        public string? Category { get; set; }
        public int? RequestId { get; set; }
        public string Type { get; set; } = null!;
    }
}
