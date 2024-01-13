#Requires -Version 7.0
#Requires -Modules @{ ModuleName="PSToml"; ModuleVersion="0.3.0" }

Set-Location $PSScriptRoot

$backupDirectory = "$PSScriptRoot\backups"
if (!(Test-Path -Path $backupDirectory)) {
    New-Item -ItemType Directory -Force -Path $backupDirectory
}

# Read production user host from flyway.user.toml.
# File should contain a section for production environment.
$flywayUserContent = Get-Content -Path "flyway.user.toml" -Raw
$flywayUserConfig = ConvertFrom-Toml $flywayUserContent
$username = $flywayUserConfig.environments.production.user
$productionUrl = $flywayUserConfig.environments.production.url
$productionUrl = $productionUrl.Replace('jdbc:postgresql://', '')
$splitByHostAndDb = $productionUrl.Split('/')
$splitByHostAddressAndPort = $splitByHostAndDb[0].Split(':')
$hostAddress = $splitByHostAddressAndPort[0]
$port = $splitByHostAddressAndPort[1]
$databaseName = $splitByHostAndDb[1]

$resultFileName = "$backupDirectory\$($databaseName)_$(Get-Date -UFormat "%Y-%m-%d_%H-%M-%S").tar"

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