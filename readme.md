# MathExam – Mathematical Education Application

A desktop app for **Windows and Linux** (including the Raspberry Pi) (C# / Avalonia, .NET 8) for practising addition, subtraction, multiplication and division, plus powers and roots. Each task is an equation with one hidden number, which can be the result or one of the operands:

```
100 × 100 = ?
 99 ×   ? = 990
```

## Features

- **Main menu:** set the lowest and highest number and choose the operations (add, sub, mul, div, powers, roots). Practise with whole numbers, decimals or fractions, and optionally find the missing operator (`6 ? 3 = 18`).
- **Game:** shows the task number, solved count, start time and elapsed time.
  - Type the hidden number and press **Send** or <kbd>Enter</kbd>.
  - A correct answer is shown in **green**. A wrong answer is shown in **red**, together with the right answer.
  - Press **Next** or <kbd>Enter</kbd> again for the next random task.
- **Endless play:** keep going until you press **Stop**, then see a summary (correct, wrong, accuracy, time).
- **Show how:** after a wrong answer, the working is shown, e.g. `8 + 7 = 8 + 2 + 5 = 10 + 5 = 15`.
- **Timed challenge:** one minute against the clock, with a personal best for each set of settings.
- **Adaptive difficulty** (optional): starts with small numbers and widens the range as you answer correctly (10 levels). The next game resumes at your last level.
- **Family game:** 2–4 players take turns on one computer, each with their own adaptive level, so a child and a grandparent can play fairly together. Live scoreboard, final ranking, and "Play again".
- **Progress history:** every game is saved on this computer and listed on the History screen with overall totals and accuracy per operation. Export it as CSV for a spreadsheet.
- **Printable worksheets:** 20 tasks with the current settings as a PDF, with an answer key on page 2.
- **Languages:** English, French, German, Hungarian, Italian, Spanish, Polish, Czech, Finnish, Portuguese, Swedish, Norwegian, Croatian, Slovenian, Slovak, Turkish, Greek, Romanian and Dutch. The app follows the system language, or you can pick one in the menu.
- **Accessibility:** three text sizes (Normal / Large / Extra large) and a high-contrast colour theme, both remembered between launches. Right and wrong answers are also shown as text (✓ / ✗), not just by colour.

## Quick start

Building requires the **.NET SDK 9.0.300 or newer** (see `global.json`). The app itself targets .NET 8, and the release builds include the runtime, so users don't need .NET installed.

**Windows:** double-click **`start.bat`**. On the first run it builds the release version, then it starts the app.

**Linux:** run `./start.sh`, which does the same. It picks the build for the computer's processor: x64, 64-bit ARM or 32-bit ARM (e.g. a Raspberry Pi 3).

| Windows | Linux | What it does |
|---|---|---|
| `build_release.bat` | `build_release.sh` | Runs the tests, then builds `release/win-x64/MathExam.exe` and `release/linux-<arch>/MathExam` for `x64`, `arm64` and `arm` (32-bit). Each is a single self-contained file that runs **without installing .NET**. |
| `start.bat` | `start.sh` | Starts the release build for the current OS, building it first if it is missing. Run the build script again after changing the code. |
| `package_linux.bat` | `package_linux.sh` | Builds, then creates **`release/MathExam-linux-x64.tar.gz`**, **`-linux-arm64.tar.gz`** and **`-linux-arm.tar.gz`**, each containing the Linux program (marked executable) and a README. |

To share the app, send `release/win-x64/MathExam.exe` to Windows users and the matching Linux archive to Linux users: `x64` for most PCs, `arm64` for a Raspberry Pi with 64-bit Raspberry Pi OS, `arm` for 32-bit Raspberry Pi OS (`uname -m` shows `x86_64`, `aarch64` or `armv7l`). On Linux: `tar -xzf MathExam-linux-<arch>.tar.gz && ./MathExam/MathExam`.

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

## License

[MIT](license.md) © 2026 Istvan Schoffhauzer
