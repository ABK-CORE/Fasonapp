using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderCreateDto:IDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public int IsOfferBased { get; set; }
        public int CategoryId { get; set; }
        public Guid? PurchaseRequestGuid { get; set; }
        public List<TenderItemCreateDto> Items { get; set; } = new();
        [JsonPropertyName("supplierOffers")]
        public List<TenderCreateOfferDto>? Offers { get; set; } = new();
    }
}
