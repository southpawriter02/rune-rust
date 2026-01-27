# Proposal: Rune Gate API (`rune-gate-api`)

**Type**: Operational Service
**Target Audience**: Developers, Automated Systems

## 1. Vision
The **Rune Gate** is the secure border control between the wild internet and your pristine game database. It is a stateless web service that handles all online interactions for `Rune & Rust`.

It allows the single-player game to feel "connected" without the complexity of a real-time multiplayer server.

## 2. Key Features

### 2.1 Telemetry & Analytics
*   **Run Ingestion**: Accepts POST requests with summary data from a completed game run.
*   **Heatmaps**: "70% of players die in the 'Fungal Caverns' level."
*   **Balance Data**: "The 'Pyromancer' specialization has a 90% win rate (Needs Nerf)."

### 2.2 Global Leaderboards
*   Validates and ranks player performance.
*   Monthly seasons/resets.

### 2.3 Cloud Save Backup (Future)
*   Allow players to upload their local save file (encrypted) to retrieve it on another machine.

### 2.4 Message of the Day (MOTD)
*   The game client fetches a text file on startup to show news/patch notes.

## 3. Technical Specifications

### 3.1 Tech Stack
*   **Framework**: **ASP.NET Core 8 Web API** (C#).
    *   *Why?* Shares code (DTOs, Validation logic) with the main Game Repo.
*   **Database**: **PostgreSQL** (Central Cloud DB).
*   **Hosting**: Azure App Service or DigitalOcean Droplet (Dockerized).

### 3.2 Security Strategy (The "Anti-Hack" Layer)

The user specifically requested "hack-proof" ideas. While nothing is 100% hack-proof, we can mitigate risks:

1.  **Replay Validation**:
    *   Instead of the client sending "I scored 10,000 points", the client sends the *Input Log* (seed + list of moves).
    *   The Server (or a background worker) re-simulates the game logic using `RuneAndRust.Engine`.
    *   If Server Result == Client Result, the score is valid.
    *   *Note: This is resource intensive but the gold standard for anti-cheat.*

2.  **Sanity Checks (Cheaper)**:
    *   If a Level 1 character reports killing a Boss in 1 turn, flag for review.
    *   Use `RuneAndRust.Core` to verify max possible damage per turn.

3.  **Rate Limiting**:
    *   Prevent spam attacks using ASP.NET Core Rate Limiting middleware.

## 4. API Endpoints (Draft)

```http
GET  /v1/status          # Service health
GET  /v1/motd            # Message of the day
POST /v1/telemetry/run   # Submit run data
GET  /v1/leaderboards    # Get top scores
```

## 5. Development Roadmap

| Phase | Goal | Deliverables |
|-------|------|--------------|
| **1** | **Ingestion** | Basic API receiving JSON logs and saving to DB. |
| **2** | **Visualization** | Internal dashboard (Grafana?) to view telemetry. |
| **3** | **Public** | Public endpoints for Leaderboards + Web Companion integration. |
