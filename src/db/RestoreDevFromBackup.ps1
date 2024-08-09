#Requires -Version 7.0
#Requires -Modules @{ ModuleName="PSToml"; ModuleVersion="0.3.0" }

Set-Location $PSScriptRoot

# Read development database information from flyway.user.toml.
# File should contain a section for development environment.
$flywayUserContent = Get-Content -Path "flyway.user.toml" -Raw
$flywayUserConfig = ConvertFrom-Toml $flywayUserContent
$username = $flywayUserConfig.environments.development.user
$developmentUrl = $flywayUserConfig.environments.development.url
$developmentUrl = $developmentUrl.Replace('jdbc:postgresql://', '')
$splitByHostAndDb = $developmentUrl.Split('/')
$splitByHostAddressAndPort = $splitByHostAndDb[0].Split(':')
$hostAddress = $splitByHostAddressAndPort[0]
$port = $splitByHostAddressAndPort[1]
$databaseName = $splitByHostAndDb[1]

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