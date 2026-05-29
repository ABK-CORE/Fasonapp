$app = Get-Content "c:\Projeler\FasonBackend-master\WebApi\appsettings.json" -Raw | ConvertFrom-Json
$enc = $app.ConnectionStrings.DatabaseConnection
$key = [Convert]::FromBase64String($app.AesSettings.Key)
$iv = [Convert]::FromBase64String($app.AesSettings.Vektor)
$cipher = [Convert]::FromBase64String($enc)
$aes = [System.Security.Cryptography.Aes]::Create(); $aes.Key = $key; $aes.IV = $iv
$ms = New-Object System.IO.MemoryStream(,$cipher)
$cs = New-Object System.Security.Cryptography.CryptoStream($ms, $aes.CreateDecryptor(), [System.Security.Cryptography.CryptoStreamMode]::Read)
$sr = New-Object System.IO.StreamReader($cs)
$connStr = $sr.ReadToEnd(); $sr.Close(); $cs.Close(); $ms.Close()
$conn = New-Object System.Data.SqlClient.SqlConnection($connStr); $conn.Open()
$cmd = $conn.CreateCommand(); $cmd.CommandText = "sp_fasontakip_fasonapp_ozet_koli"; $cmd.CommandType = [System.Data.CommandType]::StoredProcedure
$cmd.Parameters.Add("@baslamatarihi", [System.Data.SqlDbType]::DateTime).Value = [datetime]"2025-01-01"
$cmd.Parameters.Add("@bitistarihi", [System.Data.SqlDbType]::DateTime).Value = [datetime]"2026-12-31"
$cmd.Parameters.Add("@Fasonsurec", [System.Data.SqlDbType]::NVarChar, 100).Value = ""
$cmd.Parameters.Add("@Fasonisim", [System.Data.SqlDbType]::NVarChar, 255).Value = "ADLE TOSUN"
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd); $dt = New-Object System.Data.DataTable; [void]$da.Fill($dt)
Write-Output ("ROWS="+$dt.Rows.Count)
if($dt.Rows.Count -gt 0){ $r=$dt.Rows[0]; Write-Output ("SAMPLE=" + ($dt.Columns | % { $_.ColumnName+"="+$r[$_.ColumnName] } | Out-String)) }
$conn.Close()
