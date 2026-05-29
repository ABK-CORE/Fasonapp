using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderCompletedDto: IDto
    {
        public Guid TenderGuid { get; set; }
        public string DeliveryNoteNumber { get; set; }
        public string InvoiceNumber { get; set; }
        public DateOnly DeliveryNoteDate { get; set; }
        public DateOnly InvoiceDate { get; set; }
        public List<TenderCompletedProductDto> Products { get; set; }
    }

    public class TenderCompletedProductDto: IDto
    {
        public int ProductId { get; set; }
        public decimal Quantity { get; set; }
    }
}
