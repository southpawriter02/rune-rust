#!/usr/bin/env bash
# docker-db-stop.sh - Stop PostgreSQL Docker container
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🛑 Stopping PostgreSQL Docker container..."

cd "$PROJECT_ROOT"

docker-compose stop postgres

echo "✅ PostgreSQL container stopped"
