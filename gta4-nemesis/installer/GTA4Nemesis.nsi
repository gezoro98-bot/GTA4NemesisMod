; ============================================================
;  GTA IV: Nemesis & Gang Wars — установщик (NSIS)
;  Устанавливает скрипт мода в папку scripts/ игры.
; ============================================================

!define APPNAME "GTA IV: Nemesis & Gang Wars"
!define APPVERSION "0.1.0"
!define APPUNINST "GTA4NemesisUninstall"
!define COMPANY "Community Mod"

Name "${APPNAME}"
OutFile "..\dist\GTA4Nemesis_Setup_${APPVERSION}.exe"
InstallDir "$PROGRAMFILES32\Grand Theft Auto IV\GTAIV"
RequestExecutionLevel admin

!include "LogicLib.nsh"
!include "MUI2.nsh"

!define MUI_ABORTWARNING

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Russian"
!insertmacro MUI_LANGUAGE "English"

Function .onInit
  ; --- Detect Steam install ---
  ReadRegStr $0 HKCU "Software\Valve\Steam" "SteamPath"
  ${If} $0 != ""
    StrCpy $INSTDIR "$0\steamapps\common\Grand Theft Auto IV\GTAIV"
  ${EndIf}

  ; --- Detect Rockstar Games Launcher install ---
  ReadRegStr $1 HKLM "SOFTWARE\WOW6432Node\Rockstar Games\Grand Theft Auto IV" "InstallFolder"
  ${If} $1 != ""
    StrCpy $INSTDIR "$1"
  ${EndIf}
  ReadRegStr $1 HKLM "SOFTWARE\Rockstar Games\Grand Theft Auto IV" "InstallFolder"
  ${If} $1 != ""
    StrCpy $INSTDIR "$1"
  ${EndIf}
FunctionEnd

Section "Файлы мода" SecMain
  SetOutPath "$INSTDIR\scripts"
  File "..\dist\scripts\NemesisMod.net.dll"
  File "..\dist\scripts\NemesisMod.ini"

  ; --- Uninstaller ---
  WriteUninstaller "$INSTDIR\${APPUNINST}.exe"

  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" \
                   "DisplayName" "${APPNAME}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" \
                   "DisplayVersion" "${APPVERSION}"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" \
                   "UninstallString" '"$INSTDIR\${APPUNINST}.exe"'
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}" \
                   "InstallLocation" "$INSTDIR"
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\scripts\NemesisMod.net.dll"
  Delete "$INSTDIR\scripts\NemesisMod.ini"
  Delete "$INSTDIR\${APPUNINST}.exe"

  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPNAME}"

  ; не удаляем папку scripts целиком — там могут быть другие моды
SectionEnd

Function .onInstSuccess
  MessageBox MB_OK "Мод установлен в:$r$n$INSTDIR\scripts$r$n$r$nУбедитесь, что в игру уже установлены ASI-загрузчик, ScriptHook.dll и ScriptHookDotNet.asi (см. README)."
FunctionEnd
