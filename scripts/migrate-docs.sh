#!/bin/bash
# migrate-docs.sh - Convert docs/*.md to migrated/*.txt
# Usage: ./scripts/migrate-docs.sh
#
# This script converts all markdown files under docs/ (specs, plans, changelogs)
# to .txt files in a "migrated/" directory, preserving the folder structure.

set -e

# Configuration
SOURCE_DIR="docs"
TARGET_DIR="migrated"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}Docs Migration Script${NC}"
echo "====================="
echo ""

# Ensure we're running from project root
if [ ! -d "$SOURCE_DIR" ]; then
    echo "Error: '$SOURCE_DIR' directory not found."
    echo "Please run this script from the project root."
    exit 1
fi

# Clean and create target directory
echo "Cleaning target directory: $TARGET_DIR/"
rm -rf "$TARGET_DIR"
mkdir -p "$TARGET_DIR"

# Find all .md files and convert
echo "Converting markdown files..."
echo ""

file_count=0
find "$SOURCE_DIR" -type f -name "*.md" | sort | while read -r file; do
    # Calculate relative path and new filename
    relative_path="${file#$SOURCE_DIR/}"
    target_path="$TARGET_DIR/${relative_path%.md}.txt"

    # Create parent directories
    mkdir -p "$(dirname "$target_path")"

    # Copy with new extension
    cp "$file" "$target_path"

    echo "  $relative_path -> ${relative_path%.md}.txt"
done

echo ""
echo -e "${GREEN}Migration complete!${NC}"
echo ""
echo "Summary:"
echo "  Source:      $SOURCE_DIR/"
echo "  Destination: $TARGET_DIR/"
echo "  Files:       $(find "$TARGET_DIR" -type f -name "*.txt" | wc -l | tr -d ' ')"
echo ""
echo "To verify: ls -la $TARGET_DIR/"
