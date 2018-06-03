# Load settings
$settingsPath = "settings.houguildbot.ini"
$absoluteSettingsPath = Join-Path -Path $PSScriptRoot -ChildPath $settingsPath
$settings = Get-IniContent $absoluteSettingsPath
# Set variables
$sourceImage = "$($settings['Image']['SourceName']):$($settings['Image']['SourceTag'])"
$targetImage = "$($settings['Network']['RegistryHost']):$($settings['Network']['RegistryPort'])/$($settings['Image']['TargetName']):$($settings['Image']['TargetTag'])"
# Docker tag
Start-Process "docker" -ArgumentList "tag $($sourceImage) $($targetImage)" -Wait
# Docker push
Start-Process "docker" -ArgumentList "push $($targetImage)" -Wait

function Get-IniContent {
    [CmdletBinding()]  
    Param(  
        [ValidateNotNullOrEmpty()]  
        [ValidateScript({(Test-Path $_) -and ((Get-Item $_).Extension -eq ".ini")})]  
        [Parameter(ValueFromPipeline=$True,Mandatory=$True)]  
        [string]$FilePath  
    )  

    $ini = @{}  
        switch -regex -file $FilePath  
        {  
            "^\[(.+)\]$" # Section  
            {  
                $section = $matches[1]  
                $ini[$section] = @{}  
                $CommentCount = 0  
            }  
            "^(;.*)$" # Comment  
            {  
                if (!($section))  
                {  
                    $section = "No-Section"  
                    $ini[$section] = @{}  
                }  
                $value = $matches[1]  
                $CommentCount = $CommentCount + 1  
                $name = "Comment" + $CommentCount  
                $ini[$section][$name] = $value  
            }   
            "(.+?)\s*=\s*(.*)" # Key  
            {  
                if (!($section))  
                {  
                    $section = "No-Section"  
                    $ini[$section] = @{}  
                }  
                $name,$value = $matches[1..2]  
                $ini[$section][$name] = $value  
            }  
        }  
        Return $ini  
}