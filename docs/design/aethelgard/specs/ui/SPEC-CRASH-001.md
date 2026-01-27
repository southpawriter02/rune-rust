---
id: SPEC-CRASH-001
title: Global Exception Handler & Crash Reporting
version: 1.0.0
status: Implemented
last_updated: 2025-12-25
related_specs: [SPEC-SAVE-001, SPEC-GAME-001]
---

# SPEC-CRASH-001: Global Exception Handler & Crash Reporting

> **Version:** 1.0.0
> **Status:** Implemented
> **Service:** `ICrashService`, `CrashService`, `CrashScreenRenderer`
> **Location:** `RuneAndRust.Engine/Services/CrashService.cs`, `RuneAndRust.Terminal/Rendering/CrashScreenRenderer.cs`

---

## Table of Contents

- [Overview](#overview)
- [Core Concepts](#core-concepts)
- [Behaviors](#behaviors)
- [Restrictions](#restrictions)
- [Limitations](#limitations)
- [Use Cases](#use-cases)
- [Decision Trees](#decision-trees)
- [Cross-Links](#cross-links)
- [Related Services](#related-services)
- [Data Models](#data-models)
- [Configuration](#configuration)
- [Testing](#testing)
- [Design Rationale](#design-rationale)
- [Changelog](#changelog)

---

## Overview

The Global Exception Handler & Crash Reporting system (codename "The Safety Net") provides production-grade error recovery for Rune & Rust. When an unhandled exception occurs, the system captures diagnostic information, writes a crash report to disk, optionally creates an emergency save backup, and displays a user-friendly "Red Screen of Death" with reporting instructions.

This system operates at the application boundary in `Program.Main`, wrapping the game loop in a `try-catch` block that excludes graceful shutdown events (`OperationCanceledException` from Ctrl+C). The design prioritizes resilience—if any recovery step fails, subsequent steps still execute, ensuring the user always sees meaningful feedback.

The crash handling system was introduced in v0.3.16a with emergency save backup added in v0.3.16b.

---

## Core Concepts

### Crash Report

A structured record capturing all diagnostic information at the moment of failure. Includes exception type, message, stack trace, inner exceptions, and system environment details (OS version, .NET runtime, game version). Written as human-readable plain text with ASCII formatting.

### Red Screen of Death

A terminal-based crash UI rendered via Spectre.Console. Displays a red-bordered panel with exception summary, crash log location, backup status (v0.3.16b), and a link to report issues on GitHub.

### DI-Independent Execution

Critical crash handling components must function when the dependency injection container is unavailable (e.g., if the host failed to build). `CrashScreenRenderer` is static for this reason, and `CrashService` can be manually instantiated with a `NullLogger`.

### Graceful Shutdown Exclusion

`OperationCanceledException` is explicitly excluded from crash handling to allow Ctrl+C to terminate the application cleanly without generating a crash report.

---

## Behaviors

### Primary Behaviors

#### 1. Log Crash to File (`LogCrash`)

```csharp
string LogCrash(Exception ex)
```

**Purpose:** Generates a timestamped crash report file with full diagnostic information.

**Logic:**
1. Ensure `logs/crashes/` directory exists (auto-create if missing)
2. Build `CrashReport` record from exception and environment
3. Generate filename using timestamp: `crash_YYYYMMDD_HHmmss.txt`
4. Format report as human-readable text with ASCII borders
5. Write to disk via `File.WriteAllText`
6. Log action via Serilog at Information level
7. Return the full path to the crash file

**Example:**
```csharp
var crashService = new CrashService(logger);
var path = crashService.LogCrash(ex);
// Returns: "logs/crashes/crash_20251225_143045.txt"
```

#### 2. Generate Report Path (`GenerateReportPath`)

```csharp
string GenerateReportPath(DateTime timestamp)
```

**Purpose:** Produces a consistent, sortable filename for crash reports.

**Logic:**
1. Combine crash directory with timestamp-based filename
2. Use format `crash_YYYYMMDD_HHmmss.txt` with zero-padding

**Example:**
```csharp
var path = crashService.GenerateReportPath(new DateTime(2025, 1, 5, 9, 3, 7));
// Returns: "logs/crashes/crash_20250105_090307.txt"
```

#### 3. Render Crash Screen (`Render`)

```csharp
static void Render(Exception ex, string? logPath = null, bool? backupSaved = null)
```

**Purpose:** Displays the "Red Screen of Death" with exception details and recovery status.

**Logic:**
1. Clear terminal via `AnsiConsole.Clear()`
2. Build content panel with exception type and truncated message (max 200 chars)
3. If `logPath` provided, show path; otherwise show "Unable to save crash report"
4. If `backupSaved` has value, show backup success/failure status
5. Display GitHub issues link for reporting
6. Show "Press ENTER to exit..." prompt
7. Wait for user acknowledgement via `Console.ReadLine()`

**Example:**
```csharp
CrashScreenRenderer.Render(ex, logPath, backupSaved: true);
// Displays red-bordered panel and waits for ENTER
```

---

## Restrictions

### What This System MUST NOT Do

1. **Never crash the crash handler:** All operations within catch blocks must be wrapped in their own try-catch to prevent cascading failures.

2. **Never treat OperationCanceledException as a crash:** Ctrl+C is a graceful shutdown, not an error condition.

3. **Never require DI container:** The crash renderer must be static and work when the host failed to build.

4. **Never block indefinitely:** While waiting for user acknowledgement, the prompt must be clear about how to exit.

5. **Never expose sensitive data:** Crash reports should contain technical details only, not user credentials or save file contents.

---

## Limitations

### Known Constraints

| Limitation | Value | Rationale |
|------------|-------|-----------|
| Message truncation | 200 characters | Prevents display overflow in terminal |
| Crash directory | `logs/crashes/` | Hardcoded; not configurable at runtime |
| Game version | Hardcoded constant | Must be updated manually each release |
| Single-threaded write | No concurrency protection | Crashes are rare; simplicity over thread safety |
| Report format | Plain text only | Human-readable without tooling |

---

## Use Cases

### UC-1: Standard Crash Recovery

**Actor:** Application runtime
**Trigger:** Unhandled exception thrown during game loop
**Preconditions:** Game is running; exception is not OperationCanceledException

```csharp
try
{
    var game = host.Services.GetRequiredService<IGameService>();
    game.StartAsync().GetAwaiter().GetResult();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    // 1. Log crash
    string? logPath = null;
    try
    {
        var crashService = new CrashService(NullLogger<CrashService>.Instance);
        logPath = crashService.LogCrash(ex);
    }
    catch { /* Swallow - don't crash the crash handler */ }

    // 2. Emergency save (v0.3.16b)
    bool? backupSaved = null;
    try
    {
        await saveManager.CreateCrashBackupAsync();
        backupSaved = true;
    }
    catch { backupSaved = false; }

    // 3. Display crash screen
    CrashScreenRenderer.Render(ex, logPath, backupSaved);

    // 4. Log to Serilog
    Log.Fatal(ex, "System Crash");
}
```

**Postconditions:**
- Crash report file exists in `logs/crashes/`
- User has seen crash screen with recovery options
- Application exits cleanly

### UC-2: DI Container Build Failure

**Actor:** Application startup
**Trigger:** Exception during host building (before DI container exists)
**Preconditions:** Host.CreateDefaultBuilder or service registration fails

```csharp
try
{
    var host = Host.CreateDefaultBuilder(args)
        .ConfigureServices((ctx, services) => { /* registration fails */ })
        .Build();
}
catch (Exception ex)
{
    // CrashService manually instantiated with NullLogger
    var crashService = new CrashService(NullLogger<CrashService>.Instance);
    var logPath = crashService.LogCrash(ex);

    // Static renderer works without DI
    CrashScreenRenderer.Render(ex, logPath);
}
```

**Postconditions:** User sees crash screen despite DI failure

### UC-3: Graceful Shutdown (Not a Crash)

**Actor:** User
**Trigger:** User presses Ctrl+C
**Preconditions:** Game is running

```csharp
catch (Exception ex) when (ex is not OperationCanceledException)
{
    // OperationCanceledException excluded - no crash handling
}
// Normal cleanup proceeds in finally block
```

**Postconditions:** Application exits without crash report or crash screen

---

## Decision Trees

### Exception Handling Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    Exception Thrown                          │
└────────────────────────────┬────────────────────────────────┘
                             │
                    ┌────────┴────────┐
                    │ Is Exception    │
                    │ Cancellation?   │
                    └────────┬────────┘
                             │
              ┌──────────────┼──────────────┐
              │ YES          │              │ NO
              ▼              │              ▼
    ┌─────────────────┐      │    ┌─────────────────────────┐
    │ Allow normal    │      │    │ 1. Try: LogCrash()      │
    │ shutdown flow   │      │    │    → On fail: swallow   │
    └─────────────────┘      │    │ 2. Try: EmergencyBackup │
                             │    │    → On fail: note it   │
                             │    │ 3. Render crash screen  │
                             │    │ 4. Log.Fatal()          │
                             │    │ 5. Exit                 │
                             │    └─────────────────────────┘
```

### Crash Report Generation Flow

```
┌─────────────────────────────────────────────────────────────┐
│                    LogCrash(Exception)                       │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ Directory.Create   │
                    │ (logs/crashes/)    │
                    └────────┬───────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ Build CrashReport  │
                    │ - Timestamp        │
                    │ - ExceptionType    │
                    │ - Message          │
                    │ - StackTrace       │
                    │ - InnerException   │
                    │ - GameVersion      │
                    │ - OS, Runtime      │
                    └────────┬───────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ FormatReport()     │
                    │ (ASCII borders)    │
                    └────────┬───────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ File.WriteAllText  │
                    └────────┬───────────┘
                             │
                             ▼
                    ┌────────────────────┐
                    │ Return file path   │
                    └────────────────────┘
```

---

## Cross-Links

### Dependencies (Consumes)

| Service | Specification | Usage |
|---------|---------------|-------|
| `ILogger<T>` | *(external)* | Logging crash events |
| `SaveManager` | [SPEC-SAVE-001](../data/SPEC-SAVE-001.md) | Emergency backup on crash (v0.3.16b) |

### Dependents (Consumed By)

| Service | Specification | Usage |
|---------|---------------|-------|
| `Program.Main` | [SPEC-GAME-001](../core/SPEC-GAME-001.md) | Top-level exception handling |

---

## Related Services

### Primary Implementation

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Engine/Services/CrashService.cs` | Crash logging service | 1-121 |
| `RuneAndRust.Terminal/Rendering/CrashScreenRenderer.cs` | Red Screen UI | 1-100 |
| `RuneAndRust.Terminal/Program.cs` | Integration point | 438-480 |

### Supporting Types

| File | Purpose | Key Lines |
|------|---------|-----------|
| `RuneAndRust.Core/Interfaces/ICrashService.cs` | Service contract | 1-25 |
| `RuneAndRust.Core/Models/CrashReport.cs` | Crash data DTO | 1-50 |

---

## Data Models

### CrashReport

```csharp
/// <summary>
/// Data Transfer Object for crash report data.
/// </summary>
/// <remarks>See: SPEC-CRASH-001 for Crash Handling System design.</remarks>
public record CrashReport
{
    // Timing
    public DateTime Timestamp { get; init; }

    // Exception Details
    public string ExceptionType { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string StackTrace { get; init; } = string.Empty;
    public string? InnerException { get; init; }

    // System Information
    public string GameVersion { get; init; } = string.Empty;
    public string OperatingSystem { get; init; } = string.Empty;
    public string RuntimeVersion { get; init; } = string.Empty;
}
```

### Report File Format

```
═══════════════════════════════════════════════════════════════════════════════
                         RUNE & RUST CRASH REPORT
═══════════════════════════════════════════════════════════════════════════════

Timestamp:      2025-12-25 14:30:45
Game Version:   v0.3.16a
OS:             Microsoft Windows 10.0.22631
.NET Runtime:   8.0.0

───────────────────────────────────────────────────────────────────────────────
EXCEPTION DETAILS
───────────────────────────────────────────────────────────────────────────────

Type:    System.NullReferenceException
Message: Object reference not set to an instance of an object.

STACK TRACE:
─────────────
   at RuneAndRust.Engine.Services.SomeService.DoWork() in ...
   at RuneAndRust.Terminal.Program.Main(String[] args) in ...

───────────────────────────────────────────────────────────────────────────────
INNER EXCEPTION
───────────────────────────────────────────────────────────────────────────────

[Inner exception details if present]

═══════════════════════════════════════════════════════════════════════════════
Please report this issue at:
https://github.com/southpawriter02/rune-rust/issues
═══════════════════════════════════════════════════════════════════════════════
```

---

## Configuration

### Constants

```csharp
// CrashService.cs
private const string CrashDirectory = "logs/crashes";
private const string GameVersion = "v0.3.16a";
```

### Settings

| Setting | Default | Range | Purpose |
|---------|---------|-------|---------|
| Crash directory | `logs/crashes` | N/A | Where crash files are written |
| Message truncation | 200 chars | N/A | Max exception message length in UI |

---

## Testing

### Test Files

| File | Tests | Coverage |
|------|-------|----------|
| `RuneAndRust.Tests/Engine/CrashServiceTests.cs` | 10 | Core crash logging functionality |

### Critical Test Scenarios

1. **GenerateReportPath_CreatesValidFilename** - Verifies correct timestamp formatting
2. **GenerateReportPath_PadsZeros_ForSingleDigitValues** - Ensures `01` not `1`
3. **LogCrash_CreatesDirectory_IfMissing** - Auto-creates `logs/crashes/`
4. **LogCrash_WritesFile_WithExceptionDetails** - Captures type, message, params
5. **LogCrash_IncludesSystemInfo** - Captures version, OS, runtime
6. **LogCrash_HandlesNestedExceptions** - Captures inner exception chain
7. **LogCrash_ReturnsValidPath** - Path starts with directory, ends with `.txt`
8. **LogCrash_IncludesStackTrace** - Stack trace appears in output
9. **LogCrash_IncludesReportingUrl** - GitHub URL in footer
10. **LogCrash_HandlesNullStackTrace** - Graceful handling of unthrown exceptions

### Validation Checklist

- [x] Crash file created in correct directory
- [x] Filename follows `crash_YYYYMMDD_HHmmss.txt` pattern
- [x] All 8 CrashReport properties populated
- [x] Report contains GitHub issues URL
- [x] Inner exceptions captured when present
- [x] OperationCanceledException excluded from handling

---

## Design Rationale

### Why Static CrashScreenRenderer?

- The renderer must work when DI container fails to build
- Static methods have no dependencies on container state
- Simple, reliable fallback for worst-case scenarios

### Why Plain Text Reports?

- Human-readable without special tooling
- Works on all operating systems
- Easy to attach to GitHub issues
- ASCII formatting survives copy/paste

### Why Swallow Errors in Catch Block?

- Crash handler must never crash
- Each recovery step (log, backup, render) is independent
- Partial success is better than total failure
- User should always see something, even if incomplete

### Why Exclude OperationCanceledException?

- Ctrl+C is intentional user action, not a bug
- Would generate misleading crash reports
- Allows clean shutdown via finally blocks
- Standard .NET pattern for cancellation

---

## Changelog

### v1.0.0 (2025-12-25)

- Initial specification documenting v0.3.16a/b implementation
- Documents CrashService, CrashReport, CrashScreenRenderer
- Documents integration in Program.Main
- Documents emergency save backup (v0.3.16b feature)
