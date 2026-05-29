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
    public class FasonUretimGunlukController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Processor _processor;

        public FasonUretimGunlukController(IConfiguration configuration, Processor processor)
        {
            _configuration = configuration;
            _processor = processor;
        }

        public class GunlukUretimRequest
        {
            public DateTime? Tarih { get; set; }
        }

        [HttpPost("GetFasonUretimEmriGunluk")]
        public IActionResult GetFasonUretimEmriGunluk([FromBody] GunlukUretimRequest request)
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
                    parameters.Add("@Tarih", request.Tarih?.Date, DbType.Date);

                    var data = con.Query("sp_GetFasonUretimemrigunluk",
                                         parameters,
                                         commandType: CommandType.StoredProcedure)
                                   .ToList();

                    return Ok(data);
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
