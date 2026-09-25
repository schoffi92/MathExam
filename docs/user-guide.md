# User Guide

MathExam is a practice app for the four basic operations. It shows one equation at a time with a single number hidden, and you type in the missing number.

## 1. Main menu

| Setting | Meaning |
|---|---|
| **Lowest number** | Smallest value an operand can take. Negative numbers are allowed. |
| **Highest number** | Largest value an operand can take. |
| **Operations** | Any combination of addition (+), subtraction (−), multiplication (×) and division (÷). |
| **Adaptive difficulty** | Adjusts how big the numbers are to how well you are doing (see below). |
| **Display → Text size** | Normal, Large (125%) or Extra large (150%). Everything gets bigger and the window grows to match. |
| **Display → High contrast colours** | White and yellow on black, with thicker borders. |

Defaults: lowest `1`, highest `10`, all operations on, adaptive difficulty on, normal text. High contrast starts on if Windows' own high-contrast mode is on.

Display changes apply immediately and are remembered the next time you open the app. If Extra large text doesn't fit on a small screen, the window can be scrolled.

**Start** (or <kbd>Enter</kbd>) begins a game. **History** shows your past games. If the settings are invalid, the button stays disabled and a red message says what to fix:

- both limits must be whole numbers,
- the lowest number must not be greater than the highest,
- at least one operation must be selected,
- division needs at least one non-zero number in the range.

Your settings are kept when you return to the menu.

## 2. The game

The top bar shows:

| Field | Meaning |
|---|---|
| `Task #N` | Number of the current task |
| `Level L / 10` | Current difficulty level (adaptive games only) |
| `Solved: C / A` | Correct answers / answered tasks |
| `Started` | Time the game started |
| `Elapsed` | Time spent so far (hh:mm:ss) |

In the middle is the equation. The `?` can be the **result** or either **operand**:

```
7 × 8 = ?
7 × ? = 56
? ÷ 4 = 9
```

### Answering

1. Type the missing number. A leading minus sign is allowed.
2. Press <kbd>Enter</kbd> or click **Send**.
   - **Correct:** the answer box turns **green**, and "✓ Correct!" appears below it.
   - **Wrong:** the answer box turns **red**, and "✗ Correct answer: N" appears below it.
3. Press <kbd>Enter</kbd> again or click **Next** for a new random task.

An empty or non-numeric answer is ignored rather than counted as wrong.

### Ending the game

The game has no fixed length. Click **Stop** to end it and see the summary.

## 3. Adaptive difficulty

With adaptive difficulty on, the game has 10 levels.

- **Level 10** uses your full range. Lower levels use a smaller part of it, starting from the easiest number (the one closest to zero). For example, with a range of 1–10, level 1 uses 1–2, level 5 uses 1–6 and level 10 uses 1–10.
- **3 correct answers in a row** move you up one level: "▲ Level up!".
- **2 wrong answers in a row** move you down one level: "▼ Level down".
- The next game with the **same lowest and highest number** starts at the level you ended on. A new range starts at level 1.

With adaptive difficulty off, every task uses the full range.

## 4. Summary

Shows the number of answered, correct and wrong tasks, your accuracy, the total time and, for adaptive games, the start and end level (e.g. `Level: 2 → 5`). **Back to menu** (or <kbd>Enter</kbd>) returns to the main menu.

The game is saved to your progress history when you press **Stop**. A game with no answered tasks is not saved.

## 5. Progress history

The **History** button in the main menu shows every saved game, newest first: date, range, operations, answered, correct, accuracy, time and level (start → end). Above the table is a total across all games.

The history is stored only on this computer, in `%LOCALAPPDATA%\MathExam\history.json`. Delete that file to clear the history. Display settings are stored next to it in `preferences.json`.

## How tasks are generated

- Operands are always within your range, or within the current level's range in adaptive games. The result may be outside it; for example, with a range of 1–100 you can get `100 × 100 = ?`.
- Division always comes out even: the dividend is built as divisor × quotient, and both of those are within your range. You never divide by zero.
- Subtraction gives no negative results when the lowest number is 0 or more.
- A number that would make every answer correct, such as `0 × ? = 0`, is never hidden. The result is hidden instead.
