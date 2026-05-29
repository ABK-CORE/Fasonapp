using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Xml.Linq;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CurrencyController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // Dönen veri için basit bir DTO
        public class CurrencyRateDto
        {
            public string CurrencyCode { get; set; }
            public decimal Rate { get; set; }
        }

        [HttpGet("rate/{currencyCode}")]
        public async Task<IActionResult> GetRate(string currencyCode)
        {
            if (string.IsNullOrWhiteSpace(currencyCode))
            {
                return BadRequest("Para birimi kodu boş olamaz.");
            }

            currencyCode = currencyCode.ToUpper();

            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetStringAsync("https://www.tcmb.gov.tr/kurlar/today.xml");

                XDocument xmlDoc = XDocument.Parse(response);

                var rateNode = xmlDoc.Descendants("Currency")
                    .FirstOrDefault(c => c.Attribute("Kod")?.Value == currencyCode);

                if (rateNode == null)
                {
                    return NotFound(new { message = $"{currencyCode} için kur bulunamadı." });
                }

                string rateValueStr = rateNode.Element("ForexBuying")?.Value;
                if (string.IsNullOrEmpty(rateValueStr))
                {
                    rateValueStr = rateNode.Element("BanknoteBuying")?.Value;
                }


                if (decimal.TryParse(rateValueStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal rate))
                {
                    return Ok(new CurrencyRateDto { CurrencyCode = currencyCode, Rate = rate });
                }

                return NotFound(new { message = $"{currencyCode} için geçerli bir kur değeri bulunamadı." });
            }
            catch (Exception ex)
            {
                // Loglama mekanizmanızla hatayı loglayın
                return StatusCode(500, "Kurlar alınırken bir sunucu hatası oluştu.");
            }
        }
    }
}
