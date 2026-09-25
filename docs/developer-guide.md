# Developer Guide

## Requirements

- Windows (WPF)
- .NET SDK 8 or newer (the projects target `net8.0` / `net8.0-windows`)

## Build, test, run

```powershell
dotnet build MathExam.sln
dotnet test
dotnet run --project src/MathExam.App
```

## Solution layout

```
MathExam.sln
src/
  MathExam.Core/          Game logic, no UI dependencies
  MathExam.App/           WPF front end (MVVM, CommunityToolkit.Mvvm)
    ViewModels/
    Views/
tests/
  MathExam.Core.Tests/    xUnit tests for MathExam.Core
docs/                     This documentation
```

All the rules live in `MathExam.Core`, so they can be unit tested without a UI. The WPF project only binds that logic to the screen.

## MathExam.Core

| Type | Responsibility |
|---|---|
| `Operation` | `Add`, `Subtract`, `Multiply`, `Divide`. `Symbol()` returns `+ − × ÷`. |
| `HiddenPart` | Which number is hidden: `Left`, `Right` or `Result`. |
| `MathTask` | Immutable record `Left Op Right = Result` plus `Hidden`. `Answer` gives the hidden value, `IsCorrect(answer)` checks it, `ToDisplayString()` renders e.g. `99 × ? = 990`. |
| `GameSettings` | `Min`, `Max`, `Operations`. `Validate()` returns a user-facing error message, or `null` when the settings are valid. |
| `TaskGenerator` | `Next(settings)` creates a random `MathTask`. Accepts a `Random` for deterministic tests. |
| `GameSession` | One endless game. Keeps `CurrentTask`, `TaskNumber`, `CorrectCount`, `WrongCount`, `StartedAt` and `Elapsed` (a `Stopwatch`). `Submit(answer)` → `bool`, then `NextTask()`, then `Stop()`. With an optional `DifficultyAdjuster` it is adaptive: tasks come from `TaskSettings`, and `LastLevelChange` reports +1/−1/0 after each answer. |
| `DifficultyAdjuster` | Adaptive level 1–10. `RecordAnswer(correct)` → level change. `ForLevel(settings, level)` / `Apply(settings)` narrow the range. |
| `SessionRecord` | A finished game as stored in the history (range, operations, counts, duration, start/end level). `FromSession(session)` builds one. |
| `HistoryStore` | Reads and writes the history JSON file. `Load()`, `Add(record)`, `ResumeLevel(records, settings)`. `DefaultPath` is `%LOCALAPPDATA%\MathExam\history.json`. |
| `DisplayPreferences` / `PreferencesStore` | `TextSize` (`Normal`/`Large`/`ExtraLarge`) and `HighContrast`, stored in `preferences.json`. `Load()` returns `null` when the file is missing or unreadable. |
| `JsonFile` (internal) | Shared JSON options, the `%LOCALAPPDATA%\MathExam` folder, and `WriteAtomic` (temp file + move) used by both stores. |

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
- **Resuming:** `HistoryStore.ResumeLevel` returns the `EndLevel` of the latest adaptive record with the same `Min`/`Max`, or level 1.

### Progress history (`HistoryStore`)

- **Format:** an indented JSON array of `SessionRecord`s. Enums are stored as names, and computed properties (`Answered`, `Accuracy`, `IsAdaptive`) are `[JsonIgnore]`d.
- **Safe writes:** `Add` writes to `history.json.tmp` and then moves it over the original, so a crash cannot leave a half-written file.
- **Corrupt files:** a file that can't be parsed is copied to `history.json.bak` and treated as empty, so the next save does not silently destroy it.
- **Error handling:** the app catches `IOException`/`UnauthorizedAccessException`. A failed save is shown on the summary screen, and a failed load is shown on the History screen. Neither crashes the app.

## MathExam.App (WPF / MVVM)

Navigation is view-model first. `MainWindow` has a single `ContentControl` bound to `MainViewModel.CurrentViewModel`, and implicit `DataTemplate`s in `App.xaml` map each view model to its view.

```
MenuViewModel ──Start──▶ GameViewModel ──Stop──▶ SummaryViewModel ──Back──▶ MenuViewModel
      │                                        (game saved to history)
      └──History──▶ HistoryViewModel ──Back──▶ MenuViewModel
```

| View model | Notes |
|---|---|
| `DisplayViewModel` | Text size and high contrast. Loads preferences at startup (first launch follows `SystemParameters.HighContrast`), applies the theme, and saves on every change. Exposes `TextScale` (1.0 / 1.25 / 1.5) and one bool per text-size radio button. |
| `MainViewModel` | Owns navigation, the `HistoryStore` and the shared `DisplayViewModel` (also exposed to the menu as `MenuViewModel.Display`). Reuses one `MenuViewModel`, so settings persist between games. Creates the `DifficultyAdjuster` at the resume level and saves each finished game that has answers. |
| `MenuViewModel` | Min/max are bound as strings, so invalid input can be reported instead of silently rejected. `ErrorMessage` and `StartCommand.CanExecute` reuse `GameSettings.Validate()`. |
| `GameViewModel` | `State` (`Answering`/`Correct`/`Wrong`) drives the button text, read-only state and colours. A single `SubmitOrNextCommand` handles both steps. A `DispatcherTimer` refreshes `ElapsedText`. |
| `SummaryViewModel` | Read-only snapshot of the finished session, plus the level range and any history save error. |
| `HistoryViewModel` | Formats records into `HistoryRow`s (newest first) for a read-only `DataGrid`, and adds a totals line. |

### Keyboard handling

The Send/Next button has `IsDefault="True"`, so <kbd>Enter</kbd> anywhere in the window triggers it. It is also `Focusable="False"`, so keyboard focus stays in the answer box. The Start and Back buttons use `IsDefault` the same way.

### Themes and colours

- **Theme files:** every colour comes from a theme dictionary, `Themes/Standard.xaml` or `Themes/HighContrast.xaml`. They define the same keys: `BackgroundBrush`, `ForegroundBrush`, `MutedBrush`, `AccentBrush`, `ErrorBrush`, `SuccessBrush`, `Correct*/Wrong*` brushes, border thicknesses, and the `SystemColors` selection keys used by the `DataGrid`.
- **Contrast targets:** Standard meets WCAG AA (4.5:1). High contrast meets AAA (7:1).
- **Switching:** `App.xaml` merges the Standard dictionary. `ThemeManager.Apply(highContrast)` replaces it at runtime, and all styles and views use `DynamicResource`, so they update immediately. Don't add hard-coded colours to views; add a key to both theme files instead.
- **Custom templates:** `App.xaml` gives `Button` and `TextBox` their own simple templates. The system (Aero2) templates hard-code light-blue hover/focus colours, which are unreadable on black and would hide the answer feedback border. Check boxes and radio buttons only get a themed text colour, because their glyphs are drawn dark on a light box.
- **Answer feedback:** `GameView.xaml` has `DataTrigger`s on `State` that set the answer box's background, foreground, border and border thickness. This style is `BasedOn` the global `TextBox` style, so it takes priority over the focus border. `FeedbackText` repeats the result as "✓ Correct!" / "✗ Correct answer: N", so it doesn't depend on colour.

### Text size

- **Scaling:** `MainWindow.xaml` puts a `ScaleTransform` bound to `Display.TextScale` in the content's `LayoutTransform`. Every screen scales uniformly, so layouts keep their proportions.
- **Scrolling:** the content sits in a vertical `ScrollViewer`, so large text stays usable on small screens.
- **Window size:** `MainWindow.FitToTextScale()` sets the size and minimum size to the base size × scale, clamped to the screen's work area. It skips this when the window is maximised.

## Tests

`tests/MathExam.Core.Tests` covers:

- arithmetic validity of generated tasks over several ranges, including negative ranges
- exact division and no division by zero
- only enabled operations appear; operands stay within range
- non-negative subtraction; every hidden part occurs; ambiguous zero cases are avoided
- `GameSettings` validation and `GameSession` counters and state
- `DifficultyAdjuster`: streaks, clamping, per-level ranges (including negative ranges and extreme `int` limits), and adaptive sessions staying within the current range
- `HistoryStore`: round trip, missing file, corrupt-file backup, resume level, and `SessionRecord.FromSession`. These tests use a temp directory.
- `PreferencesStore`: round trip, missing file, corrupt file

The tests use fixed `Random` seeds, so every run gives the same results.

## Notes

- The default .NET `.gitignore` contains a macOS `*.app` rule. On Windows it also matches the `MathExam.App` folder, so the file ends with an explicit `!src/MathExam.App/` exception. Keep it if you regenerate the file.
