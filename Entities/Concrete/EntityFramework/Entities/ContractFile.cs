using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ContractFile:IEntity
    {
        public int FileId { get; set; }
        public int ContractId { get; set; }
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime UploadedDate { get; set; }

        public Contract? Contract { get; set; }
    }
}
