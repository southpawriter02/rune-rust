#!/usr/bin/env bash
# docker-db-shell.sh - Connect to PostgreSQL container shell (psql)
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

echo "🐘 Connecting to PostgreSQL shell..."
echo "Database: RuneAndRust"
echo ""

docker-compose exec postgres psql -U postgres -d RuneAndRust
