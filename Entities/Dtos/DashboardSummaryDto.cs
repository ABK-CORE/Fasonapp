using Core.Entities;

namespace Entities.Dtos
{
    public class DashboardSummaryDto:IDto
    {
        public int TotalTenders { get; set; }
        public int CompletedTenders { get; set; }
        public PeriodCountDto January { get; set; }
        public List<DayCountDto> NewSuppliersLast7Days { get; set; }
        public List<DayCountDto> NewTendersLast7Days { get; set; }
        public List<MonthlyTenderCountDto> MonthlyTenderStats { get; set; }
        public List<DayCountDto> NewRequestedPartsLast7Days { get; set; }
        public List<MonthlyProductCountDto> MonthlyRequestedProductStats { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalRequestedParts { get; set; }
        public List<DayCountDto> WeeklyRequestedParts { get; set; }
    }

}
