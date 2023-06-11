Set-Location $PSScriptRoot

flyway -configFiles="local.flyway.user.conf" migrate