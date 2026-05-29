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
    public class FasonAylikHareketController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Processor _processor;

        public FasonAylikHareketController(IConfiguration configuration, Processor processor)
        {
            _configuration = configuration;
            _processor = processor;
        }

        [HttpGet("GetFasonAylikHareketler")]
        public IActionResult GetFasonAylikHareketler(string fasonIsim)
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
                    parameters.Add("@Fasonisim", fasonIsim);

                    var data = con.Query("sp_fasonsurecozetAYAAITbyfason",
                                         parameters,
                                         commandType: CommandType.StoredProcedure)
                                   .ToList();

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }
    }
}
