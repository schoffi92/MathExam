# User Guide

MathExam is a practice app for the four basic operations, plus powers, roots and metric unit conversion, with whole numbers, decimals or fractions. It shows one equation at a time with a single part hidden, and you type in what is missing.

## 1. Main menu

| Setting | Meaning |
|---|---|
| **Lowest number** | Smallest value an operand can take. Negative numbers are allowed. |
| **Highest number** | Largest value an operand can take. |
| **Operations** | Any combination of addition (+), subtraction (−), multiplication (×), division (÷), powers (xⁿ), roots (√) and **unit conversion** (e.g. `3 km = ? m`). |
| **Missing operator** | Some + − × ÷ tasks hide the operator instead of a number, e.g. `6 ? 3 = 18`. |
| **Numbers** | *Whole numbers*, *Decimals* (tenths, e.g. `1.7`) or *Fractions* (e.g. `3/4`, `1 1/2`). Fractions work with addition and subtraction only; decimals also with unit conversion (`2.5 m = ? cm`). |
| **Adaptive difficulty** | Adjusts how big the numbers are to how well you are doing (see below). |
| **Game** | *Practice* (no end), *Timed challenge (1 minute)* or *Test* of 10 or 20 tasks (see [Timed challenge](#timed-challenge) and [Tests](#tests)). Family games ignore it. |
| **Player** | Who plays solo games: *Guest* or a profile (see [Players and goals](#8-players-and-goals)). **Players…** adds and removes profiles. |
| **Display → Text size** | Normal, Large (125%) or Extra large (150%). Everything gets bigger and the window grows to match. |
| **Display → High contrast colours** | White and yellow on black, with thicker borders. |
| **Display → Language** | English, Français (French), Deutsch (German), Magyar (Hungarian), Italiano (Italian), Español (Spanish), Polski (Polish), Čeština (Czech), Suomi (Finnish), Português (Portuguese), Svenska (Swedish), Norsk bokmål (Norwegian), Hrvatski (Croatian), Slovenščina (Slovenian), Slovenčina (Slovak), Türkçe (Turkish), Ελληνικά (Greek), Română (Romanian) or Nederlands (Dutch). *System default* uses your operating system's language, or English if it is none of these. |

Defaults: lowest `1`, highest `10`, the four basic operations on (powers, roots and unit conversion off), whole numbers, no missing operators, adaptive difficulty on, *Practice*, the *Guest* player, normal text, system language. High contrast starts on if the operating system's high-contrast mode is on.

Display changes, including the language, apply immediately and are remembered the next time you open the app. If Extra large text doesn't fit on a small screen, the window can be scrolled.

The menu has two columns: what to practise on the left; the player and the display settings on the right. **How to use** at the top right opens a short guide to the app in the chosen language; scroll through it and press **Close** (or <kbd>Esc</kbd>) to return to the menu, with your settings as they were. The guide has its own **Language** list: it is the same setting as on the menu, so choosing a language there switches the guide and the whole app.

**Start** (or <kbd>Enter</kbd>) begins a game. **Family game** sets up a game for several players (see [Family game](#5-family-game)). **History** shows your past games. **Worksheet (PDF)** opens the options for a printable worksheet (see [Worksheets](#7-printable-worksheets)). If the settings are invalid, the button stays disabled and a red message says what to fix:

- both limits must be whole numbers,
- the lowest number must not be greater than the highest,
- at least one operation must be selected,
- fractions need only addition and/or subtraction to be selected; decimals may also use unit conversion,
- unit conversion needs at least one number of 1 or more in the range,
- division needs at least one non-zero number in the range,
- powers and roots need at least one number between −1000 and 1000 in the range.

Your settings are kept when you return to the menu.

## 2. The game

The top bar shows:

| Field | Meaning |
|---|---|
| `Task #N` | Number of the current task; in a test `Task N of 20` |
| `Level L / 10` | Current difficulty level (adaptive games only) |
| `Solved: C / A` | Correct answers / answered tasks (hidden during a test) |
| Player and goal | The chosen player's name and, with a daily goal, `Today: 12 / 20 tasks` |
| `Started` | Time the game started |
| `Elapsed` | Time spent so far (hh:mm:ss); in a timed challenge, `Time left` counts down instead |

In the middle is the equation. The `?` can be the **result** or either **operand**:

```
7 × 8 = ?
7 × ? = 56
? ÷ 4 = 9
?² = 49
³√? = 4
```

With powers, the `?` can also be the exponent (`2^? = 8`, shown raised); with roots, the degree (`?√27 = 3`). For `?² = 49`, both 7 and −7 count as correct.

With **Missing operator** on, some tasks ask for the sign: `6 ? 3 = 18`. Click one of the **+ − × ÷** buttons below the answer box, or type `+`, `-`, `*` (or `x`) or `/` (or `:`) and press <kbd>Enter</kbd>. Any sign that makes the equation true counts, so both `+` and `×` are right for `2 ? 2 = 4`.

With **Unit conversion**, the `?` is one of the two amounts: `3 km = ? m` or `? kg = 4000 g`. Type just the number (`3000`), without the unit.

With **Decimals** or **Fractions**, type the answer as `1.7` or `1,7`, `3/4`, or a mixed number like `1 1/2`. Any equal value counts: for `1/2` you may also type `2/4` or `0.5`.

### Answering

1. Type the missing number. A leading minus sign is allowed.
2. Press <kbd>Enter</kbd> or click **Send**.
   - **Correct:** the answer box turns **green**, and "✓ Correct!" appears below it.
   - **Wrong:** the answer box turns **red**, and "✗ Correct answer: N" appears below it.
3. Press <kbd>Enter</kbd> again or click **Next** for a new random task.

An empty or non-numeric answer is ignored rather than counted as wrong.

### Show how

After a wrong answer, a line below the correct answer shows how to work it out, for example:

| Task | How |
|---|---|
| `8 + 7 = ?` | `8 + 7 = 8 + 2 + 5 = 10 + 5 = 15` (make ten first) |
| `52 − 17 = ?` | `52 − 17 = 52 − 10 − 7 = 42 − 7 = 35` |
| `12 × 7 = ?` | `12 × 7 = 10 × 7 + 2 × 7 = 70 + 14 = 84` |
| `56 ÷ 8 = ?` | `7 × 8 = 56 → 56 ÷ 8 = 7` |
| `? + 8 = 15` | `15 − 8 = 7` (undo the operation) |
| `3⁴ = ?` | `3⁴ = 3 × 3 × 3 × 3 = 81` |
| `3000 g = ? kg` | `1 kg = 1000 g → 3000 ÷ 1000 = 3` |
| `1/2 + 1/4 = ?` | `1/2 + 1/4 = 2/4 + 1/4 = 3/4` |

Tasks that are already as simple as they get, like `2 + 3`, show no extra line.

### Ending the game

In *Practice* the game has no fixed length: click **Stop** to end it and see the summary. A timed challenge ends when the time is up, and a test after its last task; **Stop** ends them early.

### Timed challenge

With **Game: Timed challenge (1 minute)**, **Start** begins a one-minute game. The top bar shows the time left, and the game ends by itself when it runs out. The summary then says "Time's up!" and compares your correct answers with your **personal best**: the best earlier timed game with exactly the same range, operations and options, by the same player. Beat it and you see "New personal best!". The challenge is for solo games; family games ignore it.

### Tests

With **Game: Test (10 tasks)** or **Test (20 tasks)**, **Start** begins a short test. It uses your whole range (adaptive levels are left out, so every test is comparable) and shows `Task 3 of 20` at the top. There is no right/wrong feedback, explanation or score during the test: after each answer the next task comes straight away. After the last task (or **Stop**), the summary shows the **score**, e.g. `Score: 17 / 20`, and **To practise:** the tasks answered wrong, solved, with the answer that was given, e.g. `7 × 8 = 56   (you answered 54)`.

## 3. Adaptive difficulty

With adaptive difficulty on, the game has 10 levels.

- **Level 10** uses your full range. Lower levels use a smaller part of it, starting from the easiest number (the one closest to zero). For example, with a range of 1–10, level 1 uses 1–2, level 5 uses 1–6 and level 10 uses 1–10.
- **3 correct answers in a row** move you up one level: "▲ Level up!".
- **2 wrong answers in a row** move you down one level: "▼ Level down".
- The next game by the **same player** with the **same lowest and highest number** and the same kind of numbers starts at the level you ended on. A new range starts at level 1.
- Tests ignore adaptive difficulty and always use the full range.

With adaptive difficulty off, every task uses the full range.

## 4. Summary

Shows the number of answered, correct and wrong tasks, your accuracy, the total time and, for adaptive games, the start and end level (e.g. `Level: 2 → 5`). After a timed challenge it also shows your personal best; after a test, the score and the tasks to practise (see [Tests](#tests)). **Back to menu** (or <kbd>Enter</kbd>) returns to the main menu.

The game is saved to your progress history when it ends: when you press **Stop**, the time runs out, or a test is complete. It is saved under the chosen player. A game with no answered tasks is not saved.

## 5. Family game

Two to four people take turns on one computer, for example a grandchild and a grandparent.

1. Choose the numbers and operations in the main menu, then click **Family game**.
2. Type a name for each player. **+ Add player** adds a player (up to 4), and **Remove** takes one away (at least 2 must stay). Names must be different. The names are kept for the next family game.
3. Choose **Tasks per turn** (1–10, default 1): how many tasks each player answers in a row before the next player's turn.
4. Click **Start** (or press <kbd>Enter</kbd>).

During the game:

- The big yellow or blue line says **whose turn it is**, and that player's box in the scoreboard at the top is framed.
- The player answers their tasks as usual (<kbd>Enter</kbd> to send, green or red feedback). With more than one task per turn, "Task 2 of 3" shows where they are, and **Next** (or <kbd>Enter</kbd>) brings their next task. After their last task, **Next player** passes the turn on.
- The scoreboard shows each player's correct answers and level. **Round N** counts how many times everyone has had a turn.

**Everyone has their own difficulty level.** Family games are always adaptive: each player moves up and down separately (3 right in a row goes up, 2 wrong in a row goes down) and starts at the level they reached in their last family game with the same numbers. A child and an adult each get tasks that suit them, so the game stays fair.

**Stop** ends the game and shows the ranking. The player with the most correct answers wins; equal scores are decided by accuracy, and completely equal players share a place. **Play again** starts a new round with the same players and tasks per turn, and **Back to menu** returns to the menu. Each player's result is saved to the history under their name.

## 6. Progress history

The **History** button in the main menu has two tabs.

**Games** shows every saved result, newest first: date, player (*Guest* for solo games without a profile), game (practice, timed, test or family), tasks (range and operations), correct / answered, accuracy, time and level (start → end). A family game adds one row per player. **Show:** limits the list, the totals and the per-operation line to one player; a player's family games count too.

**Players** has one row per player, as an overview for a parent or a teacher: games, tasks, accuracy, total time, tasks today (against the daily goal, e.g. `12 / 20`) and when they last played.

Above the table is a total across the games shown. A family game counts once, and its time is counted once. Below it, **By operation** shows your accuracy for each operation (e.g. `+ 95 %  ·  × 70 %`), so you can see what to practise. It covers games played since this was added; older games only count in the totals.

In the Tasks column, `(1/2)` marks a fraction game, `(0.1)` a decimal game and `(?)` a game with missing operators; `↔` stands for unit conversion.

**Export (CSV)** saves what the Games tab shows (all players, or the chosen one) as a CSV file for a spreadsheet: one row per result, with the date, player, game mode, settings, counts, accuracy, time, levels, time limit and the correct/wrong counts per operation. The file uses English column names, `yyyy-MM-dd HH:mm:ss` dates and `.` as the decimal point, whatever the app's language.

The history is stored only on this computer, in `history.json`. Delete that file to clear the history. Display settings are stored next to it in `preferences.json`, and the players in `profiles.json`. The folder is:

- **Windows:** `%LOCALAPPDATA%\MathExam\`
- **Linux:** `~/.local/share/MathExam/`

## 7. Printable worksheets

**Worksheet (PDF)** in the main menu opens the worksheet options, which use the current menu settings (the full range, without adaptive levels):

| Option | Meaning |
|---|---|
| **Tasks per sheet** | 10, 20 or 30. |
| **Versions** | 1 to 4 different sheets (A, B, C, D) with the same settings, e.g. so neighbours in class get different tasks. |
| **Sheet code** | Any number up to 999999. The same code, settings and number of tasks always give the same tasks, so every pupil gets the same sheet (a later app version may make different tasks from the same code). Leave it empty for new tasks; the sheet then shows the code it used, so it can be made again. |

**Save PDF…** asks where to save, then writes an A4 PDF:

- **One task page per version:** the title, the settings, the version and code, lines for a name and the date, and the tasks in two columns. A line marks each missing number, a box each missing operator, and a small raised box a missing exponent or root degree.
- **Then one answer key per version,** with every task written out in full, so the task pages can be printed for the class and the keys kept apart.

## 8. Players and goals

**Players…** next to the player list in the main menu opens the Players screen:

- Type a name and click **Add** (or press <kbd>Enter</kbd>). Names must be different.
- Set each player's **daily goal** in tasks (0 means no goal).
- **Remove** takes a player off the list. Their games stay in the history.

Choose the player in the menu before pressing **Start**. Solo games are then saved under that player, who has their own adaptive levels and personal bests. *Guest* plays without a profile; games from before profiles existed belong to the guest.

With a daily goal, the menu and the game screen show the progress, e.g. `Today: 12 / 20 tasks`, and then `✓ Daily goal reached!`. Every answered task counts, including family games played under the same name.

For a class, add each pupil as a player and let them pick their name before playing; the History screen's **Players** tab and **Export (CSV)** then give the teacher an overview of everyone.

## How tasks are generated

- Operands are always within your range, or within the current level's range in adaptive games. The result may be outside it; for example, with a range of 1–100 you can get `100 × 100 = ?`.
- Division always comes out even: the dividend is built as divisor × quotient, and both of those are within your range. You never divide by zero.
- Subtraction gives no negative results when the lowest number is 0 or more.
- A number that would make every answer correct, such as `0 × ? = 0` or `1^? = 1`, is never hidden. The result is hidden instead.
- **Powers:** the base comes from your range (at most ±1000) and the exponent is 2 to 5. Exponents above 2 are only used while the result stays within ±1000, so you get `12²` but not `12³`, and `4⁴` but not `6⁴`.
- **Roots:** built backwards from the answer, so they always come out even: `√49 = 7` comes from `7²`. Square roots are written without the small 2. A negative number only gets an odd root, such as `³√−8 = −2`.
- **Decimals:** both numbers are tenths within your range, e.g. `1.2 + 0.5`.
- **Fractions:** the denominators are 2, 3, 4, 5, 6, 8, 10 or 12, and each number lies within your range, so a range of `0`–`1` gives fractions below one and `0`–`3` also mixed numbers like `2 1/4`. Half of the tasks use one denominator (`1/4 + 2/4`), the other half two different ones (`1/2 + 1/3`). At least one number in each task is a real fraction.
- **Missing operator:** about one in four + − × ÷ tasks hides the operator.
- **Unit conversion:** length (mm, cm, dm, m, km), mass (mg, g, kg, t) or capacity (mL, cL, dL, L, hL), always between two units at most three steps apart (×10, ×100 or ×1000), e.g. m and cm, or kg and g. The amount in the larger unit comes from your range (only its numbers of 1 or more), so `1–10` gives tasks like `7 km = ? m` or `7000 m = ? km`. With *Decimals* that amount can have tenths: `2.5 m = ? cm`, `250 cm = ? m`. Units use their international symbols in every language.
