Set-Location $PSScriptRoot

$backupDirectory = "$PSScriptRoot\backups"
if (!(Test-Path -Path $backupDirectory)) {
    New-Item -ItemType Directory -Force -Path $backupDirectory
}

# Read production host from production.flyway.user.conf,
# which should only contain a single line with the connection string.
$flywayConf = Get-Content -Path "production.flyway.user.conf" -Raw
$flywayConf = $flywayConf.Replace('flyway.url=jdbc:postgresql://', '')
$splitByHostAndDb = $flywayConf.Split('/')
$splitByHostAddressAndPort = $splitByHostAndDb[0].Split(':')
$hostAddress = $splitByHostAddressAndPort[0]
$port = $splitByHostAddressAndPort[1]
$databaseName = $splitByHostAndDb[1]

$resultFileName = "$backupDirectory\$($databaseName)_$(Get-Date -UFormat "%Y-%m-%d_%H-%M-%S").tar"

# Ask for credentials
$username = Read-Host -Prompt "User name for connection"

Write-Host "Parameters for pg_dump:" -ForegroundColor Yellow
Write-Host "host: $hostAddress" -ForegroundColor Yellow
Write-Host "port: $port" -ForegroundColor Yellow
Write-Host "dbname: $databaseName" -ForegroundColor Yellow
Write-Host "username: $username" -ForegroundColor Yellow

Write-Host "Starting pg_dump ..." -ForegroundColor Cyan

& pg_dump --data-only `
          --file=$resultFileName `
          --schema=hou `
          --schema=config `
          --exclude-table="config.units_endpoint" `
          --exclude-table="config.flyway_schema_history" `
          --format=tar `
          --host=$hostAddress `
          --port=$port `
          --dbname=$databaseName `
          --username=$username

Write-Host "Completed pg_dump to file ""$resultFileName""." -ForegroundColor Cyan