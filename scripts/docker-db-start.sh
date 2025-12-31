#!/usr/bin/env bash
# docker-db-start.sh - Start PostgreSQL Docker container for testing
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🐘 Starting PostgreSQL Docker container..."

cd "$PROJECT_ROOT"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    echo "❌ Error: Docker is not running. Please start Docker Desktop."
    exit 1
fi

# Start the container
docker-compose up -d postgres

# Wait for PostgreSQL to be ready
echo "⏳ Waiting for PostgreSQL to be ready..."
MAX_ATTEMPTS=30
ATTEMPT=0

while [ $ATTEMPT -lt $MAX_ATTEMPTS ]; do
    if docker-compose exec -T postgres pg_isready -U postgres -d RuneAndRust > /dev/null 2>&1; then
        echo "✅ PostgreSQL is ready!"
        echo ""
        echo "Connection details:"
        echo "  Host:     localhost"
        echo "  Port:     5433"
        echo "  Database: RuneAndRust"
        echo "  Username: postgres"
        echo "  Password: password"
        echo ""
        echo "Connection string:"
        echo "  Host=localhost;Port=5433;Database=RuneAndRust;Username=postgres;Password=password"
        echo ""
        exit 0
    fi

    ATTEMPT=$((ATTEMPT + 1))
    sleep 1
done

echo "❌ Error: PostgreSQL failed to start within 30 seconds"
echo "Check logs with: docker-compose logs postgres"
exit 1
