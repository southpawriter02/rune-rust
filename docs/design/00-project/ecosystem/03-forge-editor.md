# Proposal: The Forge Editor (`forge-editor`)

**Type**: Development Aid / Modding Tool
**Target Audience**: Developer (You), Content Designers, Modders

## 1. Vision
Currently, adding content (Items, Rooms, Specializations) likely involves editing JSON files or C# code directly. This is error-prone and slow.

**The Forge** is a visual GUI tool that allows you to "fill in the blanks" to create content. It ensures that you never break the game by referencing a texture or ID that doesn't exist.

## 2. Key Features

### 2.1 Specialization Tree Editor
*   **Node Graph View**: Visual drag-and-drop interface for Ability Trees.
*   **Validation**: "You cannot connect a Rank 3 ability to a Rank 1 parent."
*   **Formula Builder**: Dropdowns to build effects like `Damage = MIGHT * 1.5 + 2`.

### 2.2 Dungeon Architect
*   **Room Layouts**: Grid-based editor for designing pre-made rooms (vaults, puzzles).
*   **Entity Placement**: Place enemies, traps, and loot chests visually.

### 2.3 Item Smith
*   Create items, assign stats, and preview their tooltips exactly as they appear in-game.

## 3. Technical Specifications

### 3.1 Tech Stack
*   **Framework**: **AvaloniaUI** (C#).
    *   *Why?* You are already using Avalonia for the game. You can reuse the actual *Game UI Controls* (e.g., the `ItemTooltip` control) inside the editor to ensure WYSIWYG.
*   **Output**: JSON / YAML files.

### 3.2 Integration
1.  **Shared Core**: The Editor references `RuneAndRust.Core` to know what `DamageType.Fire` or `Attribute.Might` means.
2.  **File Watchers**: The Game Client can watch the "Mods" folder. When you save in the Editor, the Game hot-reloads the data.

## 4. Modding Potential
By releasing this tool to the public, you essentially open-source your content pipeline. Users can create "Level Packs" or "New Class Mods" without needing to see your source code.

## 5. Development Roadmap

| Phase | Goal | Deliverables |
|-------|------|--------------|
| **1** | **Item Editor** | Simple form to edit/save Item JSONs. |
| **2** | **Tree Editor** | Visual graph editor for Specializations. |
| **3** | **Mod Support** | Ability to package files into a `.rrmod` zip file. |
