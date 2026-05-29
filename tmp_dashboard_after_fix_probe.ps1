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
$cmd.Parameters.Add("@baslamatarihi", [System.Data.SqlDbType]::DateTime).Value = (Get-Date).Date.AddMonths(-12)
$cmd.Parameters.Add("@bitistarihi", [System.Data.SqlDbType]::DateTime).Value = (Get-Date).Date.AddDays(1).AddTicks(-1)
$cmd.Parameters.Add("@Fasonsurec", [System.Data.SqlDbType]::NVarChar,100).Value = ""
$cmd.Parameters.Add("@Fasonisim", [System.Data.SqlDbType]::NVarChar,255).Value = ""
$da = New-Object System.Data.SqlClient.SqlDataAdapter($cmd); $dt = New-Object System.Data.DataTable; [void]$da.Fill($dt)
$grp = $dt | Group-Object { "$($_.Tarih.ToString('yyyy-MM-dd'))|$($_.URUNGRUPADI)" } | ForEach-Object {
  $parts = $_.Name.Split('|',2)
  [PSCustomObject]@{
    Tarih = $parts[0]
    URUNGRUPADI = $parts[1]
    ToplamUretilenMiktar = ($_.Group | Measure-Object -Property ToplamUretilenMiktar -Sum).Sum
    ToplamHakedis = ($_.Group | Measure-Object -Property ToplamHakedis -Sum).Sum
  }
}
Write-Output ("RAW_ROWS="+$dt.Rows.Count)
Write-Output ("GROUPED_ROWS="+$grp.Count)
$grp | Sort-Object Tarih | Select-Object -First 5 | Format-Table -AutoSize | Out-String | Write-Output
$conn.Close()
