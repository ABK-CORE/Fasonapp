using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
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
    public class FasonController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Processor _processor;

        private static string? NormalizeFasonIsim(string? fasonIsim)
        {
            if (string.IsNullOrWhiteSpace(fasonIsim))
                return null;

            var normalized = fasonIsim.Trim();
            return string.Equals(normalized, "null", StringComparison.OrdinalIgnoreCase)
                ? null
                : normalized;
        }

        private static List<dynamic> QueryDashboardWithFasonFallback(SqlConnection con, string storedProcedureName, string? fasonIsim)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Fasonisim", fasonIsim, DbType.String);

            var data = con.Query(storedProcedureName, parameters, commandType: CommandType.StoredProcedure).ToList();
            if (fasonIsim == null && data.Count == 0)
            {
                var legacyParams = new DynamicParameters();
                legacyParams.Add("@Fasonisim", "Null", DbType.String);
                data = con.Query(storedProcedureName, legacyParams, commandType: CommandType.StoredProcedure).ToList();
            }

            return data;
        }

        private static decimal ToDecimalSafe(object? value)
        {
            if (value == null || value == DBNull.Value)
                return 0m;

            try
            {
                return Convert.ToDecimal(value);
            }
            catch
            {
                return 0m;
            }
        }

        private static DateTime ToDateSafe(object? value)
        {
            if (value == null || value == DBNull.Value)
                return DateTime.MinValue;

            try
            {
                return Convert.ToDateTime(value);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private static string ToStringSafe(object? value)
        {
            if (value == null || value == DBNull.Value)
                return string.Empty;

            return value.ToString()?.Trim() ?? string.Empty;
        }

        private static List<string> GetAllFasonNames(SqlConnection con)
        {
            var raw = con.Query("sp_GetFasonFirmaadi", commandType: CommandType.StoredProcedure).ToList();
            return raw
                .Select(x => x as IDictionary<string, object>)
                .Where(x => x != null)
                .Select(x =>
                {
                    x!.TryGetValue("FasonFirmaadi", out var val);
                    return ToStringSafe(val);
                })
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static List<object> AggregateDashboardOzetForAllFasons(SqlConnection con)
        {
            var cmd = con.CreateCommand();
            cmd.CommandText = "sp_fasontakip_fasonapp_ozet_koli";
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.Add(new SqlParameter("@baslamatarihi", SqlDbType.DateTime) { Value = DateTime.Today.AddMonths(-12) });
            cmd.Parameters.Add(new SqlParameter("@bitistarihi", SqlDbType.DateTime) { Value = DateTime.Today.AddDays(1).AddTicks(-1) });
            cmd.Parameters.Add(new SqlParameter("@Fasonsurec", SqlDbType.NVarChar, 100) { Value = string.Empty });
            cmd.Parameters.Add(new SqlParameter("@Fasonisim", SqlDbType.NVarChar, 255) { Value = string.Empty });

            var da = new SqlDataAdapter(cmd);
            var dt = new DataTable();
            da.Fill(dt);

            var grouped = new Dictionary<string, (DateTime Tarih, string Urun, decimal Uretim, decimal Hakedis)>(StringComparer.OrdinalIgnoreCase);
            foreach (DataRow row in dt.Rows)
            {
                var tarih = ToDateSafe(row["Tarih"]);
                if (tarih == DateTime.MinValue)
                    continue;

                var urun = ToStringSafe(row.Table.Columns.Contains("URUNGRUPADI") ? row["URUNGRUPADI"] : null);
                var key = $"{tarih:yyyy-MM-dd}|{urun}";

                if (!grouped.TryGetValue(key, out var current))
                {
                    current = (tarih, urun, 0m, 0m);
                }

                current.Uretim += ToDecimalSafe(row.Table.Columns.Contains("ToplamUretilenMiktar") ? row["ToplamUretilenMiktar"] : null);
                current.Hakedis += ToDecimalSafe(row.Table.Columns.Contains("ToplamHakedis") ? row["ToplamHakedis"] : null);
                grouped[key] = current;
            }

            return grouped.Values
                .OrderBy(x => x.Tarih)
                .Select(x =>
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["Tarih"] = x.Tarih,
                        ["URUNGRUPADI"] = string.IsNullOrWhiteSpace(x.Urun) ? string.Empty : x.Urun,
                        ["ToplamUretilenMiktar"] = x.Uretim,
                        ["ToplamHakedis"] = x.Hakedis
                    };

                    return (object)row;
                })
                .ToList();
        }

        private static List<object> AggregateDashboardYilMiktarForAllFasons(SqlConnection con)
        {
            var fasonNames = GetAllFasonNames(con);
            var grouped = new Dictionary<string, Dictionary<string, decimal>>(StringComparer.OrdinalIgnoreCase);
            var numericColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var fason in fasonNames)
            {
                var part = QueryDashboardWithFasonFallback(con, "sp_FasonDashboard_ozet_YTdate_miktarlı", fason);
                foreach (var row in part)
                {
                    if (row is not IDictionary<string, object> data)
                        continue;

                    data.TryGetValue("URUNGRUPADI", out var urunVal);
                    var urun = ToStringSafe(urunVal);
                    if (string.IsNullOrWhiteSpace(urun))
                        urun = "(Bos)";

                    if (!grouped.TryGetValue(urun, out var totals))
                    {
                        totals = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                        grouped[urun] = totals;
                    }

                    foreach (var kv in data)
                    {
                        if (string.Equals(kv.Key, "URUNGRUPADI", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var value = ToDecimalSafe(kv.Value);
                        numericColumns.Add(kv.Key);
                        totals[kv.Key] = totals.TryGetValue(kv.Key, out var existing) ? existing + value : value;
                    }
                }
            }

            return grouped
                .OrderBy(x => x.Key)
                .Select(x =>
                {
                    var row = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["URUNGRUPADI"] = x.Key
                    };

                    foreach (var col in numericColumns)
                    {
                        row[col] = x.Value.TryGetValue(col, out var val) ? val : 0m;
                    }

                    return (object)row;
                })
                .ToList();
        }

        public FasonController(IConfiguration configuration, Processor processor)
        {
            _configuration = configuration;
            _processor = processor;
        }

        [HttpGet("GetFasonSurecOzet")]
        public IActionResult GetFasonSurecOzet(string fasonIsim)
        {
            try
            {
                // 1) Appsettings'teki şifreli connection string'i al
                var encryptedConnStr = _configuration.GetConnectionString("DatabaseConnection");
                var keyStr = _configuration["AesSettings:Key"];
                var vectorStr = _configuration["AesSettings:Vektor"];
                if (string.IsNullOrEmpty(encryptedConnStr) || string.IsNullOrEmpty(keyStr) || string.IsNullOrEmpty(vectorStr))
                    return StatusCode(500, new { Message = "Bağlantı ayarları eksik" });

                // AES key ve IV
                var aesKey = Convert.FromBase64String(keyStr);
                var aesVector = Convert.FromBase64String(vectorStr);

                // 2) Connection string'i çöz
                string connStr;
                using (Aes aes = Aes.Create())
                {
                    aes.Key = aesKey;
                    aes.IV = aesVector;
                    connStr = _processor.DecryptorSymmetric(encryptedConnStr, aes);
                }

                // 3) SP çağır
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

        [HttpGet("GetFasonDashboardOzet")]
        public IActionResult GetFasonDashboardOzet(string? fasonIsim = null)
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
                    var normalized = NormalizeFasonIsim(fasonIsim);
                    var data = normalized == null
                        ? AggregateDashboardOzetForAllFasons(con)
                        : QueryDashboardWithFasonFallback(con, "sp_FasonDashboard_ozet_MTDate", normalized).Cast<object>().ToList();

                    if (normalized == null && data.Count == 0)
                    {
                        data = AggregateDashboardOzetForAllFasons(con);
                    }

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        [HttpGet("GetFasonDashboardYTDateMiktarli")]
        public IActionResult GetFasonDashboardYTDateMiktarli(string? fasonIsim = null)
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
                    var normalized = NormalizeFasonIsim(fasonIsim);
                    var data = normalized == null
                        ? AggregateDashboardYilMiktarForAllFasons(con)
                        : QueryDashboardWithFasonFallback(con, "sp_FasonDashboard_ozet_YTdate_miktarlı", normalized).Cast<object>().ToList();

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        [HttpGet("GetFasonDashboardYTDateFiyatli")]
        public IActionResult GetFasonDashboardYTDateFiyatli(string? fasonIsim = null)
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
                    var data = QueryDashboardWithFasonFallback(con, "sp_FasonDashboard_ozet_YTdate_fiyatli", NormalizeFasonIsim(fasonIsim));

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }
        [HttpGet("GetFasonIsler")]
        public IActionResult GetFasonIsler()
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
                    var data = con.Query("sp_GetFasonIsler", commandType: CommandType.StoredProcedure).ToList();
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

        [HttpGet("GetFasonTakipFasonAppOzet")]
        public IActionResult GetFasonTakipFasonAppOzet(DateTime baslamatarihi, DateTime bitistarihi, string fasonsurec, string fasonisim)
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
                    parameters.Add("@baslamatarihi", baslamatarihi);
                    parameters.Add("@bitistarihi", bitistarihi);
                    parameters.Add("@Fasonsurec", fasonsurec);
                    parameters.Add("@Fasonisim", fasonisim);

                    var data = con.Query("sp_fasontakip_fasonapp_ozet",
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
        [HttpGet("GetBosFasonHakedis")]
        public IActionResult GetBosFasonHakedis(int? gunSayisi)
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
                    parameters.Add("@GunSayisi", gunSayisi ?? 40);

                    var data = con.Query("sp_bosfasonhakedisi", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        [HttpGet("GetFasonTakipFasonAppOzetKoli")]
        public IActionResult GetFasonTakipFasonAppOzetKoli(DateTime baslamatarihi, DateTime bitistarihi, string fasonsurec, string fasonisim)
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
                    parameters.Add("@baslamatarihi", baslamatarihi);
                    parameters.Add("@bitistarihi", bitistarihi);
                    parameters.Add("@Fasonsurec", fasonsurec);
                    parameters.Add("@Fasonisim", fasonisim);

                    var data = con.Query("sp_fasontakip_fasonapp_ozet_koli",
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

        [HttpGet("GetFasonFirmaadi")]
        public IActionResult GetFasonFirmaadi()
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

                    var data = con.Query("sp_GetFasonFirmaadi",
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
        [HttpGet("GetFasonDashboardYTDateMiktarliFasonFiyatli")]
        public IActionResult GetFasonDashboardYTDateMiktarliFasonFiyatli(string? fasonisim)
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
                    parameters.Add("@Fasonisim", fasonisim);

                    var data = con.Query("sp_FasonDashboard_ozet_YTdate_miktarlı_FASON_fiyatlı",
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

        [HttpGet("GetFasonDashboardYTDateMiktarliFason")]
        public IActionResult GetFasonDashboardYTDateMiktarliFason(string? fasonisim)
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
                    parameters.Add("@Fasonisim", fasonisim);

                    var data = con.Query(
                        "sp_FasonDashboard_ozet_YTdate_miktarlı_FASON",
                        parameters,
                        commandType: CommandType.StoredProcedure
                    ).ToList();

                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        public class CreateUretimEmriVeLotMutabakRequest
        {
            public string UrunKodu { get; set; }
            public string YariMamülKodu { get; set; }
            public int PlanlananMiktar { get; set; }
            public int KoliAdeti { get; set; }
            public string Fasonis { get; set; }
            public string Fasomsirket { get; set; }
            public string CreateUser { get; set; }
        }

        public class SurecDegistirLotRequest
        {
            public string LotNumarasi { get; set; }
            public string isim { get; set; }
            public string Surec { get; set; }
            public DateTime YuklemeTarihi { get; set; }
        }




        [HttpPost("SurecDegistirLot")]
        public IActionResult SurecDegistirLot([FromBody] SurecDegistirLotRequest request)
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
                    parameters.Add("@LotNumarasi", request.LotNumarasi);
                    parameters.Add("@isim", request.isim);
                    parameters.Add("@Surec", request.Surec);
                    parameters.Add("@YuklemeTarihi", request.YuklemeTarihi);

                    var result = con.Query("sp_SurecDegistirLot", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        [HttpPost("CreateUretimEmriVeLotMutabak")]
        public IActionResult CreateUretimEmriVeLotMutabak([FromBody] CreateUretimEmriVeLotMutabakRequest request)
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
                    parameters.Add("@YariMamülKodu", request.YariMamülKodu);
                    parameters.Add("@PlanlananMiktar", request.PlanlananMiktar);
                    parameters.Add("@KoliAdeti", request.KoliAdeti);
                    parameters.Add("@Fasonis", request.Fasonis);
                    parameters.Add("@Fasomsirket", request.Fasomsirket);
                    parameters.Add("@CreateUser", request.CreateUser);

                    var result = con.Query("sp_CreateUretimEmriVeLot_mutabak", parameters, commandType: CommandType.StoredProcedure).ToList();
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


        public class DeleteLotRequest
        {
            public string LotNumarasi { get; set; }
        }

        [HttpPost("DeleteLotByLotNumarasiManuel")]
        public IActionResult DeleteLotByLotNumarasiManuel([FromBody] DeleteLotRequest request)
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
                    parameters.Add("@LotNumarasi", request.LotNumarasi);

                    var result = con.QueryMultiple("sp_DeleteLotByLotNumarasi_MANUEL", parameters, commandType: CommandType.StoredProcedure);
                    var summary = result.Read().ToList();
                    var lottable = result.Read().ToList();
                    var lottakip = result.Read().ToList();
                    var fasonline = result.Read().ToList();

                    return Ok(new
                    {
                        Summary = summary,
                        DeletedLottable = lottable,
                        DeletedLotTakip = lottakip,
                        DeletedFasonAracYuklemeLine = fasonline
                    });
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


        public class ReassignLotToNewFasonYuklemeRequest
        {
            public string LotNumarasi { get; set; }
            public DateTime YeniYuklemeTarihi { get; set; }
            public string CreateUser { get; set; }
            public bool DeleteEmptyHeader { get; set; }
        }

        [HttpPost("ReassignLotToNewFasonYukleme")]
        public IActionResult ReassignLotToNewFasonYukleme([FromBody] ReassignLotToNewFasonYuklemeRequest request)
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
                    parameters.Add("@LotNumarasi", request.LotNumarasi);
                    parameters.Add("@YeniYuklemeTarihi", request.YeniYuklemeTarihi);
                    parameters.Add("@CreateUser", request.CreateUser);
                    parameters.Add("@DeleteEmptyHeader", request.DeleteEmptyHeader);

                    var result = con.Query("sp_ReassignLotToNewFasonYukleme", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        public class UpdateUretilenMiktarByLotRequest
        {
            public string LotNumarasi { get; set; }
            public decimal YeniUretilenMiktar { get; set; }
            public string UpdateUser { get; set; }
        }

        [HttpPost("UpdateUretilenMiktarByLotManuel")]
        public IActionResult UpdateUretilenMiktarByLotManuel([FromBody] UpdateUretilenMiktarByLotRequest request)
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
                    parameters.Add("@LotNumarasi", request.LotNumarasi);
                    parameters.Add("@YeniUretilenMiktar", request.YeniUretilenMiktar);
                    parameters.Add("@UpdateUser", request.UpdateUser);

                    var result = con.Query("sp_UpdateUretilenMiktarByLot_MANUEL", parameters, commandType: CommandType.StoredProcedure).ToList();
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

        // Tüm kayıtları getirir
        [HttpGet("GetFasonHakedisAll")]
        public IActionResult GetFasonHakedisAll()
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
                    parameters.Add("@Fasonisim", null);
                    parameters.Add("@StokKodu", null);
                    parameters.Add("@UrunGrupAdi", null);

                    var data = con.Query("sp_Getfasonhakedis", parameters, commandType: CommandType.StoredProcedure).ToList();
                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        // Sadece belirli fason için
        [HttpGet("GetFasonHakedisByFasonisim")]
        public IActionResult GetFasonHakedisByFasonisim(string fasonisim)
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
                    parameters.Add("@Fasonisim", fasonisim);
                    parameters.Add("@StokKodu", null);
                    parameters.Add("@UrunGrupAdi", null);

                    var data = con.Query("sp_Getfasonhakedis", parameters, commandType: CommandType.StoredProcedure).ToList();
                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        // Belirli stok kodu için
        [HttpGet("GetFasonHakedisByStokKodu")]
        public IActionResult GetFasonHakedisByStokKodu(string stokKodu)
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
                    parameters.Add("@Fasonisim", null);
                    parameters.Add("@StokKodu", stokKodu);
                    parameters.Add("@UrunGrupAdi", null);

                    var data = con.Query("sp_Getfasonhakedis", parameters, commandType: CommandType.StoredProcedure).ToList();
                    return Ok(data);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Sunucu hatası", Detail = ex.Message });
            }
        }

        // Belirli ürün grubu için
        [HttpGet("GetFasonHakedisByUrunGrupAdi")]
        public IActionResult GetFasonHakedisByUrunGrupAdi(string urunGrupAdi)
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
                    parameters.Add("@Fasonisim", null);
                    parameters.Add("@StokKodu", null);
                    parameters.Add("@UrunGrupAdi", urunGrupAdi);

                    var data = con.Query("sp_Getfasonhakedis", parameters, commandType: CommandType.StoredProcedure).ToList();
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
