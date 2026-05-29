using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderOfferCreateDto
    {
        public string TenderGuid { get; set; } = null!;
        public List<OfferDto> Offers { get; set; } = new();
    }

    public class TenderCreateOfferDto
    {
        public Guid? supplierId { get; set; }
        public List<OfferDto> Offers { get; set; } = new();
    }
}
