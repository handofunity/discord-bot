$context = Join-Path -Path $PSScriptRoot -ChildPath '..\src\'
$dockerfile = Join-Path -Path $PSScriptRoot -ChildPath '..\src\WebHost\Dockerfile'

docker build $context -f $dockerfile -t houguildbot:latest