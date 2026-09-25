# MathExam – Mathematical Education Application

A Windows desktop app (C# / WPF, .NET 8) for practising addition, subtraction, multiplication and division. Each task is an equation with one hidden number, which can be the result or one of the operands:

```
100 × 100 = ?
 99 ×   ? = 990
```

## Features

- **Main menu:** set the lowest and highest number and choose the operations (add, sub, mul, div).
- **Game:** shows the task number, solved count, start time and elapsed time.
  - Type the hidden number and press **Send** or <kbd>Enter</kbd>.
  - A correct answer is shown in **green**. A wrong answer is shown in **red**, together with the right answer.
  - Press **Next** or <kbd>Enter</kbd> again for the next random task.
- **Endless play:** keep going until you press **Stop**, then see a summary (correct, wrong, accuracy, time).
- **Adaptive difficulty** (optional): starts with small numbers and widens the range as you answer correctly (10 levels). The next game resumes at your last level.
- **Progress history:** every game is saved on this PC and listed on the History screen with overall totals.
- **Accessibility:** three text sizes (Normal / Large / Extra large) and a high-contrast colour theme, both remembered between launches. Right and wrong answers are also shown as text (✓ / ✗), not just by colour.

## Quick start

Requires Windows and the .NET 8 SDK (or newer).

```powershell
dotnet run --project src/MathExam.App
```

Run the tests:

```powershell
dotnet test
```

## Project structure

| Path | Contents |
|---|---|
| `src/MathExam.Core` | Game logic: task generation, adaptive difficulty, session tracking, history storage |
| `src/MathExam.App` | WPF user interface (MVVM) |
| `tests/MathExam.Core.Tests` | xUnit tests for the core logic |
| `docs/` | Documentation |

## Documentation

- [User Guide](docs/user-guide.md): how to play, settings, and how tasks are generated
- [Developer Guide](docs/developer-guide.md): architecture, core types, generation rules, UI structure, tests
