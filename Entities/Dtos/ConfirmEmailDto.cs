using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ConfirmEmailDto
    {
        public Guid UserGuid { get; set; }
        public string Code { get; set; } = "";
    }
}
