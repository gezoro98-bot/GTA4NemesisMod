# GTA IV: Nemesis \& Gang Wars

Модификация для **GTA IV Complete Edition (1.2.0.x, Steam / Rockstar Games Launcher)**,
написанная на C# поверх **ScriptHookDotNet**. Добавляет в игру живую криминальную
песочницу с персональными врагами, войнами банд, новыми заданиями и активностями.

> Вдохновлено системой Nemesis из \\\\\\\*Middle-earth: Shadow of Mordor / Shadow of War\\\\\\\*
> и фанатским модом \\\\\\\*enRage: Living World and Nemesis System\\\\\\\* для RDR2.

\---

## Возможности

|Модуль|Что делает|Готовность|
|-|-|-|
|**Nemesis**|Именованные члены банд с рангом, уровнем и чертами. Они запоминают вас, мстят, повышаются, а у игрока появляется личная **Немезида**, которая выслеживает его.|✅|
|**Войны банд**|Территории Либерти-Сити переходят из рук в руки; банды атакуют ваши зоны, их надо отбивать.|✅|
|**Задания**|Основное («охота на Немезиду») и побочные («оборона территории», «устранение цели») — цепочки целей с метками и наградами.|✅|
|**Активности**|Розыск банд и преступников, заказы киллера, случайные перестрелки и засады.|✅|
|**ИИ**|Поднята точность/здоровье/агрессия у мод-персонажей, союзники реально дерутся, а не убегают.|✅ (частично)|

**Ограничения (честно):** новые озвученные диалоги и катсцены невозможны без записи
аудио и правки анимаций — их в моде нет. «ИИ» улучшается только через доступные
скриптовые параметры, переписать поиск пути движка нельзя.

\---

## Управление в игре

|Клавиша|Действие|
|-|-|
|`F9`|Статус (Немезида, территории, контракты, счёт)|
|`F10`|Запустить атаку банд на вашу территорию|
|`F11`|Начать/продолжить основное задание|
|`F12`|Начать побочное задание|
|`NumPad1`|Открыть контракт «Розыск»|
|`NumPad2`|Открыть контракт «Заказ киллера»|
|`NumPad3`|Позвать союзника|

\---

## Требования

* **GTA IV Complete Edition** (версия 1.2.0.x).
* Установленный в игру инструментарий ScriptHookDotNet (см. ниже).
* Для сборки: **.NET SDK 6+** (проверено на 6.0.423) и, опционально, **NSIS**.

\---

## Разовый шаг: поставить ScriptHookDotNet в игру

Сам мод — это только `NemesisMod.net.dll`. Чтобы он работал, в игру нужно
поставить три вещи (они **не** входят в этот установщик, чтобы не нарушать чужие лицензии):

1. **ASI-загрузчик**, совместимый с Complete Edition (например, `aCompleteEditionHook.asi`
или обновлённый `dsound.dll`).
2. **Нативный ScriptHook.dll**.
3. **ScriptHookDotNet.asi + ScriptHookDotNet.dll** — для CE используйте форк **Tomasak**
(релиз «GTA4 .Net ScriptHook v1.7.1.8»; в описании релиза прямо указана поддержка
**1.2.0.59** и исправление «дождя в помещениях», в архив входит `aCompleteEditionHook.asi`):
[https://github.com/Tomasak/gta4\_scripthookdotnet/releases/download/release/scripthookdotnet\_v1.7.1.8.zip](https://github.com/Tomasak/gta4_scripthookdotnet/releases/download/release/scripthookdotnet_v1.7.1.8.zip)

Положите их в папку с `GTAIV.exe` (обычно `...\\\\\\\\Grand Theft Auto IV\\\\\\\\GTAIV\\\\\\\\`).
Проверка: после запуска игры в папке появится `ScriptHookDotNet.log`.

> Форк Tomasak поддерживает Complete Edition и чинит «баг с дождём в помещениях».
> Источники: \\\\\\\[тред ScriptHookDotNet на GTAForums](https://gtaforums.com/topic/946154-release-gtaiv-net-scripthook-v1718-support-for-gta-iv-1080-and-eflc-1130-by-arinc9-zolika1351/),
> \\\\\\\[форк Priler (CE) на NexusMods](https://www.nexusmods.com/gta4/mods/1217).

\---

## Сборка

```
1. Запустите build.ps1 — он сам скачает ScriptHookDotNet.dll (если его ещё нет).
2. (только без интернета) положите ScriptHookDotNet.dll (x86) в lib\\\\\\\\ — см. lib\\\\\\\\README.txt.
```

`build.ps1` делает:

* скачивает `ScriptHookDotNet.dll` (форк Tomasak для CE) в `lib\\\\\\\\`, если его ещё нет,
* `dotnet restore` + `dotnet build` (собирает `NemesisMod.net.dll`),
* копирует DLL и `NemesisMod.ini` в `dist\\\\\\\\scripts\\\\\\\\`,
* если установлен `makensis` — собирает `dist\\\\\\\\GTA4Nemesis\\\\\\\_Setup\\\\\\\_0.1.0.exe`.

Вручную (без build.ps1):

```
dotnet restore src\\\\\\\\NemesisMod\\\\\\\\NemesisMod.csproj
dotnet build  src\\\\\\\\NemesisMod\\\\\\\\NemesisMod.csproj -c Release
```

Результат: `src\\\\\\\\NemesisMod\\\\\\\\bin\\\\\\\\Release\\\\\\\\NemesisMod.net.dll`.

> Скрипты ScriptHookDotNet должны называться `\\\\\\\*.net.dll` и лежать в `scripts\\\\\\\\`
> внутри папки игры. Именно поэтому сборка называется `NemesisMod.net`.

### Установка NSIS (для сборки .exe)

Скачайте и установите [NSIS](https://nsis.sourceforge.io/Download), затем либо
добавьте `makensis.exe` в `PATH`, либо запускайте `build.ps1` — он найдёт его сам.
Без NSIS можно запустить `.\\\\\\\\install.ps1` (найдёт папку игры и скопирует файлы)
или вручную скопировать `dist\\\\\\\\scripts\\\\\\\\NemesisMod.net.dll` в папку игры.

### Сборка .exe без локального инструментария (GitHub Actions)

Если не хочется ставить .NET/NSIS локально — залейте папку проекта в репозиторий
GitHub: workflow `.github/workflows/build.yml` сам скачает ScriptHookDotNet, установит
NSIS, соберёт `.exe` и выложит его в артефакты Actions.

\---

## Установка

**Вариант A — установщик `.exe` (нужен NSIS):**

1. Соберите `GTA4Nemesis\\\\\\\_Setup\\\\\\\_0.1.0.exe` (см. «Сборка»).
2. Запустите, при необходимости укажите папку с `GTAIV.exe` — файлы попадут в `scripts\\\\\\\\`.

**Вариант B — без NSIS:**
Запустите `.\\\\\\\\install.ps1` — он сам найдёт папку игры (Steam / Rockstar Launcher)
и скопирует файлы в `scripts\\\\\\\\`.

В обоих случаях убедитесь, что в игру уже установлены ASI-загрузчик, `ScriptHook.dll`
и `ScriptHookDotNet.asi` (см. выше). Затем запустите игру.

Удаление (вариант A) — через «Установка и удаление программ» или `GTA4NemesisUninstall.exe`.

\---

## Сохранения

Прогресс (банды, члены, территории, контракты, счёт) хранится в файле
`state.txt` рядом с DLL (в `scripts\\\\\\\\`) либо в `Документы\\\\\\\\GTA4Nemesis\\\\\\\\state.txt`,
если запись в папку игры недоступна. Удаление файла сбрасывает мир.

\---

## Настройка

Параметры — в `scripts\\\\\\\\NemesisMod.ini` (см. `NemesisMod.ini.example`). Можно
включать/выключать модули, менять частоту засад/перестрелок и интервал автосохранения.

\---

## Структура проекта

```
gta4-nemesis/
├── build.ps1                     # сборка (сам качает ScriptHookDotNet.dll) + упаковка
├── install.ps1                   # установка без NSIS (находит папку игры)
├── README.md
├── .github\\\\\\\\workflows\\\\\\\\build.yml   # CI: собирает .exe на GitHub Actions
├── lib\\\\\\\\                          # сюда кладут ScriptHookDotNet.dll
├── installer\\\\\\\\GTA4Nemesis.nsi     # NSIS-инсталлятор
└── src\\\\\\\\NemesisMod\\\\\\\\
    ├── NemesisMod.csproj         # .NET Framework 4.0, сборка NemesisMod.net.dll
    ├── ModEntry.cs               # точка входа Script
    ├── NemesisMod.ini.example
    ├── Model\\\\\\\\                    # данные (банды, члены, территории, контракты)
    └── Core\\\\\\\\                     # системы (Nemesis, войны банд, активности, задания, ИИ)
```

\---

## План развития

* v0.1 — текущий фундамент (Nemesis, войны банд, активности, задания, ИИ).
* v0.2 — больше типов заданий и настройка баланса/наград, интерфейс территорий.
* v0.3+ — полировка, новые активности, тонкая настройка поведения ИИ.

## Примечание для разработчика

Код написан и **сверен с исходниками ScriptHookDotNet v1.7.1.8** (форк Tomasak):
имена классов и методов (`Game.LocalPlayer`, `RelationshipGroup.Player`,
`Relationship.Companion/Hate`, `Weapon.Handgun\\\\\\\_Glock/SMG\\\\\\\_MP5/Rifle\\\\\\\_AK47`,
`Blip.AddBlip`, `Ped.Task.FightAgainst/GoTo/FightAgainstHatedTargets`,
`BlipColor`, `Ped.Accuracy/MaxHealth/Enemy` и т.д.) проверены по заголовкам
`Ped.h`, `Player.h`, `World.h`, `Blip.h`, `vTasks.h`, `enums.h`, `vWeaponCollection.h`.
Все обращения к игре вынесены в `Core\\\\\\\\Helpers.cs` — при обновлении форка правка
нужна в одном месте. Смерть персонажей отслеживается опросом `isAlive` (у `Ped`
в этой версии нет событий), а не событиями.

