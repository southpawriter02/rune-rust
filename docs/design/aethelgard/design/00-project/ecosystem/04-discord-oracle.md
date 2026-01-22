# Proposal: Discord Oracle (`discord-oracle`)

**Type**: External Tool / Community Bot
**Target Audience**: Community Members

## 1. Vision
The **Discord Oracle** is a bot that lives in your community Discord server. It bridges the gap between the game data and social discussion.

## 2. Key Features

### 2.1 The Rules Lawyer
*   **Command**: `/lookup [term]`
*   **Response**: Fetches the exact text from the Game Data / Wiki.
*   *Example*: `/lookup "Fireball"` -> Returns damage formula, cost, and tags.

### 2.2 Dice Roller
*   **Command**: `/roll [expression]`
*   **Response**: Simulation of the game's specific dice mechanics (e.g., exploding 6s, if applicable).

### 2.3 Daily Challenge
*   **Command**: `/daily`
*   **Response**: "Today's Mutator is: [Low Gravity]. Best score: User123 (500 pts)."
*   *Integration*: Queries `rune-gate-api`.

## 3. Technical Specifications

### 3.1 Tech Stack
*   **Language**: **Python** (discord.py) or **C#** (Discord.Net).
    *   *Recommendation*: **Python** is faster to iterate for simple text commands. **C#** is better if you want to reuse the exact `DiceRoll` logic from `RuneAndRust.Core`.
    *   *Decision*: Go with **C#** to ensure the "Dice Roller" behaves *exactly* like the game.

### 3.2 Integration
*   **Data Source**: Reads the same `gamedata.json` exported for the Web Companion, or queries the `rune-archive-web` API.

## 4. Development Roadmap

| Phase | Goal | Deliverables |
|-------|------|--------------|
| **1** | **Roller** | Basic bot that parses dice commands. |
| **2** | **Lookup** | Load JSON data and fuzzy match queries. |
| **3** | **Live** | Hook into API for server status/leaderboards. |
