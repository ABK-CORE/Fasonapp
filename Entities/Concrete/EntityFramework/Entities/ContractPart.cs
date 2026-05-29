using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ContractPart:IEntity
    {
        public int ContractId { get; set; }
        public int PartId { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }

        public Contract? Contract { get; set; }
        public Part? Part { get; set; }
    }
}
