using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class ExcelManager : IExcelService
    {
        private readonly IPartDal _partDal;

        public ExcelManager(IPartDal partDal)
        {
            _partDal = partDal;
        }

        public Stream GeneratePartImportTemplate()
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var package = new ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Parçalar");
            // Başlık satırı
            ws.Cells[1, 1].Value = "Stok Kodu";
            ws.Cells[1, 2].Value = "Stok Kodu Adı";
            ws.Cells[1, 3].Value = "Açıklama";
            ws.Row(1).Style.Font.Bold = true;
            var stream = new MemoryStream(package.GetAsByteArray());
            stream.Position = 0;
            return stream;
        }

        public IResult ImportPartsFromExcel(IFormFile excelFile)
        {
            if (excelFile == null || excelFile.Length == 0)
                return new Result(false, "Geçerli bir dosya seçmelisiniz.");

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using var stream = new MemoryStream();
            excelFile.CopyTo(stream);
            using var package = new ExcelPackage(stream);
            var ws = package.Workbook.Worksheets.FirstOrDefault();
            if (ws == null)
                return new Result(false, "Excel dosyasında sayfa bulunamadı.");

            int rowCount = ws.Dimension.Rows;
            for (int row = 2; row <= rowCount; row++)
            {
                var code = ws.Cells[row, 1].Text?.Trim();
                var name = ws.Cells[row, 2].Text?.Trim();
                var desc = ws.Cells[row, 3].Text?.Trim();

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(name))
                    continue; // zorunlu alan değilse atla

                var part = new Part
                {
                    PartCode = code,
                    PartName = name,
                    Description = desc,
                    IsActive = true
                };
                _partDal.Add(part);
            }

            return new SuccessResult("Excel verileri başarıyla içe aktarıldı.");
        }
    }
}
