#!/usr/bin/env bash
# docker-db-reset.sh - Reset PostgreSQL database (destroy and recreate)
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔄 Resetting PostgreSQL database..."

cd "$PROJECT_ROOT"

# Stop and remove the container and volume
docker-compose down -v postgres

echo "✅ Database reset complete"
echo ""
echo "Run './scripts/docker-db-start.sh' to start a fresh database"
