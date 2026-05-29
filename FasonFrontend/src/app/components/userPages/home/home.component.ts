import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ChartConfiguration, ChartData } from 'chart.js';
import { FasonDashboardYilMiktarRow } from '../../../models/fason-dashboard-yil-miktar.models';
import { FasonDashboardOzetRow } from '../../../models/fason-dashboard-ozet.models';
import { FasonDashboardYilMiktarService } from '../../../services/fason-dashboard-yil-miktar.service';
import { FasonDashboardOzetService } from '../../../services/fason-dashboard-ozet.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-homepage',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent implements OnInit {
  scopeLabel = 'Tum Fasonlar (Toplu)';

  loading = false;
  errorMessage = '';
  lastUpdated: Date | null = null;

  kpis = {
    monthlyProduced: 0,
    monthlyHakedis: 0,
    weeklyProduced: 0,
    weeklyHakedis: 0
  };

  topProducts: Array<{ name: string; quantity: number; ratio: number }> = [];
  recentMovements: FasonDashboardOzetRow[] = [];

  monthlyTrendData: ChartData<'line'> = {
    labels: [],
    datasets: [
      {
        label: 'Uretim',
        data: [],
        borderColor: '#0b7a75',
        backgroundColor: 'rgba(11, 122, 117, 0.18)',
        tension: 0.35,
        fill: true
      },
      {
        label: 'Hakedis',
        data: [],
        borderColor: '#da6a00',
        backgroundColor: 'rgba(218, 106, 0, 0.18)',
        tension: 0.35,
        fill: true
      }
    ]
  };

  weeklyTrendData: ChartData<'bar'> = {
    labels: [],
    datasets: [
      {
        label: 'Uretim',
        data: [],
        backgroundColor: '#0b7a75'
      },
      {
        label: 'Hakedis',
        data: [],
        backgroundColor: '#da6a00'
      }
    ]
  };

  lineChartOptions: ChartConfiguration<'line'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom'
      }
    }
  };

  barChartOptions: ChartConfiguration<'bar'>['options'] = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        position: 'bottom'
      }
    }
  };

  private readonly monthNames = ['Ocak', 'Subat', 'Mart', 'Nisan', 'Mayis', 'Haziran', 'Temmuz', 'Agustos', 'Eylul', 'Ekim', 'Kasim', 'Aralik'];

  constructor(
    private yilService: FasonDashboardYilMiktarService,
    private ozetService: FasonDashboardOzetService,
    private auth: AuthService
  ) {}

  ngOnInit() {
    const companyName = (this.auth.currentUser?.CompanyName || '').trim();
    const isBalmy = companyName.localeCompare('BalmyUretim', undefined, { sensitivity: 'accent' }) === 0;

    if (companyName && !isBalmy) {
      this.scopeLabel = `${companyName} (Sadece Kendi Veriniz)`;
      this.loadDashboard(companyName);
      return;
    }

    this.scopeLabel = 'Tum Fasonlar (Toplu)';
    this.loadDashboard();
  }

  private loadDashboard(fasonIsim?: string) {
    this.loading = true;
    this.errorMessage = '';

    forkJoin({
      yilRows: this.yilService.getDashboardYilMiktar(fasonIsim),
      ozetRows: this.ozetService.getDashboardOzet(fasonIsim)
    }).subscribe({
      next: ({ yilRows, ozetRows }) => {
        const normalizedOzet = this.normalizeOzetRows(ozetRows);
        this.buildKpis(normalizedOzet);
        this.buildMonthlyTrend(normalizedOzet);
        this.buildWeeklyTrend(normalizedOzet);
        this.buildTopProducts(yilRows);
        this.recentMovements = [...normalizedOzet]
          .sort((a, b) => +new Date(b.Tarih) - +new Date(a.Tarih))
          .slice(0, 8);
        this.lastUpdated = new Date();
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        this.errorMessage = err?.message || 'Dashboard verileri alinamadi.';
      }
    });
  }

  private normalizeOzetRows(rows: FasonDashboardOzetRow[]): FasonDashboardOzetRow[] {
    return (rows || []).filter(r => !Number.isNaN(+new Date(r.Tarih)));
  }

  private buildKpis(rows: FasonDashboardOzetRow[]) {
    const now = new Date();
    const monthStart = new Date(now.getFullYear(), now.getMonth(), 1);
    const weekStart = new Date(now);
    weekStart.setHours(0, 0, 0, 0);
    weekStart.setDate(weekStart.getDate() - 6);

    this.kpis.monthlyProduced = this.sumByRange(rows, monthStart, now, 'ToplamUretilenMiktar');
    this.kpis.monthlyHakedis = this.sumByRange(rows, monthStart, now, 'ToplamHakedis');
    this.kpis.weeklyProduced = this.sumByRange(rows, weekStart, now, 'ToplamUretilenMiktar');
    this.kpis.weeklyHakedis = this.sumByRange(rows, weekStart, now, 'ToplamHakedis');
  }

  private buildMonthlyTrend(rows: FasonDashboardOzetRow[]) {
    const producedData: number[] = [];
    const hakedisData: number[] = [];
    const labels: string[] = [];
    const now = new Date();

    for (let i = 5; i >= 0; i--) {
      const d = new Date(now.getFullYear(), now.getMonth() - i, 1);
      const month = d.getMonth();
      const year = d.getFullYear();

      labels.push(`${this.monthNames[month]} ${String(year).slice(-2)}`);
      producedData.push(this.sumByMonth(rows, month, year, 'ToplamUretilenMiktar'));
      hakedisData.push(this.sumByMonth(rows, month, year, 'ToplamHakedis'));
    }

    this.monthlyTrendData = {
      labels,
      datasets: [
        { ...this.monthlyTrendData.datasets[0], data: producedData },
        { ...this.monthlyTrendData.datasets[1], data: hakedisData }
      ]
    };
  }

  private buildWeeklyTrend(rows: FasonDashboardOzetRow[]) {
    const producedData: number[] = [];
    const hakedisData: number[] = [];
    const labels: string[] = [];

    const now = new Date();
    const currentWeekStart = this.startOfWeek(now);

    for (let i = 3; i >= 0; i--) {
      const weekStart = this.addDays(currentWeekStart, -7 * i);
      const weekEnd = this.addDays(weekStart, 6);
      labels.push(`${weekStart.getDate()}-${weekEnd.getDate()} ${this.monthNames[weekEnd.getMonth()]}`);
      producedData.push(this.sumByRange(rows, weekStart, weekEnd, 'ToplamUretilenMiktar'));
      hakedisData.push(this.sumByRange(rows, weekStart, weekEnd, 'ToplamHakedis'));
    }

    this.weeklyTrendData = {
      labels,
      datasets: [
        { ...this.weeklyTrendData.datasets[0], data: producedData },
        { ...this.weeklyTrendData.datasets[1], data: hakedisData }
      ]
    };
  }

  private buildTopProducts(rows: FasonDashboardYilMiktarRow[]) {
    const total = (rows || []).reduce((acc, r) => acc + (r.YilToplam_Miktar || 0), 0);
    this.topProducts = [...(rows || [])]
      .sort((a, b) => (b.YilToplam_Miktar || 0) - (a.YilToplam_Miktar || 0))
      .slice(0, 6)
      .map(r => ({
        name: r.URUNGRUPADI,
        quantity: r.YilToplam_Miktar || 0,
        ratio: total > 0 ? ((r.YilToplam_Miktar || 0) / total) * 100 : 0
      }));
  }

  private sumByMonth(rows: FasonDashboardOzetRow[], month: number, year: number, field: 'ToplamUretilenMiktar' | 'ToplamHakedis'): number {
    return rows
      .filter(r => {
        const d = new Date(r.Tarih);
        return d.getMonth() === month && d.getFullYear() === year;
      })
      .reduce((acc, r) => acc + (Number(r[field]) || 0), 0);
  }

  private sumByRange(rows: FasonDashboardOzetRow[], start: Date, end: Date, field: 'ToplamUretilenMiktar' | 'ToplamHakedis'): number {
    const s = +new Date(start.getFullYear(), start.getMonth(), start.getDate());
    const e = +new Date(end.getFullYear(), end.getMonth(), end.getDate(), 23, 59, 59, 999);
    return rows
      .filter(r => {
        const t = +new Date(r.Tarih);
        return t >= s && t <= e;
      })
      .reduce((acc, r) => acc + (Number(r[field]) || 0), 0);
  }

  private startOfWeek(date: Date): Date {
    const d = new Date(date);
    const day = d.getDay();
    const diff = day === 0 ? -6 : 1 - day;
    d.setDate(d.getDate() + diff);
    d.setHours(0, 0, 0, 0);
    return d;
  }

  private addDays(date: Date, days: number): Date {
    const d = new Date(date);
    d.setDate(d.getDate() + days);
    return d;
  }

}
