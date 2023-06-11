Set-Location $PSScriptRoot

flyway -configFiles="production.flyway.user.conf" migrate