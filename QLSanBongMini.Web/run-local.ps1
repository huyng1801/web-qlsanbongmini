# Chay local - SQLite tu dong neu khong co SQL Server
$ErrorActionPreference = "Stop"
$env:DOTNET_ROLL_FORWARD = "LatestPatch"
$env:ASPNETCORE_ENVIRONMENT = "Development"
Set-Location $PSScriptRoot

Write-Host "[build]..." -ForegroundColor Cyan
dotnet build -c Release | Out-Null
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$dll = Join-Path $PSScriptRoot "bin\Release\net8.0\QLSanBongMini.Web.dll"
$url = "http://localhost:5099"
Write-Host "[run] $url (Ctrl+C de dung)" -ForegroundColor Green
dotnet exec $dll --urls $url
