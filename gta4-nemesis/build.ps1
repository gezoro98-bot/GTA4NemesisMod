param(
    [ValidateSet("Debug", "Release")] [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"
$root = Split-Path -Parent $PSScriptRoot
$src = Join-Path $root "src\NemesisMod"
$lib = Join-Path $root "lib"
$dist = Join-Path $root "dist"

# Проверенный релиз ScriptHookDotNet (CE, поддержка 1.2.0.59)
$DllUrl = "https://github.com/Tomasak/gta4_scripthookdotnet/releases/download/release/scripthookdotnet_v1.7.1.8.zip"

Write-Host "=== GTA IV: Nemesis & Gang Wars - сборка ===" -ForegroundColor Cyan

# 1. ScriptHookDotNet.dll (ссылка для компиляции)
$ref = Join-Path $lib "ScriptHookDotNet.dll"
if (-not (Test-Path $ref)) {
    Write-Host "ScriptHookDotNet.dll не найден - скачиваю форк Tomasak (CE, 1.2.0.59)..." -ForegroundColor Cyan
    $zip = Join-Path $env:TEMP "scripthookdotnet.zip"
    $ex  = Join-Path $env:TEMP "scripthookdotnet_extract"
    try {
        [Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12 -bor [Net.SecurityProtocolType]::Tls13
        Invoke-WebRequest -Uri $DllUrl -OutFile $zip -UseBasicParsing
        if (Test-Path $ex) { Remove-Item $ex -Recurse -Force }
        Expand-Archive -Path $zip -DestinationPath $ex -Force
        $dll = Get-ChildItem -Recurse $ex -Filter "ScriptHookDotNet.dll" | Select-Object -First 1
        if (-not $dll) { throw "ScriptHookDotNet.dll не найден в архиве" }
        Copy-Item $dll.FullName $ref -Force
        Write-Host "Готово: $ref" -ForegroundColor Green
    }
    catch {
        Write-Host "Не удалось скачать ScriptHookDotNet.dll: $($_.Exception.Message)" -ForegroundColor Yellow
        Write-Host "Скачайте архив вручную ($DllUrl) и положите ScriptHookDotNet.dll в папку lib" -ForegroundColor Yellow
        exit 1
    }
    finally {
        Remove-Item $zip -ErrorAction SilentlyContinue
        if (Test-Path $ex) { Remove-Item $ex -Recurse -Force -ErrorAction SilentlyContinue }
    }
}

# 2. Сборка C# скрипта
$proj = Join-Path $src "NemesisMod.csproj"
dotnet restore $proj
dotnet build $proj -c $Configuration
if ($LASTEXITCODE -ne 0) {
    Write-Host "Сборка завершилась с ошибкой." -ForegroundColor Red
    exit $LASTEXITCODE
}

# 3. Копируем результат в dist\
New-Item -ItemType Directory -Force -Path (Join-Path $dist "scripts") | Out-Null
$dll = Join-Path $src "bin\Release\NemesisMod.net.dll"
if (Test-Path $dll) {
    Copy-Item $dll (Join-Path $dist "scripts\NemesisMod.net.dll") -Force
    Write-Host "Собран скрипт: $dll" -ForegroundColor Green
} else {
    Write-Host "Не найден выходной DLL: $dll" -ForegroundColor Red
    exit 1
}

$iniSrc = Join-Path $src "NemesisMod.ini.example"
if (Test-Path $iniSrc) {
    Copy-Item $iniSrc (Join-Path $dist "scripts\NemesisMod.ini") -Force
}

# 4. Сборка NSIS-инсталлятора (если установлен makensis)
$makensis = Get-Command makensis -ErrorAction SilentlyContinue
if ($makensis) {
    Write-Host "Сборка установщика NSIS..." -ForegroundColor Cyan
    $mkPath = $makensis.Source
    & $mkPath (Join-Path $root "installer\GTA4Nemesis.nsi")
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Установщик собран в папку dist\." -ForegroundColor Green
    } else {
        Write-Host "makensis завершился с ошибкой (код $LASTEXITCODE)." -ForegroundColor Yellow
    }
} else {
    Write-Host "NSIS (makensis) не найден - .exe не собран." -ForegroundColor Yellow
    Write-Host "Можно установить через install.ps1 или вручную скопировать dist\scripts\ в папку игры." -ForegroundColor Yellow
}

Write-Host "=== Готово ===" -ForegroundColor Cyan
