export interface FasonOzetRow {
  Fasonisim: string;
  Sofor: string;
  Surec: string;
  YuklemeTarihi: string;
  Fasonurunisim: string | null;
  StokAdi: string;
  cuvalsayisi: number;
  ToplamYapilanMiktar: number;
  Tür: string;
}

export interface FasonOzetFilter {
  startDate?: string;  // yyyy-MM-dd
  endDate?: string;    // yyyy-MM-dd
  fasonIsim?: string;
}
