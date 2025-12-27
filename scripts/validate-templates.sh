#!/bin/bash
#
# validate-templates.sh
# Validates all capture template JSON files for Rune & Rust
# v0.3.25a: Initial implementation for externalized capture templates.
#

set -e

echo "=== Capture Template Validator ==="
echo "v0.3.25a - Rune & Rust"
echo ""

SCHEMA_PATH="data/capture-templates/schema/capture-template.schema.json"
TEMPLATES_PATH="data/capture-templates/categories"

# Check if jq is available
if ! command -v jq &> /dev/null; then
    echo "ERROR: jq is required but not installed."
    echo "Install with: brew install jq (macOS) or apt install jq (Linux)"
    exit 1
fi

# Check if schema exists
if [ ! -f "$SCHEMA_PATH" ]; then
    echo "ERROR: Schema not found: $SCHEMA_PATH"
    exit 1
fi

echo "Loaded schema: $SCHEMA_PATH"

# Valid CaptureType enum values
VALID_TYPES=("TextFragment" "EchoRecording" "VisualRecord" "Specimen" "OralHistory" "RunicTrace")

# Domain 4 violation patterns
DOMAIN4_PATTERNS=(
    '[0-9]+%'                                    # Percentages
    '[0-9]+\.[0-9]+'                            # Decimal numbers
    '[0-9]+\s*(meters?|km|kilometers?|miles?|feet|ft)'  # Distance
    '[0-9]+\s*(degrees?|°C|°F|celsius|fahrenheit)'      # Temperature
    '[0-9]+\s*(seconds?|minutes?|hours?|days?|years?)'  # Time
    '[0-9]+\s*(ppm|dB|Hz|kg|lbs?|pounds?)'              # Technical units
    '[0-9]{4,}'                                  # Large precise numbers
    'API|CPU|GPU|RAM|ROM|HTTP|URL'              # Modern tech acronyms
)

ERRORS=()
WARNINGS=()
TEMPLATE_COUNT=0
ALL_IDS=()

# Get all template files
TEMPLATE_FILES=$(find "$TEMPLATES_PATH" -name "*.json" 2>/dev/null)

if [ -z "$TEMPLATE_FILES" ]; then
    echo "ERROR: No template files found in: $TEMPLATES_PATH"
    exit 1
fi

for FILE in $TEMPLATE_FILES; do
    FILENAME=$(basename "$FILE")
    echo ""
    echo "Validating: $FILENAME"

    # Check JSON validity
    if ! jq empty "$FILE" 2>/dev/null; then
        ERRORS+=("$FILENAME: Invalid JSON syntax")
        continue
    fi

    # Check required top-level fields
    if [ "$(jq -r '."$schema" // empty' "$FILE")" == "" ]; then
        ERRORS+=("$FILENAME: Missing '\$schema' field")
    fi
    if [ "$(jq -r '.category // empty' "$FILE")" == "" ]; then
        ERRORS+=("$FILENAME: Missing 'category' field")
    fi
    if [ "$(jq -r '.version // empty' "$FILE")" == "" ]; then
        ERRORS+=("$FILENAME: Missing 'version' field")
    fi

    TEMPLATE_ARRAY=$(jq -r '.templates // empty' "$FILE")
    if [ "$TEMPLATE_ARRAY" == "" ] || [ "$TEMPLATE_ARRAY" == "null" ]; then
        ERRORS+=("$FILENAME: Missing or empty 'templates' array")
        continue
    fi

    # Count templates in this file
    COUNT=$(jq '.templates | length' "$FILE")
    echo "  Found $COUNT templates"

    # Validate each template
    for i in $(seq 0 $((COUNT - 1))); do
        TEMPLATE_COUNT=$((TEMPLATE_COUNT + 1))

        ID=$(jq -r ".templates[$i].id // empty" "$FILE")
        TYPE=$(jq -r ".templates[$i].type // empty" "$FILE")
        CONTENT=$(jq -r ".templates[$i].fragmentContent // empty" "$FILE")
        SOURCE=$(jq -r ".templates[$i].source // empty" "$FILE")

        # Check required fields
        if [ -z "$ID" ]; then
            ERRORS+=("$FILENAME: Template #$((i+1)) missing 'id'")
            continue
        fi

        # Check for duplicate IDs
        if [[ " ${ALL_IDS[*]} " =~ " ${ID} " ]]; then
            ERRORS+=("$FILENAME: Duplicate template ID '$ID'")
        fi
        ALL_IDS+=("$ID")

        # Check ID format (kebab-case)
        if ! [[ "$ID" =~ ^[a-z][a-z0-9-]*$ ]]; then
            ERRORS+=("$FILENAME: Template '$ID' has invalid ID format (must be kebab-case)")
        fi

        # Check type
        if [ -z "$TYPE" ]; then
            ERRORS+=("$FILENAME: Template '$ID' missing 'type'")
        else
            TYPE_VALID=false
            for VALID_TYPE in "${VALID_TYPES[@]}"; do
                if [ "$TYPE" == "$VALID_TYPE" ]; then
                    TYPE_VALID=true
                    break
                fi
            done
            if [ "$TYPE_VALID" == "false" ]; then
                ERRORS+=("$FILENAME: Template '$ID' has invalid type '$TYPE'. Valid types: ${VALID_TYPES[*]}")
            fi
        fi

        # Check fragmentContent
        if [ -z "$CONTENT" ]; then
            ERRORS+=("$FILENAME: Template '$ID' missing 'fragmentContent'")
        else
            CONTENT_LEN=${#CONTENT}
            if [ $CONTENT_LEN -lt 20 ]; then
                ERRORS+=("$FILENAME: Template '$ID' fragmentContent too short ($CONTENT_LEN chars, minimum 20)")
            elif [ $CONTENT_LEN -gt 1000 ]; then
                ERRORS+=("$FILENAME: Template '$ID' fragmentContent too long ($CONTENT_LEN chars, maximum 1000)")
            fi
        fi

        # Check source
        if [ -z "$SOURCE" ]; then
            ERRORS+=("$FILENAME: Template '$ID' missing 'source'")
        else
            SOURCE_LEN=${#SOURCE}
            if [ $SOURCE_LEN -lt 5 ]; then
                ERRORS+=("$FILENAME: Template '$ID' source too short ($SOURCE_LEN chars, minimum 5)")
            fi
        fi

        # Domain 4 compliance check (basic grep)
        for PATTERN in "${DOMAIN4_PATTERNS[@]}"; do
            if echo "$CONTENT" | grep -qE "$PATTERN"; then
                MATCH=$(echo "$CONTENT" | grep -oE "$PATTERN" | head -1)
                WARNINGS+=("$FILENAME: Template '$ID' may violate Domain 4 (found '$MATCH')")
                break
            fi
        done

        # Quality range check
        QUALITY=$(jq -r ".templates[$i].quality // empty" "$FILE")
        if [ -n "$QUALITY" ] && [ "$QUALITY" != "null" ]; then
            if [ "$QUALITY" -lt 1 ] || [ "$QUALITY" -gt 100 ]; then
                ERRORS+=("$FILENAME: Template '$ID' quality out of range ($QUALITY, must be 1-100)")
            fi
        fi
    done

    echo "  OK $COUNT templates validated"
done

# Summary
echo ""
echo "=== Summary ==="
echo "Total template files: $(echo "$TEMPLATE_FILES" | wc -l | tr -d ' ')"
echo "Total templates: $TEMPLATE_COUNT"

if [ ${#WARNINGS[@]} -gt 0 ]; then
    echo ""
    echo "Warnings: ${#WARNINGS[@]}"
    for WARN in "${WARNINGS[@]}"; do
        echo "  ! $WARN"
    done
fi

if [ ${#ERRORS[@]} -gt 0 ]; then
    echo ""
    echo "Errors: ${#ERRORS[@]}"
    for ERR in "${ERRORS[@]}"; do
        echo "  X $ERR"
    done
    exit 1
else
    echo ""
    echo "OK All templates valid!"
    exit 0
fi
