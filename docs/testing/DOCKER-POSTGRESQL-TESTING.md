# Docker PostgreSQL Testing Guide

This guide covers all aspects of PostgreSQL testing with Docker for Rune & Rust development.

## Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Testing Approaches](#testing-approaches)
4. [Docker Scripts Reference](#docker-scripts-reference)
5. [Configuration](#configuration)
6. [CI/CD Integration](#cicd-integration)
7. [Troubleshooting](#troubleshooting)

---

## Overview

Rune & Rust uses PostgreSQL 16 as its primary database. For testing, we provide two approaches:

1. **Manual Docker Container** - Use `docker-compose` with helper scripts
2. **Testcontainers** - Automated container management in tests

### When to Use Each Approach

| Approach | Best For | Pros | Cons |
|----------|----------|------|------|
| **Manual Docker** | Local development, repeated test runs | Fast (container persists), simple debugging | Requires manual start/stop |
| **Testcontainers** | CI/CD, isolated test runs, new developers | Fully automated, no manual setup | Slower startup (~5-10s) |

---

## Quick Start

### Prerequisites

- Docker Desktop installed and running
- .NET 9.0 SDK
- PostgreSQL client tools (optional, for debugging)

### Option 1: Manual Docker Container (Recommended for Local Development)

```bash
# Start the PostgreSQL container
./scripts/docker-db-start.sh

# Run your tests
dotnet test

# Stop the container when done
./scripts/docker-db-stop.sh
```

### Option 2: Testcontainers (Automated)

```bash
# Just run tests - container is managed automatically
dotnet test --filter "Category=Testcontainers"
```

No manual container management needed! The container is created and destroyed automatically.

---

## Testing Approaches

### Manual Docker Container Setup

The manual approach uses `docker-compose` with a persistent container.

#### Connection Details

```
Host:     localhost
Port:     5433
Database: RuneAndRust
Username: postgres
Password: password

Connection String:
Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password
```

#### Using PostgreSqlTestFixture

```csharp
using RuneAndRust.Tests.Integration.PostgreSQL;
using Xunit;

[Collection("PostgreSQL")]
public class MyDatabaseTests
{
    private readonly PostgreSqlTestFixture _fixture;

    public MyDatabaseTests(PostgreSqlTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestDatabaseOperation()
    {
        // Create isolated context for this test
        using var context = _fixture.CreateContext();

        // Your test code here
        var character = new Character { Name = "Test Hero" };
        context.Characters.Add(character);
        await context.SaveChangesAsync();

        // Assertions
        character.Id.Should().BeGreaterThan(0);
    }
}
```

**Key Features:**
- Creates unique test database per fixture instance
- Automatic cleanup after tests
- Shared fixture across test collection for efficiency
- Uses environment variable `RUNEANDRUST_CONNECTION_STRING` if set

### Testcontainers Setup

Testcontainers automatically manages the Docker container lifecycle.

#### Using TestcontainersFixture

```csharp
using RuneAndRust.Tests.Integration.PostgreSQL;
using Xunit;

[Collection("Testcontainers")]
public class MyContainerizedTests
{
    private readonly TestcontainersFixture _fixture;

    public MyContainerizedTests(TestcontainersFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestWithAutomatedContainer()
    {
        using var context = _fixture.CreateContext();

        // Test code - container is already running
        var spell = new Spell { Name = "Fireball" };
        context.Spells.Add(spell);
        await context.SaveChangesAsync();

        spell.Id.Should().BeGreaterThan(0);
    }
}
```

**Key Features:**
- No manual Docker setup required
- Container created on first test run
- Automatic cleanup after all tests complete
- Perfect for CI/CD pipelines
- Port 5435 (avoids conflicts with dev container on 5433)

---

## Docker Scripts Reference

All scripts are located in `./scripts/` and require execute permissions.

### `docker-db-start.sh`

Start the PostgreSQL Docker container.

```bash
./scripts/docker-db-start.sh
```

**What it does:**
1. Checks if Docker is running
2. Starts the `postgres` service via docker-compose
3. Waits for PostgreSQL to be ready (max 30 seconds)
4. Displays connection details

**Output:**
```
🐘 Starting PostgreSQL Docker container...
⏳ Waiting for PostgreSQL to be ready...
✅ PostgreSQL is ready!

Connection details:
  Host:     localhost
  Port:     5433
  Database: RuneAndRust
  Username: postgres
  Password: password
```

### `docker-db-stop.sh`

Stop the PostgreSQL container (preserves data).

```bash
./scripts/docker-db-stop.sh
```

**What it does:**
- Stops the container without removing data
- Data persists in the `runeandrust_pgdata` volume

### `docker-db-reset.sh`

Completely reset the database (destroy and recreate).

```bash
./scripts/docker-db-reset.sh
```

**What it does:**
1. Stops the container
2. Removes the container
3. Deletes the data volume

**⚠️ Warning:** This destroys all data. Use when you need a clean slate.

### `docker-db-logs.sh`

View PostgreSQL container logs (follows by default).

```bash
./scripts/docker-db-logs.sh
```

**What it does:**
- Displays container logs in real-time
- Useful for debugging connection issues or query problems
- Press Ctrl+C to exit

### `docker-db-shell.sh`

Connect to the PostgreSQL shell (psql).

```bash
./scripts/docker-db-shell.sh
```

**What it does:**
- Opens an interactive `psql` session
- Connected to the `RuneAndRust` database
- Useful for manual inspection or debugging

**Example commands in psql:**
```sql
-- List all tables
\dt

-- Describe a table
\d "Characters"

-- Query data
SELECT * FROM "Characters" LIMIT 10;

-- Exit
\q
```

---

## Configuration

### docker-compose.yml

The main Docker configuration file:

```yaml
services:
  postgres:
    image: postgres:16
    container_name: runeandrust-db
    environment:
      POSTGRES_DB: RuneAndRust
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: password
    ports:
      - "5433:5432"
    volumes:
      - runeandrust_pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres -d RuneAndRust"]
      interval: 5s
      timeout: 5s
      retries: 5
```

### Optional: Separate Test Database

Uncomment the `postgres-test` service in `docker-compose.yml` for complete isolation:

```yaml
postgres-test:
  image: postgres:16
  container_name: runeandrust-db-test
  ports:
    - "5434:5432"
  tmpfs:
    - /var/lib/postgresql/data  # In-memory for speed
```

Then update the environment variable:
```bash
export RUNEANDRUST_CONNECTION_STRING="Host=localhost;Port=5434;Username=postgres;Password=password"
```

### Environment Variables

| Variable | Purpose | Default |
|----------|---------|---------|
| `RUNEANDRUST_CONNECTION_STRING` | Override default connection | `Host=localhost;Port=5433;...` |

**Usage:**
```bash
# Set custom connection for tests
export RUNEANDRUST_CONNECTION_STRING="Host=localhost;Port=5434;Database=CustomDB;Username=postgres;Password=password"

# Run tests with custom connection
dotnet test
```

---

## CI/CD Integration

### GitHub Actions

Create `.github/workflows/postgresql-tests.yml`:

```yaml
name: PostgreSQL Integration Tests

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main, develop ]

jobs:
  test:
    runs-on: ubuntu-latest

    services:
      postgres:
        image: postgres:16
        env:
          POSTGRES_DB: RuneAndRust
          POSTGRES_USER: postgres
          POSTGRES_PASSWORD: password
        ports:
          - 5433:5432
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x'

    - name: Restore dependencies
      run: dotnet restore

    - name: Run PostgreSQL Tests
      run: dotnet test --filter "Category=Integration" --logger "trx;LogFileName=test-results.trx"
      env:
        RUNEANDRUST_CONNECTION_STRING: "Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password"

    - name: Upload Test Results
      if: always()
      uses: actions/upload-artifact@v4
      with:
        name: test-results
        path: '**/test-results.trx'
```

### Using Testcontainers in CI

Testcontainers is the simplest approach for CI/CD:

```yaml
- name: Run Tests with Testcontainers
  run: dotnet test --filter "Category=Testcontainers"
```

No service container configuration needed - Testcontainers handles everything!

---

## Troubleshooting

### Container Won't Start

**Problem:** `docker-db-start.sh` times out or fails

**Solutions:**
1. Check if Docker Desktop is running:
   ```bash
   docker info
   ```

2. Check for port conflicts:
   ```bash
   lsof -i :5433
   ```
   If port is in use, change the port in `docker-compose.yml`

3. View container logs:
   ```bash
   ./scripts/docker-db-logs.sh
   ```

4. Reset everything:
   ```bash
   ./scripts/docker-db-reset.sh
   ./scripts/docker-db-start.sh
   ```

### Tests Fail with Connection Errors

**Problem:** Tests can't connect to PostgreSQL

**Solutions:**
1. Ensure container is running:
   ```bash
   docker ps | grep runeandrust-db
   ```

2. Test connection manually:
   ```bash
   ./scripts/docker-db-shell.sh
   ```

3. Check environment variable:
   ```bash
   echo $RUNEANDRUST_CONNECTION_STRING
   ```

4. Verify port mapping:
   ```bash
   docker-compose ps
   ```

### Testcontainers Fails to Start

**Problem:** Testcontainers tests fail with Docker connection errors

**Solutions:**
1. Ensure Docker Desktop is running

2. Check Docker socket permissions:
   ```bash
   # macOS/Linux
   ls -la /var/run/docker.sock
   ```

3. Verify Docker API is accessible:
   ```bash
   docker version
   ```

4. Check for conflicting containers:
   ```bash
   docker ps -a | grep postgres
   ```

### Database Migration Issues

**Problem:** Migrations fail or database schema is out of sync

**Solutions:**
1. Reset the database:
   ```bash
   ./scripts/docker-db-reset.sh
   ./scripts/docker-db-start.sh
   ```

2. Manually apply migrations:
   ```bash
   cd RuneAndRust.Persistence
   dotnet ef database update
   ```

3. Check migration status:
   ```bash
   dotnet ef migrations list
   ```

### Performance Issues

**Problem:** Tests are running slowly

**Solutions:**
1. **Use tmpfs for test database** (Linux/macOS):
   Uncomment `tmpfs` in `postgres-test` service

2. **Adjust resource limits** in `docker-compose.yml`:
   ```yaml
   deploy:
     resources:
       limits:
         cpus: '4'      # Increase CPU
         memory: 4G     # Increase memory
   ```

3. **Use connection pooling** in tests:
   ```csharp
   builder.UseNpgsql(connectionString, options =>
       options.MaxBatchSize(100));
   ```

### Data Cleanup Between Tests

**Problem:** Test data persists and causes conflicts

**Solutions:**
1. **Use test transactions** (rollback after each test):
   ```csharp
   [Fact]
   public async Task TestWithTransaction()
   {
       using var context = _fixture.CreateContext();
       using var transaction = await context.Database.BeginTransactionAsync();

       // Test code

       await transaction.RollbackAsync(); // No commit
   }
   ```

2. **Create fresh context per test**:
   ```csharp
   using var context = _fixture.CreateContext();
   ```
   The `PostgreSqlTestFixture` creates unique databases per fixture.

---

## Best Practices

### Test Organization

1. **Use collections for grouping**:
   ```csharp
   [Collection("PostgreSQL")]  // Shares fixture, faster
   public class CharacterTests { }

   [Collection("PostgreSQL")]
   public class SpellTests { }
   ```

2. **Isolate integration tests**:
   ```
   RuneAndRust.Tests/
   ├── Integration/
   │   ├── PostgreSQL/          # PostgreSQL-specific tests
   │   ├── CharacterTests.cs    # Uses in-memory or PostgreSQL
   │   └── ...
   └── Unit/                    # Pure unit tests
   ```

3. **Tag tests appropriately**:
   ```csharp
   [Trait("Category", "Integration")]
   [Trait("Category", "Database")]
   [Trait("Category", "PostgreSQL")]
   public class MyTests { }
   ```

### Performance Optimization

1. **Minimize context creation**:
   ```csharp
   // ❌ Bad - creates many contexts
   [Fact]
   public async Task Test1()
   {
       using var ctx1 = _fixture.CreateContext();
       using var ctx2 = _fixture.CreateContext();  // Unnecessary
   }

   // ✅ Good - single context
   [Fact]
   public async Task Test2()
   {
       using var context = _fixture.CreateContext();
       // All operations use same context
   }
   ```

2. **Use async/await consistently**:
   ```csharp
   await context.SaveChangesAsync();     // ✅
   context.SaveChanges();                // ❌ Blocks thread
   ```

3. **Batch operations when possible**:
   ```csharp
   context.Characters.AddRange(characters);  // ✅ Single batch
   await context.SaveChangesAsync();

   // vs
   foreach (var c in characters)
   {
       context.Characters.Add(c);            // ❌ Multiple operations
       await context.SaveChangesAsync();
   }
   ```

### Security

1. **Never commit credentials** to source control
2. **Use environment variables** for connection strings in CI/CD
3. **Rotate test database passwords** periodically
4. **Don't use production credentials** in tests

---

## Additional Resources

- [PostgreSQL Docker Official Image](https://hub.docker.com/_/postgres)
- [Testcontainers Documentation](https://dotnet.testcontainers.org/)
- [Entity Framework Core Documentation](https://learn.microsoft.com/ef/core/)
- [xUnit Test Patterns](https://xunit.net/)

---

## Summary

- **Local Development**: Use `docker-db-start.sh` for persistent container
- **CI/CD**: Use Testcontainers for automated testing
- **Debugging**: Use `docker-db-shell.sh` for manual inspection
- **Reset**: Use `docker-db-reset.sh` when you need a clean slate

For issues or questions, refer to the [Troubleshooting](#troubleshooting) section or check the project's issue tracker.
