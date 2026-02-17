# Docker PostgreSQL Quick Start Guide

Get up and running with PostgreSQL testing in under 5 minutes.

## Prerequisites

- Docker Desktop installed and running
- .NET 9.0 SDK installed
- Git repository cloned

## Method 1: Manual Docker (Recommended for Local Development)

### Step 1: Verify Setup

```bash
./scripts/verify-docker-setup.sh
```

This checks that all required components are installed and configured.

### Step 2: Start PostgreSQL

```bash
./scripts/docker-db-start.sh
```

**Expected output:**
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

### Step 3: Run Tests

```bash
dotnet test --filter "Category=Integration"
```

### Step 4: (Optional) Inspect Database

```bash
# Open PostgreSQL shell
./scripts/docker-db-shell.sh

# View tables
\dt

# Exit
\q
```

### Step 5: Stop PostgreSQL

```bash
./scripts/docker-db-stop.sh
```

---

## Method 2: Testcontainers (Fully Automated)

### Step 1: Restore Dependencies

```bash
dotnet restore
```

### Step 2: Run Tests

```bash
dotnet test --filter "Category=Testcontainers"
```

That's it! The container is automatically created and destroyed.

---

## Common Tasks

### Reset Database (Clean Slate)

```bash
./scripts/docker-db-reset.sh
./scripts/docker-db-start.sh
```

### View Logs

```bash
./scripts/docker-db-logs.sh
```

### Check Container Status

```bash
docker ps | grep runeandrust-db
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~CharacterPersistenceTests"
```

---

## Troubleshooting

### Docker Not Running

**Error:** `Cannot connect to the Docker daemon`

**Solution:**
```bash
# Start Docker Desktop
# Then retry the command
```

### Port Conflict

**Error:** `port is already allocated`

**Solution:**
```bash
# Stop conflicting service on port 5433
lsof -i :5433

# Or change port in docker-compose.yml
```

### Tests Failing

**Error:** Connection errors in tests

**Solution:**
```bash
# Ensure container is running
docker ps | grep runeandrust-db

# Check logs
./scripts/docker-db-logs.sh

# Reset if needed
./scripts/docker-db-reset.sh
./scripts/docker-db-start.sh
```

---

## Next Steps

- Read the [full documentation](DOCKER-POSTGRESQL-TESTING.md)
- Check out [scripts README](../../scripts/README.md)
- Learn about [test fixtures](../../RuneAndRust.Tests/Integration/PostgreSQL/)

---

## Quick Reference Card

```bash
# Verify setup
./scripts/verify-docker-setup.sh

# Start database
./scripts/docker-db-start.sh

# Run all integration tests
dotnet test --filter "Category=Integration"

# Run PostgreSQL-specific tests
dotnet test --filter "Category=PostgreSQL"

# Run Testcontainers tests
dotnet test --filter "Category=Testcontainers"

# Open database shell
./scripts/docker-db-shell.sh

# View logs
./scripts/docker-db-logs.sh

# Stop database
./scripts/docker-db-stop.sh

# Reset database (delete all data)
./scripts/docker-db-reset.sh
```

---

**Need help?** See [DOCKER-POSTGRESQL-TESTING.md](DOCKER-POSTGRESQL-TESTING.md) for comprehensive documentation.
