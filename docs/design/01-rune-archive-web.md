# Proposal: Rune Archive Web (`rune-archive-web`)

**Type**: External Tool / Community Resource
**Target Audience**: Players, Theory-crafters

## 1. Vision
A centralized, web-based hub for all `Rune & Rust` knowledge. It serves as a dynamic Wiki, a Character Planner, and a window into the global state of the game (Leaderboards).

Unlike a static wiki (like Fandom), the **Rune Archive** is data-driven. When you update a spell's damage in the game code, the website updates automatically via the deployment pipeline.

## 2. Key Features

### 2.1 The Codex (Wiki)
*   **Auto-Generated Pages**: Pages for Archetypes, Specializations, Items, and Enemies generated directly from game data.
*   **Search**: Instant fuzzy search for mechanics (e.g., "What does 'Bleed' do?").
*   **Version History**: "This spell was nerfed in v0.5".

### 2.2 Character Planner
*   **Build Calculator**: Select an Archetype, spend Profession Points (PP), and choose Specializations.
*   **Stat Preview**: See resulting Attributes (MIGHT, WITS, etc.) and derived stats (HP, Stamina).
*   **Shareable Builds**: Generates a short URL (e.g., `runeandrust.com/build/x92ks`) to share with the community.

### 2.3 The Hall of Heroes (Leaderboards)
*   *Requires `rune-gate-api`.*
*   **Top Runs**: Filter by Dungeon Depth, Speed, or Score.
*   **Graveyard**: A view of the most recent player deaths and what killed them.

## 3. Technical Specifications

### 3.1 Tech Stack
*   **Framework**: **Next.js** (React) - Best for SEO and static generation.
*   **Hosting**: Vercel or Netlify (Zero cost for starter tiers).
*   **Styling**: Tailwind CSS - Rapid UI development.
*   **State Management**: Zustand or Redux (for the Character Planner).

### 3.2 Integration with Game Data
To avoid rewriting game logic in JavaScript:

**Option A: JSON Export (Recommended for Simplicity)**
1.  Create a script in the main Game Repo: `ExportData.cs`.
2.  This script serializes all `Abilities`, `Items`, and `Classes` to a large `gamedata.json`.
3.  The Web Repo imports this JSON file during build time.

**Option B: WASM (Advanced)**
1.  Compile `RuneAndRust.Core` to WebAssembly.
2.  The web app runs the *actual* C# C# damage formulas in the browser.

## 4. Development Roadmap

| Phase | Goal | Deliverables |
|-------|------|--------------|
| **1** | **Static Codex** | JSON export script + Next.js site displaying Items/Spells. |
| **2** | **Planner** | Interactive UI to select classes and view ability trees. |
| **3** | **Live Data** | Integration with API for Leaderboards. |

## 5. Security Implications
*   **Low Risk**: This is a client-side application. It only reads public data.
*   **No DB Access**: It fetches leaderboard data from `rune-gate-api`, never the DB directly.
