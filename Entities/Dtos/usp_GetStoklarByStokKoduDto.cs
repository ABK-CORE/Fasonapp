using Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    [Keyless]
    public class usp_GetStoklarByStokKoduDto: IDto
    {
        public string Depo { get; set; }
        public string StokKodu { get; set; }
        public string StokAdi { get; set; }
        public decimal StokMiktari { get; set; }
    }
}
