using Core.Utilities.Result.Abstract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IExcelService
    {
        Stream GeneratePartImportTemplate();
        IResult ImportPartsFromExcel(IFormFile excelFile);
    }
}
