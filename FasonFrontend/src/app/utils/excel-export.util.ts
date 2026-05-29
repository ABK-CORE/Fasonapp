import * as XLSX from 'xlsx';

export function exportToExcel(data: any[], filename: string = 'rapor.xlsx') {
  const worksheet = XLSX.utils.json_to_sheet(data);
  const workbook = XLSX.utils.book_new();
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Rapor');
  // Dosya uzantısı .xlsx değilse ekle
  if (!filename.toLowerCase().endsWith('.xlsx')) {
    filename += '.xlsx';
  }
  XLSX.writeFile(workbook, filename);
}
