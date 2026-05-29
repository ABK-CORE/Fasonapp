using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using Microsoft.Data.SqlClient;
using Dapper;
using Enigma;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FasonUretimController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Processor _processor;

        public FasonUretimController(IConfiguration configuration, Processor processor)
        {
            _configuration = configuration;
            _processor = processor;
        }

        public class UretimEmriRequest
        {
            public string UrunKodu { get; set; }
            public string YariMamulkodu { get; set; }
            public int PlanlananMiktar { get; set; }
            public int KoliAdeti { get; set; }
            public string Fasonis { get; set; }
            public string Fasomsirket { get; set; }
            public string CreateUser { get; set; }
        }

        [HttpPost("CreateUretimEmriVeLot")]
        public IActionResult CreateUretimEmriVeLot([FromBody] UretimEmriRequest request)
        {
            try
            {
                var encryptedConnStr = _configuration.GetConnectionString("DatabaseConnection");
                var keyStr = _configuration["AesSettings:Key"];
                var vectorStr = _configuration["AesSettings:Vektor"];
                if (string.IsNullOrEmpty(encryptedConnStr) || string.IsNullOrEmpty(keyStr) || string.IsNullOrEmpty(vectorStr))
                    return StatusCode(500, new { Message = "Bağlantı ayarları eksik" });

                var aesKey = Convert.FromBase64String(keyStr);
                var aesVector = Convert.FromBase64String(vectorStr);

                string connStr;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesVector;
                    connStr = _processor.DecryptorSymmetric(encryptedConnStr, aes);
                }

                using (var con = new SqlConnection(connStr))
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@UrunKodu", request.UrunKodu);
                    parameters.Add("@YariMamülKodu", request.YariMamulkodu);
                    parameters.Add("@PlanlananMiktar", request.PlanlananMiktar);
                    parameters.Add("@KoliAdeti", request.KoliAdeti);
                    parameters.Add("@Fasonis", request.Fasonis);
                    parameters.Add("@Fasomsirket", request.Fasomsirket);
                    parameters.Add("@CreateUser", request.CreateUser);

                    var result = con.Query("sp_CreateUretimEmriVeLot", parameters, commandType: CommandType.StoredProcedure).ToList();
                    return Ok(result);
                }
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { Message = "SQL hatası", Detail = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }
    }
}
