using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/part")]
    public class PartController : BaseApiController
    {
        private readonly IPartService _partService;
        private readonly IExcelService _excelService;

        public PartController(IPartService partService, IExcelService excelService)
        {
            _partService = partService;
            _excelService = excelService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _partService.GetParts();
            return Ok(result);
        }

        [HttpPost]
        [RequireDbRole("PartManagement")]
        [RequestSizeLimit(10_000_000)]
        public IActionResult Add([FromForm] string partCode, [FromForm] string partName, [FromForm] string? description, [FromForm] IFormFile? partPhoto)
        {
            var dto = new PartCreateDto
            {
                PartCode = partCode,
                PartName = partName,
                Description = description,
                PartPhoto = null
            };

            // Dosya varsa klasöre kaydet
            if (partPhoto != null && partPhoto.Length > 0)
            {
                var fileExt = Path.GetExtension(partPhoto.FileName);
                var guid = Guid.NewGuid().ToString();
                var fileName = $"{guid}{fileExt}";
                var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MainFile");
                var filePath = Path.Combine(folderPath, fileName);

                // klasör yoksa oluştur
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    partPhoto.CopyTo(stream);
                }

                // DTO'ya path ekle
                dto.PartPhoto = $"/{fileName}";
            }

            var result = _partService.AddPart(dto);
            return Ok(result);
        }

        [HttpDelete("{partId}")]
        [RequireDbRole("PartManagement")]
        public IActionResult Delete(int partId)
        {
            var result = _partService.DeletePart(partId);
            return Ok(result);
        }

        [HttpPut("{partId}")]
        [RequireDbRole("PartManagement")]
        [RequestSizeLimit(10_000_000)]
        public IActionResult Update(
            int partId,
            [FromForm] string partCode,
            [FromForm] string partName,
            [FromForm] string? description,
            [FromForm] IFormFile? partPhoto)
        {
            var dto = new PartUpdateDto
            {
                PartId = partId,
                PartCode = partCode,
                PartName = partName,
                Description = description,
                PartPhoto = null
            };

            if (partPhoto != null && partPhoto.Length > 0)
            {
                var ext = Path.GetExtension(partPhoto.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "MainFile");
                if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
                var path = Path.Combine(folder, fileName);
                using var stream = new FileStream(path, FileMode.Create);
                partPhoto.CopyTo(stream);
                dto.PartPhoto = $"/{fileName}";
            }

            var result = _partService.UpdatePart(dto);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

        // Şablon indir
        [HttpGet("template")]
        [RequireDbRole("PartManagement")]
        public IActionResult DownloadTemplate()
        {
            var stream = _excelService.GeneratePartImportTemplate();
            return File(
                fileStream: stream,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: "SatinAlmaSablonOrnegi.xlsx"
            );
        }

        // Excel import
        [HttpPost("import")]
        [RequireDbRole("PartManagement")]
        public IActionResult Import([FromForm] IFormFile file)
        {
            var result = _excelService.ImportPartsFromExcel(file);
            if (result.Success) return Ok(result);
            return BadRequest(result);
        }

    }
}