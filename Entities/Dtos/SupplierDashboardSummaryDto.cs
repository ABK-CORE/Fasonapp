using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class SupplierDashboardSummaryDto:IDto
    {
        public int TotalContracts { get; set; }            // Toplam aktif sözleşme sayısı
        public int ActiveTendersCount { get; set; }        // Devam eden ihaleler
        public int PendingOffersCount { get; set; }        // Henüz teklif girilmemiş ihaleler
        public int AcceptedOffersCount { get; set; }       // Kabul edilmiş teklifler
        public List<DayCountDto> NewContractsLast7Days { get; set; }    // Son 7 günde yeni sözleşmeler
        public List<DayCountDto> NewOffersLast7Days { get; set; }       // Son 7 günde girilen teklifler
    }
}
