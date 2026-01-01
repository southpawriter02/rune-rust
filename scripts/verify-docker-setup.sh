#!/usr/bin/env bash
# verify-docker-setup.sh - Verify Docker PostgreSQL testing setup
set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

echo "🔍 Verifying Docker PostgreSQL Setup for Rune & Rust"
echo "=================================================="
echo ""

# Color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Track overall status
ALL_CHECKS_PASSED=true

# Function to print status
print_status() {
    if [ $1 -eq 0 ]; then
        echo -e "${GREEN}✓${NC} $2"
    else
        echo -e "${RED}✗${NC} $2"
        ALL_CHECKS_PASSED=false
    fi
}

# Check 1: Docker is installed
echo "1. Checking Docker installation..."
if command -v docker &> /dev/null; then
    DOCKER_VERSION=$(docker --version)
    print_status 0 "Docker is installed: $DOCKER_VERSION"
else
    print_status 1 "Docker is not installed"
fi
echo ""

# Check 2: Docker is running
echo "2. Checking if Docker is running..."
if docker info &> /dev/null; then
    print_status 0 "Docker daemon is running"
else
    print_status 1 "Docker daemon is not running. Please start Docker Desktop."
fi
echo ""

# Check 3: docker-compose is available
echo "3. Checking docker-compose..."
if docker compose version &> /dev/null; then
    COMPOSE_VERSION=$(docker compose version)
    print_status 0 "docker-compose is available: $COMPOSE_VERSION"
else
    print_status 1 "docker-compose is not available"
fi
echo ""

# Check 4: docker-compose.yml exists
echo "4. Checking docker-compose.yml..."
if [ -f "$PROJECT_ROOT/docker-compose.yml" ]; then
    print_status 0 "docker-compose.yml found"
else
    print_status 1 "docker-compose.yml not found"
fi
echo ""

# Check 5: Helper scripts exist and are executable
echo "5. Checking helper scripts..."
SCRIPTS=(
    "docker-db-start.sh"
    "docker-db-stop.sh"
    "docker-db-reset.sh"
    "docker-db-logs.sh"
    "docker-db-shell.sh"
)

for script in "${SCRIPTS[@]}"; do
    SCRIPT_PATH="$SCRIPT_DIR/$script"
    if [ -f "$SCRIPT_PATH" ]; then
        if [ -x "$SCRIPT_PATH" ]; then
            print_status 0 "$script exists and is executable"
        else
            print_status 1 "$script exists but is not executable (run: chmod +x scripts/$script)"
        fi
    else
        print_status 1 "$script not found"
    fi
done
echo ""

# Check 6: .NET SDK installed
echo "6. Checking .NET SDK..."
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    print_status 0 ".NET SDK is installed: $DOTNET_VERSION"

    # Check for .NET 9.0
    if [[ "$DOTNET_VERSION" == 9.* ]]; then
        print_status 0 ".NET 9.0 detected (required)"
    else
        print_status 1 ".NET 9.0 not detected (found $DOTNET_VERSION)"
    fi
else
    print_status 1 ".NET SDK is not installed"
fi
echo ""

# Check 7: Test project configuration
echo "7. Checking test project..."
TEST_CSPROJ="$PROJECT_ROOT/RuneAndRust.Tests/RuneAndRust.Tests.csproj"
if [ -f "$TEST_CSPROJ" ]; then
    print_status 0 "Test project found"

    # Check for Testcontainers package
    if grep -q "Testcontainers.PostgreSql" "$TEST_CSPROJ"; then
        print_status 0 "Testcontainers.PostgreSql package reference found"
    else
        print_status 1 "Testcontainers.PostgreSql package not found"
    fi
else
    print_status 1 "Test project not found"
fi
echo ""

# Check 8: PostgreSQL test fixtures
echo "8. Checking PostgreSQL test fixtures..."
PG_FIXTURE="$PROJECT_ROOT/RuneAndRust.Tests/Integration/PostgreSQL/PostgreSqlTestFixture.cs"
TC_FIXTURE="$PROJECT_ROOT/RuneAndRust.Tests/Integration/PostgreSQL/TestcontainersFixture.cs"

if [ -f "$PG_FIXTURE" ]; then
    print_status 0 "PostgreSqlTestFixture.cs found"
else
    print_status 1 "PostgreSqlTestFixture.cs not found"
fi

if [ -f "$TC_FIXTURE" ]; then
    print_status 0 "TestcontainersFixture.cs found"
else
    print_status 1 "TestcontainersFixture.cs not found"
fi
echo ""

# Check 9: Documentation
echo "9. Checking documentation..."
DOCKER_DOCS="$PROJECT_ROOT/docs/testing/DOCKER-POSTGRESQL-TESTING.md"
SCRIPTS_README="$SCRIPT_DIR/README.md"

if [ -f "$DOCKER_DOCS" ]; then
    print_status 0 "Docker PostgreSQL testing documentation found"
else
    print_status 1 "Docker PostgreSQL testing documentation not found"
fi

if [ -f "$SCRIPTS_README" ]; then
    print_status 0 "Scripts README.md found"
else
    print_status 1 "Scripts README.md not found"
fi
echo ""

# Check 10: Optional - Test if container can be started
echo "10. Testing Docker container startup (optional)..."
if docker info &> /dev/null; then
    echo -e "${YELLOW}   Starting PostgreSQL container for verification...${NC}"

    cd "$PROJECT_ROOT"
    if docker compose up -d postgres &> /dev/null; then
        print_status 0 "PostgreSQL container started successfully"

        # Wait for container to be ready
        MAX_WAIT=15
        WAITED=0
        while [ $WAITED -lt $MAX_WAIT ]; do
            if docker compose exec -T postgres pg_isready -U postgres -d RuneAndRust &> /dev/null; then
                print_status 0 "PostgreSQL is accepting connections"
                break
            fi
            sleep 1
            WAITED=$((WAITED + 1))
        done

        if [ $WAITED -ge $MAX_WAIT ]; then
            print_status 1 "PostgreSQL failed to accept connections within ${MAX_WAIT}s"
        fi

        # Clean up
        echo -e "${YELLOW}   Stopping container...${NC}"
        docker compose stop postgres &> /dev/null
    else
        print_status 1 "Failed to start PostgreSQL container"
    fi
else
    echo -e "${YELLOW}   Skipped (Docker not running)${NC}"
fi
echo ""

# Summary
echo "=================================================="
if [ "$ALL_CHECKS_PASSED" = true ]; then
    echo -e "${GREEN}✓ All checks passed!${NC}"
    echo ""
    echo "Your Docker PostgreSQL testing setup is ready."
    echo ""
    echo "Next steps:"
    echo "  1. Start PostgreSQL:  ./scripts/docker-db-start.sh"
    echo "  2. Run tests:         dotnet test"
    echo "  3. Read docs:         docs/testing/DOCKER-POSTGRESQL-TESTING.md"
    exit 0
else
    echo -e "${RED}✗ Some checks failed${NC}"
    echo ""
    echo "Please review the errors above and fix any issues."
    echo "Refer to: docs/testing/DOCKER-POSTGRESQL-TESTING.md"
    exit 1
fi
