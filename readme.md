# MathExam – Mathematical Education Application

A desktop app for **Windows and Linux** (C# / Avalonia, .NET 8) for practising addition, subtraction, multiplication and division. Each task is an equation with one hidden number, which can be the result or one of the operands:

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
- **Progress history:** every game is saved on this computer and listed on the History screen with overall totals.
- **Accessibility:** three text sizes (Normal / Large / Extra large) and a high-contrast colour theme, both remembered between launches. Right and wrong answers are also shown as text (✓ / ✗), not just by colour.

## Quick start

Building requires the **.NET SDK 9.0.300 or newer** (see `global.json`). The app itself targets .NET 8, and the release builds include the runtime, so users don't need .NET installed.

**Windows:** double-click **`start.bat`**. On the first run it builds the release version, then it starts the app.

**Linux:** run `./start.sh`, which does the same.

| Windows | Linux | What it does |
|---|---|---|
| `build_release.bat` | `build_release.sh` | Runs the tests, then builds `release/win-x64/MathExam.exe` and `release/linux-x64/MathExam`. Each is a single self-contained file that runs **without installing .NET**. |
| `start.bat` | `start.sh` | Starts the release build for the current OS, building it first if it is missing. Run the build script again after changing the code. |
| `package_linux.bat` | `package_linux.sh` | Builds, then creates **`release/MathExam-linux-x64.tar.gz`**, containing the Linux program (marked executable) and a README. |

To share the app, send `release/win-x64/MathExam.exe` to Windows users and `MathExam-linux-x64.tar.gz` to Linux users. On Linux: `tar -xzf MathExam-linux-x64.tar.gz && ./MathExam/MathExam`.

During development:

```sh
dotnet run --project src/MathExam.App
dotnet test
```

## Project structure

| Path | Contents |
|---|---|
| `src/MathExam.Core` | Game logic: task generation, adaptive difficulty, session tracking, history and settings storage |
| `src/MathExam.App` | Cross-platform user interface (Avalonia, MVVM) |
| `tests/MathExam.Core.Tests` | xUnit tests for the core logic |
| `packaging/linux/` | README and file list for the Linux archive |
| `docs/` | Documentation |

## Documentation

- [User Guide](docs/user-guide.md): how to play, settings, and how tasks are generated
- [Developer Guide](docs/developer-guide.md): architecture, core types, generation rules, UI structure, builds, tests
