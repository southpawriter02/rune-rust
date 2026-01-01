#!/usr/bin/env bash
# docker-db-logs.sh - View PostgreSQL container logs
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

cd "$PROJECT_ROOT"

# Follow logs by default, use -f flag
docker-compose logs -f postgres
