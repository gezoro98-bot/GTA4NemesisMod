# install.ps1 — установка мода без NSIS (находит папку игры и копирует файлы).
# Сначала соберите мод:  .\build.ps1
param([string]$GameDir = "")

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$distScripts = Join-Path $root "dist\scripts"

if (-not (Test-Path (Join-Path $distScripts "NemesisMod.net.dll"))) {
    Write-Host "Сначала соберите мод: запустите build.ps1" -ForegroundColor Red
    exit 1
}

function Find-GameDir {
    if ($GameDir -and (Test-Path (Join-Path $GameDir "GTAIV.exe"))) { return $GameDir }

    # Steam
    foreach ($hive in 'HKCU:\Software\Valve\Steam', 'HKLM:\Software\Valve\Steam', 'HKLM:\SOFTWARE\WOW6432Node\Valve\Steam') {
        try {
            $steamPath = (Get-ItemProperty -Path $hive -Name SteamPath -ErrorAction Stop).SteamPath
            $cand = Join-Path $steamPath "steamapps\common\Grand Theft Auto IV\GTAIV"
            if (Test-Path (Join-Path $cand "GTAIV.exe")) { return $cand }
        } catch { }
    }

    # Rockstar Games Launcher
    foreach ($hive in 'HKLM:\SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto IV', 'HKLM:\SOFTWARE\Rockstar Games\Grand Theft Auto IV') {
        try {
            $p = (Get-ItemProperty -Path $hive -Name InstallFolder -ErrorAction Stop).InstallFolder
            if ($p -and (Test-Path (Join-Path $p "GTAIV.exe"))) { return $p }
        } catch { }
    }

    return $null
}

$dir = Find-GameDir
if (-not $dir) {
    Write-Host "Папка игры не найдена автоматически." -ForegroundColor Yellow
    $dir = Read-Host "Укажите папку с GTAIV.exe"
}
if (-not $dir -or -not (Test-Path (Join-Path $dir "GTAIV.exe"))) {
    Write-Host "В указанной папке нет GTAIV.exe." -ForegroundColor Red
    exit 1
}

$scripts = Join-Path $dir "scripts"
New-Item -ItemType Directory -Force -Path $scripts | Out-Null
Copy-Item (Join-Path $distScripts "NemesisMod.net.dll") (Join-Path $scripts "NemesisMod.net.dll") -Force
Copy-Item (Join-Path $distScripts "NemesisMod.ini") (Join-Path $scripts "NemesisMod.ini") -Force

Write-Host "Мод установлен в: $scripts" -ForegroundColor Green
Write-Host "Убедитесь, что в папку с GTAIV.exe уже установлены ASI-загрузчик, ScriptHook.dll и ScriptHookDotNet.asi (см. README)." -ForegroundColor Yellow
