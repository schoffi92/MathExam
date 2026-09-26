# Developer Guide

## Requirements

- Windows or Linux (macOS should also work, since Avalonia supports it, but it is untested)
- **.NET SDK 9.0.300 or newer.** The projects target `net8.0`, but Avalonia 12's source generators need the C# compiler that ships with SDK 9.0.3xx. With SDK 8 the build fails with misleading `InitializeComponent does not exist` errors, so `global.json` requires 9.0.300 and rolls forward to any newer SDK.

## Build, test, run

```sh
dotnet build MathExam.sln
dotnet test
dotnet run --project src/MathExam.App
```

### Release builds

`build_release.bat` (Windows) and `build_release.sh` (Linux) do the same thing. They run `dotnet test -c Release`, then publish once per platform (runtime identifier) into `release/<rid>/`:

```
dotnet publish src/MathExam.App/MathExam.App.csproj -c Release -r <win-x64|linux-x64> --self-contained true
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
    -p:EnableCompressionInSingleFile=true -p:DebugType=none -o release/<rid>
```

- **Output:** `release/win-x64/MathExam.exe` and `release/linux-x64/MathExam`, each a compressed single file of about 45 MB that includes the .NET runtime. `IncludeNativeLibrariesForSelfExtract` bundles Avalonia's native libraries (SkiaSharp, HarfBuzz).
- **Cross-publishing:** either OS can build both platforms.
- **Debug symbols:** the `RemovePdbsFromPublish` target in `MathExam.App.csproj` deletes the ~100 MB of native `.pdb` files that SkiaSharp would otherwise leave next to the exe.
- **Failure handling:** a failing test stops the build before anything is published. `release/` is deleted first, so no stale files remain.
- **Pausing:** when double-clicked, the `.bat` scripts pause at the end so you can read the output. Pass `--no-pause` when calling one from another script.
- **Calling scripts:** the `.bat` scripts call each other by full path (`"%~dp0build_release.bat"`), because cmd does not search the current folder when `NoDefaultCurrentDirectoryInExePath` is set. The `.sh` scripts call each other with `bash`, so they work even if a checkout lost the execute bit.

`start.bat` / `start.sh` launch the release build for the current OS, and run the build script first if it is missing. They do not rebuild when the code changes.

### Linux archive

`package_linux.bat` / `package_linux.sh` run the build script, then create `release/MathExam-linux-x64.tar.gz`:

```
MathExam/MathExam     (mode 0755, the program)
MathExam/README.txt   (mode 0644, copied from packaging/linux/README.txt)
MathExam/LICENSE.md   (mode 0644, copied from license.md; MIT requires the notice in copies)
```

- **Permissions:** Windows files have no Unix permissions. `package_linux.bat` therefore passes Windows' built-in `tar.exe` (bsdtar) an mtree file list, `packaging/linux/files.mtree`, which states each file's mode. `package_linux.sh` stages the files with `install -m` and uses GNU tar. Both produce root-owned entries.
- **Line endings:** `.gitattributes` keeps `*.bat` in CRLF (cmd.exe can misread labels otherwise), and keeps `*.sh`, `*.mtree` and `packaging/linux/*` in LF.
- **Git:** `release/` is ignored by the `[Rr]elease/` rule in `.gitignore`.

### Testing on Linux without a desktop

Verified on Ubuntu 20.04 (WSL2): a Linux build and an extracted archive run under the Xvfb virtual display, driven with `xdotool` (Enter → Start, type an answer, Enter → Send). The Linux program needs the usual X11 libraries: `libX11`, `libICE`, `libSM` and `fontconfig`. Without a display it exits with `XOpenDisplay failed`.

## Solution layout

```
MathExam.sln
src/
  MathExam.Core/          Game logic, no UI dependencies (Resources/: validation messages)
  MathExam.App/           Avalonia front end (MVVM, CommunityToolkit.Mvvm); builds MathExam(.exe)
    Resources/            Strings.resx (English) + Strings.fr/de/hu.resx
    Themes/               Standard.axaml, HighContrast.axaml
    ViewModels/
    Views/
tests/
  MathExam.Core.Tests/    xUnit tests for MathExam.Core
packaging/linux/          README.txt and files.mtree for the Linux archive
docs/                     This documentation
global.json               Minimum .NET SDK (9.0.300)
build_release.bat/.sh     Test + publish self-contained builds into release/<rid>/
start.bat/.sh             Start the release build (builds it if missing)
package_linux.bat/.sh     Build + create release/MathExam-linux-x64.tar.gz
```

All the rules live in `MathExam.Core`, so they can be unit tested without a UI. The Avalonia project only binds that logic to the screen.

## MathExam.Core

| Type | Responsibility |
|---|---|
| `Operation` | `Add`, `Subtract`, `Multiply`, `Divide`. `Symbol()` returns `+ − × ÷`. |
| `HiddenPart` | Which number is hidden: `Left`, `Right` or `Result`. |
| `MathTask` | Immutable record `Left Op Right = Result` plus `Hidden`. `Answer` gives the hidden value, `IsCorrect(answer)` checks it, `ToDisplayString()` renders e.g. `99 × ? = 990`. |
| `GameSettings` | `Min`, `Max`, `Operations`. `Validate()` returns a user-facing error message in the current UI language, or `null` when the settings are valid. |
| `TaskGenerator` | `Next(settings)` creates a random `MathTask`. Accepts a `Random` for deterministic tests. |
| `GameSession` | One endless game. Keeps `CurrentTask`, `TaskNumber`, `CorrectCount`, `WrongCount`, `StartedAt` and `Elapsed` (a `Stopwatch`). `Submit(answer)` → `bool`, then `NextTask()`, then `Stop()`. With an optional `DifficultyAdjuster` it is adaptive: tasks come from `TaskSettings`, and `LastLevelChange` reports +1/−1/0 after each answer. |
| `DifficultyAdjuster` | Adaptive level 1–10. `RecordAnswer(correct)` → level change. `ForLevel(settings, level)` / `Apply(settings)` narrow the range. |
| `FamilyGame` | Turn-based game for 2–4 players. Each `FamilyPlayer` has their own adaptive `GameSession`, and all sessions share one start time. `Submit(answer)` answers for `Current`; `NextTurn()` prepares that player's next task and passes the turn on. Also provides `Round`, `Ranking` (correct answers, then accuracy), `Winners` (more than one means a tie; empty if nobody scored), and `ValidatePlayers(names)`. |
| `SessionRecord` | A finished game as stored in the history (range, operations, counts, duration, start/end level, and `Player`: the name in a family game, `null` for solo). `FromSession(session, player)` builds one. |
| `HistoryTotals` | Totals over the history. The records of one family game (same `StartedAt`, non-null `Player`) count as one game, and its time counts once. |
| `HistoryStore` | Reads and writes the history JSON file. `Load()`, `Add(record)`, `ResumeLevel(records, settings, player)`. `DefaultPath` is `<LocalApplicationData>/MathExam/history.json`, which is `%LOCALAPPDATA%` on Windows and `~/.local/share` on Linux. |
| `DisplayPreferences` / `PreferencesStore` | `TextSize` (`Normal`/`Large`/`ExtraLarge`), `HighContrast` and `Language` (`"en"`, `"fr"`, `"de"`, `"hu"`, or `null` for the system language), stored in `preferences.json`. Older files without `Language` load with `null`. `Load()` returns `null` when the file is missing or unreadable. |
| `JsonFile` (internal) | Shared JSON options, the app data folder, and `WriteAtomic` (temp file + move) used by both stores. |

### Task generation rules (`TaskGenerator`)

1. Pick a random enabled operation.
2. Generate operands within `[Min, Max]`:
   - **Add / Multiply:** `a`, `b` random; the result is computed.
   - **Subtract:** `a`, `b` random. If `Min >= 0`, they are swapped so that `a >= b` and the result is never negative.
   - **Divide:** built backwards to stay exact. Divisor `b ≠ 0` and quotient `q` are random, and the dividend is `b × q`.
3. Pick a random `HiddenPart`. If hiding that operand would allow any answer (`× 0`, `0 ÷ ?`), hide the `Result` instead.

Numbers are `long`, so multiplying two `int`-range operands cannot overflow.

### Adaptive difficulty (`DifficultyAdjuster`)

- **Level changes:** +1 after `CorrectStreakToLevelUp` (3) correct answers in a row, −1 after `WrongStreakToLevelDown` (2) wrong answers in a row. The level is clamped to `MinLevel`..`MaxLevel` (1..10), and each streak resets when the level changes or the other kind of answer arrives.
- **Range per level:** the range grows outwards from the *anchor*, `clamp(0, Min, Max)`, which is the easiest number. Each side covers `ceil(span × level / 10)` of its span, using integer maths so no floating-point rounding creeps in. Rounding up guarantees that a valid `GameSettings` stays valid on every level; for example, division always keeps a non-zero divisor.
- **Resuming:** `HistoryStore.ResumeLevel` returns the `EndLevel` of the latest adaptive record with the same `Min`/`Max` and the same player (`null` for solo; names compared case-insensitively), or level 1. Solo games and each family player therefore keep separate levels.

### Progress history (`HistoryStore`)

- **Format:** an indented JSON array of `SessionRecord`s. Enums are stored as names, and computed properties (`Answered`, `Accuracy`, `IsAdaptive`) are `[JsonIgnore]`d. Files written before family mode have no `Player` property; they load with `Player = null` (solo).
- **Safe writes:** `Add` writes to `history.json.tmp` and then moves it over the original, so a crash cannot leave a half-written file.
- **Corrupt files:** a file that can't be parsed is copied to `history.json.bak` and treated as empty, so the next save does not silently destroy it.
- **Error handling:** the app catches `IOException`/`UnauthorizedAccessException`. A failed save is shown on the summary screen, and a failed load is shown on the History screen. Neither crashes the app.

## MathExam.App (Avalonia / MVVM)

The UI uses [Avalonia](https://avaloniaui.net) 12 with the Fluent theme and the bundled Inter font, so it looks the same on every OS. `Program.cs` is the entry point. `App.axaml` holds the view templates, theme dictionaries and global styles.

Navigation is view-model first. `MainWindow` has a single `ContentControl` bound to `MainViewModel.CurrentViewModel`, and the `Application.DataTemplates` in `App.axaml` map each view model to its view. Views use compiled bindings (`x:DataType`), so a binding typo is a build error rather than a silent runtime failure.

```
MenuViewModel ──Start──▶ GameViewModel ──Stop──▶ SummaryViewModel ──Back──▶ MenuViewModel
      │                                        (game saved to history)
      ├──Family game──▶ FamilySetupViewModel ──Start──▶ FamilyGameViewModel ──Stop──▶ FamilySummaryViewModel
      │                                                  ▲   (one record per player saved)      │
      │                                                  └────────────── Play again ────────────┘
      └──History──▶ HistoryViewModel ──Back──▶ MenuViewModel
```

| View model | Notes |
|---|---|
| `DisplayViewModel` | Text size, high contrast and language. Loads preferences at startup (first launch follows the OS contrast preference, via `ThemeManager.SystemPrefersHighContrast()`), applies the theme and language, and saves on every change. Exposes `TextScale` (1.0 / 1.25 / 1.5), one bool per text-size radio button, and `Languages` / `Language` for the language list. |
| `MainViewModel` | Owns navigation, the `HistoryStore` and the shared `DisplayViewModel` (also exposed to the menu as `MenuViewModel.Display`). Reuses one `MenuViewModel`, so settings persist between games. Creates the `DifficultyAdjuster` at the resume level and saves each finished game that has answers. |
| `MenuViewModel` | Min/max are bound as strings, so invalid input can be reported instead of silently rejected. `ErrorMessage` and `StartCommand.CanExecute` reuse `GameSettings.Validate()`. |
| `GameViewModel` | Solo game. `State` (`Answering`/`Correct`/`Wrong`) drives the button text and read-only state. `IsCorrect`/`IsWrong` toggle the views' `correct`/`wrong` style classes. A single `SubmitOrNextCommand` handles both steps. An Avalonia `DispatcherTimer` refreshes `ElapsedText`. |
| `SummaryViewModel` | Read-only snapshot of the finished session, plus the level range and any history save error. |
| `HistoryViewModel` | Formats records into `HistoryRow`s (newest first) for a read-only `DataGrid`, and adds a totals line from `HistoryTotals`. |
| `FamilySetupViewModel` | 2–4 `PlayerNameEntry` rows (add/remove), validated with `FamilyGame.ValidatePlayers`. Reused, so names are kept; `Open(settings)` receives the menu settings each time. |
| `FamilyGameViewModel` | Like `GameViewModel`, but for the current player of a `FamilyGame`. Adds `TurnText`, `RoundText` and a `Scoreboard` of `ScoreRow`s (the current player gets the `current` style class). The button reads "Next player". |
| `FamilySummaryViewModel` | Winner or tie text, and `RankingRow`s where equal players share a place. **Play again** restarts with the same names and settings, loading resume levels fresh. |

`Answers` holds what both game screens share: answer parsing (which accepts `−` as a minus sign), the ✓/✗ feedback text and the level-change text. The green/red answer styles live in `App.axaml` for both screens.

### Keyboard handling

The Send/Next button has `IsDefault="True"`, so <kbd>Enter</kbd> anywhere in the window triggers it. It is also `Focusable="False"`, so keyboard focus stays in the answer box. The Start and Back buttons use `IsDefault` the same way.

### Themes and colours

- **Theme files:** every app colour comes from `Themes/Standard.axaml` or `Themes/HighContrast.axaml`. They define the same keys: `BackgroundBrush`, `ForegroundBrush`, `MutedBrush`, `AccentBrush`, `ErrorBrush`, `SuccessBrush`, `Correct*/Wrong*` brushes and border thicknesses. They also override Fluent's check box and radio button brushes (`CheckBox*`, `RadioButton*`), so those controls use the app's blue, or yellow with a black mark, instead of the OS accent colour.
- **Contrast targets:** Standard meets WCAG AA (4.5:1). High contrast meets AAA (7:1).
- **Switching:** Avalonia theme variants do the switching. `App.axaml` registers the two files as `ThemeDictionaries` for `Light` and for `ThemeManager.HighContrast`, a custom variant that inherits `Dark`, so any Fluent control not restyled here renders light-on-dark. `ThemeManager.Apply(highContrast)` sets `Application.RequestedThemeVariant`, and all styles and views use `DynamicResource`, so they update immediately. Don't add hard-coded colours to views; add a key to both theme files instead.
- **Control styles:** styles in `App.axaml`, declared after `<FluentTheme />`, restyle `Button` and `TextBox` in every state (`:pointerover`, `:pressed`, `:focus`, `:disabled`, targeting `/template/` parts). Fluent's own hover colours are unreadable on black.
- **Answer feedback:** the answer box and feedback text get `correct`/`wrong` style classes (`Classes.correct="{Binding IsCorrect}"`). The matching styles live in `GameView.axaml`, which is closer to the control than the app styles, so they override the focus border. `FeedbackText` repeats the result as "✓ Correct!" / "✗ Correct answer: N", so it doesn't depend on colour.
- **No GroupBox:** Avalonia has no `GroupBox`, so framed sections are a `Border` with the `group` class plus a `groupHeader` text block.

### Languages

- **Strings:** every UI text lives in `Resources/Strings.resx` (English, the fallback) with one `Strings.<lang>.resx` per translation (`fr`, `de`, `hu`). `MathExam.Core` has its own small set for the validation messages. The build generates a `Strings` class from each neutral file (`StronglyTyped*` metadata in the `.csproj`), so keys are checked at compile time. The app's class is public so views can use `{x:Static res:Strings.Key}`.
- **Formatting:** view models use `string.Format(Strings.Key, ...)`, so each language can put the numbers where its grammar wants them (`Task #{0}` → `{0}. feladat`). Plurals that differ get separate keys (`History_GameOne` / `History_GameMany`).
- **Switching:** `LanguageManager.Apply(code)` sets `CultureInfo.CurrentUICulture` (and the default for new threads); `null` restores the culture the app started with. Only the UI language changes; numbers, percentages and parsing keep following the OS regional settings (`CurrentCulture`).
- **Live change:** views read `{x:Static}` texts once, so when `DisplayViewModel.Language` changes, `MainWindow.RebuildScreen()` replaces the `ContentControl` and every view is created again. View models are kept, so entered settings survive. `FamilySetupViewModel.Open` relabels the player rows for the same reason.
- **Release builds:** the satellite assemblies (`fr/MathExam.resources.dll`, ...) are bundled into the single-file executable, so the packaging is unchanged.
- **Adding a language:** copy `Strings.resx` to `Strings.<code>.resx` in both projects, translate the values, and add the code to `LanguageManager.Options`. `TranslationTests` fails if a translation misses a key or changes a `{n}` placeholder.

### Text size

- **Scaling:** `MainWindow.axaml` wraps the content in a `LayoutTransformControl`. `MainWindow.FitToTextScale()` gives it a `ScaleTransform` of `Display.TextScale` whenever the setting changes. Every screen scales uniformly, so layouts keep their proportions.
- **Scrolling:** the content sits in a vertical `ScrollViewer`, so large text stays usable on small screens.
- **Window size:** the size and minimum size are set to the base size × scale, clamped to the screen's working area. This is skipped when the window is maximised.

## Tests

`tests/MathExam.Core.Tests` covers:

- arithmetic validity of generated tasks over several ranges, including negative ranges
- exact division and no division by zero
- only enabled operations appear; operands stay within range
- non-negative subtraction; every hidden part occurs; ambiguous zero cases are avoided
- `GameSettings` validation and `GameSession` counters and state
- `DifficultyAdjuster`: streaks, clamping, per-level ranges (including negative ranges and extreme `int` limits), and adaptive sessions staying within the current range
- `HistoryStore`: round trip, missing file, corrupt-file backup, resume level, and `SessionRecord.FromSession`. These tests use a temp directory.
- `PreferencesStore`: round trip, missing file, corrupt file, older files without a language
- translations: every `Strings.<lang>.resx` has exactly the English keys and the same `{n}` placeholders, and messages follow `CurrentUICulture`
- `FamilyGame`: turn order and wrap-around, rounds, independent per-player levels and scores, ranking, ties, no winner, name validation and trimming, per-player resume level, and the player name in records, including old history files without it
- `HistoryTotals`: a family game counts once with its time counted once; family players share the start time

The tests use fixed `Random` seeds, so every run gives the same results.

## Notes

- The default .NET `.gitignore` contains a macOS `*.app` rule. On Windows it also matches the `MathExam.App` folder, so the file ends with an explicit `!src/MathExam.App/` exception. Keep it if you regenerate the file.
- The app was originally WPF (Windows only). It moved to Avalonia to run on Linux. The view models and `MathExam.Core` were reused; the views, themes and window code were rewritten in Avalonia XAML (`.axaml`).
