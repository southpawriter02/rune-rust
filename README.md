# ᚱ Rune & Rust ᚱ

> *A text-based dungeon crawler set in the twilight of a broken world. Corrupted machines guard ancient ruins. Only the bold survive.*

[![Version](https://img.shields.io/badge/version-v0.4.3e-blue.svg)](docs/changelogs/v0.4.x/v0.4.3e.md)
[![Build](https://img.shields.io/badge/build-passing-brightgreen.svg)]()
[![Platform](https://img.shields.io/badge/platform-.NET%209.0-blue.svg)]()

---

## 🌑 Overview

**Rune & Rust** is a deep, turn-based dungeon crawler that combines traditional RPG mechanics with a rich, atmospheric terminal experience. Navigate a ruined facility, combat corrupted machine-spirits, manage your magical flux, and navigate the complex social landscape of the survivors.

### ⚙️ Core Loop

1.  **Awaken** → Craft your survivor from 4 archetypes (Warrior, Skirmisher, Mystic, Adept) and 10 levels of progression.
2.  **Scavenge** → Explore procedurally generated rooms across diverse biomes (Ruin, Industrial, Organic, Void).
3.  **Survive** → Engage in tactical dice-pool combat against machine-horrors using a d6 success-based system.
4.  **Harness** → Master the 4 schools of magic while avoiding the catastrophic corrupting touch of Flux.
5.  **Diplomacy** → Navigate reputation with 4 distinct factions and unlock unique dialogue and rewards.
6.  **Persist** → Save your journey via SQLite/PostgreSQL persistence and build your Legend.

---

## ⚡ What's New in v0.4.x "The Saga Cycle"

The v0.4 series transforms Rune & Rust from a vertical slice into a comprehensive RPG framework.

### v0.4.3: The Resonance (Current)
Introduces the **Magic & Spell Library System**, bringing tactical depth and risk-reward mechanics.
-   **✨ Magic System:** 4 schools (Destruction, Restoration, Alteration, Divination) with 12 starter spells.
-   **🌊 Flux & Corruption:** Manage magical resonance to avoid backlash and permanent Soul Corruption.
-   **📊 Magic UI:** Threshold-based visualization for Flux, Corruption, and AP reserves.

### v0.4.2: The Repute
Introduces the **Faction & Reputation System**.
-   **🤝 Factions:** Interact with the *Iron-Banes*, *Dvergr*, *The Bound*, and *The Faceless*.
-   **📈 Reputation:** Signed reputation (-100 to +100) with 5 disposition tiers from *Hated* to *Exalted*.
-   **💬 Dialogue:** Branching dialogue system with skill and reputation-gated responses.

### v0.4.0: The Legend
Introduces the **Saga Progression System**.
-   **🏆 Saga Backend:** Level cap increased to 10 with recursive multi-level jump handling.
-   **💎 Progression Points:** Earn PP on level-up to customize your attributes and specializations.
-   **🌲 Specialization Grid:** Initial infrastructure for class specializations and ability trees.

---

## 🛠️ Technology Stack

-   **Runtime:** .NET 9.0
-   **Language:** C# 13
-   **Interface:** [Spectre.Console](https://spectreconsole.net/) for rich, responsive terminal UI.
-   **Persistence:** EF Core with PostgreSQL (Docker-ready) and SQLite support.
-   **Architecture:** Clean Architecture / Service-Repository pattern.
-   **Audio:** Event-driven cross-platform console audio.

---

## 📂 Project Structure

-   `RuneAndRust.Core/`: Domain entities, interfaces, events, and constants.
-   `RuneAndRust.Engine/`: Core game logic, AI, loot tables, and combat resolution.
-   `RuneAndRust.Persistence/`: EF Core context, repositories, and data seeding.
-   `RuneAndRust.Terminal/`: The primary UI, renderers, and controllers.
-   `RuneAndRust.Tests/`: Over 3,800 unit and integration tests (100% pass rate).

---

## 🚀 Getting Started

### Prerequisites
-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   Docker Desktop (Optional, for PostgreSQL backend)

### Installation
1.  Clone the repository:
    ```bash
    git clone https://github.com/ryan/rune-rust.git
    cd rune-rust
    ```
2.  (Optional) Start the database:
    ```bash
    docker-compose up -d
    ```
3.  Build the project:
    ```bash
    dotnet build
    ```
4.  Run the game:
    ```bash
    dotnet run --project RuneAndRust.Terminal
    ```

---

## 🗺️ Roadmap

-   **v0.5.x:** Settlements, Economy, and Trade Districts.
-   **v0.6.x:** Advanced AI Traits, Stealth, and Boss Phase mechanics.
-   **v0.7.x:** Biome Expansion (Muspelheim, Niflheim, Jötunheim).
-   **v1.0.0:** Full Avalonia GUI Transition.

---

## 📜 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
