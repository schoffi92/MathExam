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
| `GameSession` | One endless game. Keeps `CurrentTask`, `TaskNumber`, `CorrectCount`, `WrongCount`, `StartedAt` and `Elapsed` (a `Stopwatch`). `Submit(answer)` → `bool`, then `NextTask()`, then `Stop()`. |

### Task generation rules (`TaskGenerator`)

1. Pick a random enabled operation.
2. Generate operands within `[Min, Max]`:
   - **Add / Multiply:** `a`, `b` random; the result is computed.
   - **Subtract:** `a`, `b` random. If `Min >= 0`, they are swapped so that `a >= b` and the result is never negative.
   - **Divide:** built backwards to stay exact. Divisor `b ≠ 0` and quotient `q` are random, and the dividend is `b × q`.
3. Pick a random `HiddenPart`. If hiding that operand would allow any answer (`× 0`, `0 ÷ ?`), hide the `Result` instead.

Numbers are `long`, so multiplying two `int`-range operands cannot overflow.

## MathExam.App (WPF / MVVM)

Navigation is view-model first. `MainWindow` has a single `ContentControl` bound to `MainViewModel.CurrentViewModel`, and implicit `DataTemplate`s in `App.xaml` map each view model to its view.

```
MenuViewModel ──Start──▶ GameViewModel ──Stop──▶ SummaryViewModel ──Back──▶ MenuViewModel
```

| View model | Notes |
|---|---|
| `MainViewModel` | Owns navigation. Reuses one `MenuViewModel`, so settings persist between games. |
| `MenuViewModel` | Min/max are bound as strings, so invalid input can be reported instead of silently rejected. `ErrorMessage` and `StartCommand.CanExecute` reuse `GameSettings.Validate()`. |
| `GameViewModel` | `State` (`Answering`/`Correct`/`Wrong`) drives the button text, read-only state and colours. A single `SubmitOrNextCommand` handles both steps. A `DispatcherTimer` refreshes `ElapsedText`. |
| `SummaryViewModel` | Read-only snapshot of the finished session. |

### Keyboard handling

The Send/Next button has `IsDefault="True"`, so <kbd>Enter</kbd> anywhere in the window triggers it. It is also `Focusable="False"`, so keyboard focus stays in the answer box. The Start and Back buttons use `IsDefault` the same way.

### Feedback colours

`GameView.xaml` has `DataTrigger`s on `State` that set the answer box's background, foreground and border to green or red.

## Tests

`tests/MathExam.Core.Tests` covers:

- arithmetic validity of generated tasks over several ranges, including negative ranges
- exact division and no division by zero
- only enabled operations appear; operands stay within range
- non-negative subtraction; every hidden part occurs; ambiguous zero cases are avoided
- `GameSettings` validation and `GameSession` counters and state

The tests use fixed `Random` seeds, so every run gives the same results.

## Notes

- The default .NET `.gitignore` contains a macOS `*.app` rule. On Windows it also matches the `MathExam.App` folder, so the file ends with an explicit `!src/MathExam.App/` exception. Keep it if you regenerate the file.
