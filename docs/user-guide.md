# User Guide

MathExam is a practice app for the four basic operations. It shows one equation at a time with a single number hidden, and you type in the missing number.

## 1. Main menu

| Setting | Meaning |
|---|---|
| **Lowest number** | Smallest value an operand can take. Negative numbers are allowed. |
| **Highest number** | Largest value an operand can take. |
| **Operations** | Any combination of addition (+), subtraction (−), multiplication (×) and division (÷). |

Defaults: lowest `1`, highest `10`, all operations on.

**Start** (or <kbd>Enter</kbd>) begins a game. If the settings are invalid, the button stays disabled and a red message says what to fix:

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
   - **Correct:** the answer box turns **green**.
   - **Wrong:** the answer box turns **red**, and the correct answer is shown below it.
3. Press <kbd>Enter</kbd> again or click **Next** for a new random task.

An empty or non-numeric answer is ignored rather than counted as wrong.

### Ending the game

The game has no fixed length. Click **Stop** to end it and see the summary.

## 3. Summary

Shows the number of answered, correct and wrong tasks, your accuracy and the total time. **Back to menu** (or <kbd>Enter</kbd>) returns to the main menu.

## How tasks are generated

- Operands are always within your range. The result may be outside it; for example, with a range of 1–100 you can get `100 × 100 = ?`.
- Division always comes out even: the dividend is built as divisor × quotient, and both of those are within your range. You never divide by zero.
- Subtraction gives no negative results when the lowest number is 0 or more.
- A number that would make every answer correct, such as `0 × ? = 0`, is never hidden. The result is hidden instead.
