# Scripts Directory

This directory contains helper scripts for Rune & Rust development and testing.

## PostgreSQL Docker Scripts

These scripts manage the PostgreSQL Docker container for local development and testing.

### Quick Reference

```bash
# Start PostgreSQL
./docker-db-start.sh

# Stop PostgreSQL
./docker-db-stop.sh

# Reset PostgreSQL (delete all data)
./docker-db-reset.sh

# View logs
./docker-db-logs.sh

# Open PostgreSQL shell
./docker-db-shell.sh
```

### Detailed Documentation

#### `docker-db-start.sh`
Starts the PostgreSQL Docker container and waits for it to be ready.

**Usage:**
```bash
./scripts/docker-db-start.sh
```

**Output:**
- Connection details (host, port, database, credentials)
- Connection string for configuration

**Exit codes:**
- `0` - Success
- `1` - Docker not running or container failed to start

---

#### `docker-db-stop.sh`
Stops the PostgreSQL container without removing data.

**Usage:**
```bash
./scripts/docker-db-stop.sh
```

**Note:** Data persists in the `runeandrust_pgdata` volume.

---

#### `docker-db-reset.sh`
Completely resets the PostgreSQL database by removing the container and volume.

**Usage:**
```bash
./scripts/docker-db-reset.sh
```

**⚠️ Warning:** This deletes all data. Use when you need a clean database.

**Use cases:**
- Migration issues
- Corrupted data
- Starting fresh

---

#### `docker-db-logs.sh`
Displays PostgreSQL container logs in real-time.

**Usage:**
```bash
./scripts/docker-db-logs.sh
```

**Controls:**
- Press `Ctrl+C` to exit
- Logs are followed by default (`-f` flag)

**Useful for:**
- Debugging connection issues
- Monitoring query performance
- Investigating errors

---

#### `docker-db-shell.sh`
Opens an interactive PostgreSQL shell (psql) connected to the RuneAndRust database.

**Usage:**
```bash
./scripts/docker-db-shell.sh
```

**Common psql commands:**
```sql
-- List all tables
\dt

-- Describe a table
\d "TableName"

-- List all databases
\l

-- List all schemas
\dn

-- Show current connection info
\conninfo

-- Execute SQL file
\i /path/to/file.sql

-- Exit
\q
```

---

## Workflow Examples

### First-time Setup

```bash
# 1. Start the database
./scripts/docker-db-start.sh

# 2. Run migrations (from project root)
cd RuneAndRust.Persistence
dotnet ef database update
cd ..

# 3. Run tests
dotnet test
```

### Daily Development

```bash
# Start database (if not running)
./scripts/docker-db-start.sh

# Do your development...

# Stop database when done
./scripts/docker-db-stop.sh
```

### Debugging Database Issues

```bash
# View logs
./scripts/docker-db-logs.sh

# Or open a shell to inspect directly
./scripts/docker-db-shell.sh
```

### Reset After Migration Changes

```bash
# Reset the database
./scripts/docker-db-reset.sh

# Start fresh
./scripts/docker-db-start.sh

# Apply new migrations
cd RuneAndRust.Persistence
dotnet ef database update
```

---

## Environment Variables

These scripts respect the following environment variables:

| Variable | Purpose | Default |
|----------|---------|---------|
| `RUNEANDRUST_CONNECTION_STRING` | Override connection string | `Host=localhost;Port=5433;...` |

**Example:**
```bash
export RUNEANDRUST_CONNECTION_STRING="Host=localhost;Port=5434;Database=CustomDB;Username=postgres;Password=password"
./scripts/docker-db-start.sh
```

---

## Troubleshooting

### Script Permission Denied

If you get a "Permission denied" error:

```bash
chmod +x scripts/docker-db-*.sh
```

### Docker Not Running

If scripts fail with Docker errors:

```bash
# Check Docker status
docker info

# Start Docker Desktop if needed
```

### Port Already in Use

If port 5433 is already in use:

1. Check what's using it:
   ```bash
   lsof -i :5433
   ```

2. Stop the conflicting process or change the port in `docker-compose.yml`:
   ```yaml
   ports:
     - "5434:5432"  # Change 5433 to 5434
   ```

### Container Won't Start

If the container fails to start:

```bash
# View detailed logs
./scripts/docker-db-logs.sh

# Reset everything
./scripts/docker-db-reset.sh
./scripts/docker-db-start.sh
```

---

## Additional Resources

- [Full Docker PostgreSQL Testing Guide](../docs/testing/DOCKER-POSTGRESQL-TESTING.md)
- [docker-compose.yml](../docker-compose.yml)
- [PostgreSQL Official Documentation](https://www.postgresql.org/docs/)

---

## Contributing

When adding new scripts:

1. Follow the naming convention: `docker-db-{action}.sh`
2. Add execute permissions: `chmod +x scripts/docker-db-{action}.sh`
3. Include error handling and user-friendly output
4. Document the script in this README
5. Use the template structure from existing scripts
