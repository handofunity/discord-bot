Set-Location $PSScriptRoot

# Read local development database information from local.flyway.user.conf,
# which should contain the host and username.
$flywayConf = Get-Content -Path "local.flyway.user.conf"
$url = $flywayConf[0].Replace('flyway.url=jdbc:postgresql://', '')
$splitByHostAndDb = $url.Split('/')
$splitByHostAddressAndPort = $splitByHostAndDb[0].Split(':')
$hostAddress = $splitByHostAddressAndPort[0]
$port = $splitByHostAddressAndPort[1]
$databaseName = $splitByHostAndDb[1]
$username = $flywayConf[1].Replace('flyway.user=', '')

# Ask for backup file
$backupFilePath = Read-Host -Prompt "Path to backup to restore (*.tar)"

Write-Host "Parameters for pg_restore:" -ForegroundColor Yellow
Write-Host "host: $hostAddress" -ForegroundColor Yellow
Write-Host "port: $port" -ForegroundColor Yellow
Write-Host "dbname: $databaseName" -ForegroundColor Yellow
Write-Host "username: $username" -ForegroundColor Yellow

Write-Host "Starting pg_restore ..." -ForegroundColor Cyan

& pg_restore --data-only `
             --host=$hostAddress `
             --port=$port `
             --dbname=$databaseName `
             --username=$username `
             $backupFilePath
             
Write-Host "Completed pg_restore of file ""$backupFilePath"" to local database ""$databaseName""." -ForegroundColor Cyan