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
function Probe([string]$sp,[string]$surec,[string]$fason){
  $cmd = $conn.CreateCommand(); $cmd.CommandText = $sp; $cmd.CommandType = [System.Data.CommandType]::StoredProcedure
  [void]$cmd.Parameters.Add("@baslamatarihi", [System.Data.SqlDbType]::DateTime)
  [void]$cmd.Parameters.Add("@bitistarihi", [System.Data.SqlDbType]::DateTime)
  [void]$cmd.Parameters.Add("@Fasonsurec", [System.Data.SqlDbType]::NVarChar, 100)
  [void]$cmd.Parameters.Add("@Fasonisim", [System.Data.SqlDbType]::NVarChar, 255)
  $cmd.Parameters["@baslamatarihi"].Value = [datetime]"2025-01-01"
  $cmd.Parameters["@bitistarihi"].Value = [datetime]"2026-12-31"
  $cmd.Parameters["@Fasonsurec"].Value = $surec
  $cmd.Parameters["@Fasonisim"].Value = $fason
  $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
  $dt = New-Object System.Data.DataTable
  [void]$da.Fill($dt)
  $cols = ($dt.Columns | ForEach-Object { $_.ColumnName }) -join ','
  Write-Output ("SP="+$sp+" surec="+$surec+" fason="+$fason+" rows="+$dt.Rows.Count+" cols="+$cols)
  if($dt.Rows.Count -gt 0){
    $r = $dt.Rows[0]
    $sample = @(); foreach($c in $dt.Columns){ $sample += ($c.ColumnName+"="+$r[$c.ColumnName]) }
    Write-Output ("SAMPLE="+($sample -join '; '))
  }
}
Probe "sp_fasontakip_fasonapp_ozet" "FASONDANTESLIMAL" ""
Probe "sp_fasontakip_fasonapp_ozet" "" ""
Probe "sp_fasontakip_fasonapp_ozet_koli" "FASONDANTESLIMAL" ""
Probe "sp_fasontakip_fasonapp_ozet_koli" "" ""
$conn.Close()
