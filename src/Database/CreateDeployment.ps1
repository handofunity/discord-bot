$sqlPackageExe = "C:\Program Files (x86)\Microsoft SQL Server\140\DAC\bin\SqlPackage.exe"
$sqlPublishProfile = $PSScriptRoot + "\Database_DeploymentScript.publish.xml"
$sourceFile = $PSScriptRoot + "\bin\Output\Database.dacpac"
$deploymentDirectory = $PSScriptRoot + "\_Deployment"
$latestDirectory = $deploymentDirectory + "\latest"
$latestDacPacFile = $latestDirectory + "\Database.dacpac"

# Copy current DacPac to 'latest' directory
New-Item $latestDirectory -ItemType Directory -Force | Out-Null
Write-Host "Copying $($sourceFile) to $($latestDacPacFile)."
Copy-Item $sourceFile $latestDacPacFile -Recurse -Force | Out-Null

# Determine newest version
Write-Host "Determining newest DacPac version in $($deploymentDirectory)."
$newestVersion = Get-ChildItem $deploymentDirectory -Directory | Where-Object Name -Match "\d+\.\d+\.\d+" | Select-Object Name | ForEach-Object { [System.Version]::Parse($_.Name) } | Sort-Object -Descending | Select-Object -First 1
if (!$newestVersion) {
    $newestVersion = "0.0.0"
}
else {
    $newestVersion = $newestVersion.ToString(3)
}

# Prepare comparison of DacPac version
$newestDacPacFile = $deploymentDirectory + "\$($newestVersion)\Database.dacpac"
$latestDeploymentScript = $latestDirectory + "\Database_$($newestVersion)_latest.sql"
# Source file: latest version from the "\bin\Output" directory
# Target file: newest version from the "\_Deployment" directory
& $sqlPackageExe /Action:Script /Profile:$sqlPublishProfile /SourceFile:$latestDacPacFile /TargetFile:$newestDacPacFile /OutputPath:$latestDeploymentScript

# Clean deployment script
Write-Host "Cleaning up deployment script"
Write-Host "Reading generated content from '$($latestDeploymentScript)' ..."
$content = [IO.File]::ReadAllText($latestDeploymentScript)
Write-Host "Removing multi-line comments"
$content = ($content -replace "(?ms)/\*.*?\*/", "")
Write-Host "Removing USE statements"
$content = ($content -replace "(?m)\r\n?\r\n?GO\r\n?USE \[\$\(DatabaseName\)\];\r\n?\r\n?\r\n?GO", "GO")
Write-Host "Removing on error exit"
$content = ($content -replace "(?m)GO\r\n?:on error exit\r\n?GO", "GO")
Write-Host "Removing :setvar block"
$content = ($content -replace "(?m)GO\r\n?((:setvar.*)\r?\n?)+", "")
Write-Host "Removing :setvar IsSqlCmdEnabled"
$content = ($content -replace "(?m):setvar __IsSqlCmdEnabled ""True""\r\n?GO", "")
Write-Host "Removing IsSqlCmdEnabled warning"
$content = ($content -replace "(?ms)IF N'\$\(__IsSqlCmdEnabled\)' NOT LIKE N'True'.*?END\r\n?GO", "")
Write-Host "Cleaning empty GO statements"
$content = ($content -replace "(?m)GO(\r\n?)*?GO", "GO")
Write-Host "Cleaning leading GO statements"
$content = ($content -replace "(?m)(\r\n?){2,}GO", "`r`nGO")
Write-Host "Ensuring one blank line after GO statements"
$content = ($content -replace "(?m)GO(\r\n?)+", "GO`r`n`r`n")
Write-Host "Removing first GO statement"
$content = ($content -replace "(?m)^\r\n?GO\r\n?\r\n?", "")
Write-Host "Removing training lines after last GO statement"
$content = ($content -replace "GO(\r\n?)+$", "GO")
[IO.File]::WriteAllText($latestDeploymentScript, $content)