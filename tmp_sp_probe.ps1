$app = Get-Content "c:\Projeler\FasonBackend-master\WebApi\appsettings.json" -Raw | ConvertFrom-Json
$enc = $app.ConnectionStrings.DatabaseConnection
$key = [Convert]::FromBase64String($app.AesSettings.Key)
$iv = [Convert]::FromBase64String($app.AesSettings.Vektor)
$cipher = [Convert]::FromBase64String($enc)
$aes = [System.Security.Cryptography.Aes]::Create()
$aes.Key = $key
$aes.IV = $iv
$ms = New-Object System.IO.MemoryStream(,$cipher)
$cs = New-Object System.Security.Cryptography.CryptoStream($ms, $aes.CreateDecryptor(), [System.Security.Cryptography.CryptoStreamMode]::Read)
$sr = New-Object System.IO.StreamReader($cs)
$connStr = $sr.ReadToEnd()
$sr.Close(); $cs.Close(); $ms.Close()
$builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder($connStr)
$conn = New-Object System.Data.SqlClient.SqlConnection($builder.ConnectionString)
$conn.Open()
function Invoke-SpInfo([string]$sp,[object]$fason){
  $cmd = $conn.CreateCommand()
  $cmd.CommandText = $sp
  $cmd.CommandType = [System.Data.CommandType]::StoredProcedure
  $p = $cmd.Parameters.Add("@Fasonisim", [System.Data.SqlDbType]::NVarChar, 255)
  if($null -eq $fason){ $p.Value = [DBNull]::Value } else { $p.Value = $fason }
  $da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
  $dt = New-Object System.Data.DataTable
  [void]$da.Fill($dt)
  $cols = ($dt.Columns | ForEach-Object { $_.ColumnName }) -join ','
  Write-Output ("SP="+$sp+" fason="+([string]$fason)+" rows="+$dt.Rows.Count+" cols="+$cols)
  if($dt.Rows.Count -gt 0){
    $r = $dt.Rows[0]
    $sample = @()
    foreach($c in $dt.Columns){ $sample += ($c.ColumnName+"="+$r[$c.ColumnName]) }
    Write-Output ("SAMPLE=" + ($sample -join '; '))
  }
}
$cmdF = $conn.CreateCommand()
$cmdF.CommandText = "sp_GetFasonFirmaadi"
$cmdF.CommandType = [System.Data.CommandType]::StoredProcedure
$daF = New-Object System.Data.SqlClient.SqlDataAdapter($cmdF)
$dtF = New-Object System.Data.DataTable
[void]$daF.Fill($dtF)
$firstFason = if($dtF.Rows.Count -gt 0){ [string]$dtF.Rows[0][0] } else { '' }
Write-Output ("FIRST_FASON="+$firstFason)
Invoke-SpInfo "sp_FasonDashboard_ozet_MTDate" $null
Invoke-SpInfo "sp_FasonDashboard_ozet_MTDate" "Null"
Invoke-SpInfo "sp_FasonDashboard_ozet_MTDate" $firstFason
Invoke-SpInfo "sp_FasonDashboard_ozet_YTdate_miktarlı" $null
Invoke-SpInfo "sp_FasonDashboard_ozet_YTdate_miktarlı" "Null"
Invoke-SpInfo "sp_FasonDashboard_ozet_YTdate_miktarlı" $firstFason
$conn.Close()
