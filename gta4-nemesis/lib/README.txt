Положите сюда ScriptHookDotNet.dll (x86).

Для GTA IV Complete Edition (1.2.0.x, Steam/RGL) используйте форк Tomasak.
Проверенный релиз «GTA4 .Net ScriptHook v1.7.1.8» (в описании указана поддержка
1.2.0.59 и исправление «дождя в помещениях»; в архив входит aCompleteEditionHook.asi):
  https://github.com/Tomasak/gta4_scripthookdotnet/releases/download/release/scripthookdotnet_v1.7.1.8.zip

Файл должен называться ровно ScriptHookDotNet.dll.
Без него сборка (build.ps1 / dotnet build) не скомпилируется — это ожидаемо.

Дополнительно в саму игру ставятся (отдельно от этого мода):
  1) ASI-загрузчик, работающий с Complete Edition (например aCompleteEditionHook.asi / обновлённый dsound.dll);
  2) нативный ScriptHook.dll;
  3) ScriptHookDotNet.asi (берётся из того же форка Tomasak).
