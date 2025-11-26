#!/usr/bin/env python3
"""
Descriptor Export Tool for Notion

Exports all descriptor data from SQL files to Notion-compatible formats:
- CSV files (for Notion database imports)
- Markdown files (for Notion pages)
- JSON files (for programmatic use)

Usage:
    python export_descriptors.py [--format csv|md|json|all] [--output-dir ./exports]

Supported descriptor types:
    - Room fragments (v0.38.1)
    - Room function variants (v0.38.1)
    - Atmospheric descriptors (v0.38.4)
    - Thematic modifiers (v0.38.0)
    - Base templates (v0.38.0)
    - Combat flavor text (v0.38.6)
    - Status effect descriptors (v0.38.8)
    - Trauma descriptors (v0.38.14)
    - And more...
"""

import os
import re
import csv
import json
import argparse
from pathlib import Path
from datetime import datetime
from typing import List, Dict, Any, Tuple, Optional


# SQL file patterns to process
SQL_FILES = {
    "descriptor_fragments": "v0.38.1_descriptor_fragments_content.sql",
    "room_function_variants": "v0.38.1_room_function_variants.sql",
    "atmospheric_descriptors": "v0.38.4_atmospheric_descriptors.sql",
    "thematic_modifiers": "v0.38.0_descriptor_framework_schema.sql",
    "combat_flavor": "v0.38.6_combat_flavor_text_schema.sql",
    "status_effects": "v0.38.8_status_effect_descriptors_data.sql",
    "galdr_actions": "v0.38.7_galdr_action_descriptors.sql",
    "galdr_outcomes": "v0.38.7_galdr_outcome_descriptors.sql",
    "npc_barks": "v0.38.11_npc_descriptors_barks_data.sql",
    "trauma_types": "v0.38.14_trauma_type_descriptors_content.sql",
    "trauma_triggers": "v0.38.14_trauma_trigger_descriptors_content.sql",
    "breaking_points": "v0.38.14_breaking_point_descriptors_content.sql",
    "recovery_descriptors": "v0.38.14_recovery_descriptors_content.sql",
    "examination_descriptors": "v0.38.9_examination_perception_descriptors_data.sql",
    "skill_usage": "v0.38.10_skill_usage_descriptors_data.sql",
    "combat_mechanics": "v0.38.12_combat_mechanics_descriptors_data.sql",
    "ambient_environmental": "v0.38.13_ambient_environmental_descriptors_data.sql",
}


def find_data_directory() -> Path:
    """Find the Data directory relative to this script."""
    script_dir = Path(__file__).parent
    # Try relative to script location
    data_dir = script_dir.parent / "Data"
    if data_dir.exists():
        return data_dir
    # Try current working directory
    data_dir = Path.cwd() / "Data"
    if data_dir.exists():
        return data_dir
    raise FileNotFoundError("Could not find Data directory")


def parse_insert_statement(sql: str, table_pattern: str) -> List[Dict[str, Any]]:
    """
    Parse INSERT statements from SQL and extract values.

    Args:
        sql: The SQL content to parse
        table_pattern: Regex pattern to match table names

    Returns:
        List of dictionaries with extracted data
    """
    results = []

    # Pattern to match INSERT INTO statements
    # Handles both single-row and multi-row inserts
    insert_pattern = rf"INSERT\s+(?:OR\s+IGNORE\s+)?INTO\s+({table_pattern})\s*\(([^)]+)\)\s*VALUES?\s*"

    matches = list(re.finditer(insert_pattern, sql, re.IGNORECASE | re.DOTALL))

    for match in matches:
        table_name = match.group(1)
        columns_str = match.group(2)
        columns = [c.strip() for c in columns_str.split(',')]

        # Find the values section after the match
        remaining = sql[match.end():]

        # Extract all value tuples
        values_pattern = r"\(([^)]+)\)"
        value_matches = re.finditer(values_pattern, remaining)

        for vm in value_matches:
            values_str = vm.group(1)

            # Stop if we hit a semicolon before this match (end of statement)
            preceding = remaining[:vm.start()]
            if ';' in preceding:
                break

            # Parse the values (handle quoted strings, NULL, numbers)
            values = parse_sql_values(values_str)

            if len(values) == len(columns):
                row = {"_table": table_name}
                for col, val in zip(columns, values):
                    row[col] = val
                results.append(row)

    return results


def parse_sql_values(values_str: str) -> List[Any]:
    """Parse a comma-separated list of SQL values."""
    values = []
    current = ""
    in_string = False
    string_char = None
    i = 0

    while i < len(values_str):
        char = values_str[i]

        if in_string:
            if char == string_char:
                # Check for escaped quote
                if i + 1 < len(values_str) and values_str[i + 1] == string_char:
                    current += char
                    i += 1
                else:
                    in_string = False
            else:
                current += char
        else:
            if char in ("'", '"'):
                in_string = True
                string_char = char
            elif char == ',':
                values.append(parse_value(current.strip()))
                current = ""
            else:
                current += char
        i += 1

    # Don't forget the last value
    if current.strip():
        values.append(parse_value(current.strip()))

    return values


def parse_value(val: str) -> Any:
    """Parse a single SQL value."""
    if val.upper() == 'NULL':
        return None
    if val.startswith("'") and val.endswith("'"):
        return val[1:-1].replace("''", "'")
    if val.startswith('"') and val.endswith('"'):
        return val[1:-1].replace('""', '"')
    try:
        if '.' in val:
            return float(val)
        return int(val)
    except ValueError:
        return val


def extract_descriptor_fragments(sql: str) -> List[Dict[str, Any]]:
    """Extract descriptor fragments from SQL."""
    return parse_insert_statement(sql, r"Descriptor_Fragments")


def extract_room_function_variants(sql: str) -> List[Dict[str, Any]]:
    """Extract room function variants from SQL."""
    return parse_insert_statement(sql, r"Room_Function_Variants")


def extract_atmospheric_descriptors(sql: str) -> List[Dict[str, Any]]:
    """Extract atmospheric descriptors from SQL."""
    return parse_insert_statement(sql, r"Atmospheric_Descriptors")


def extract_thematic_modifiers(sql: str) -> List[Dict[str, Any]]:
    """Extract thematic modifiers from SQL."""
    return parse_insert_statement(sql, r"Descriptor_Thematic_Modifiers")


def extract_base_templates(sql: str) -> List[Dict[str, Any]]:
    """Extract base templates from SQL."""
    return parse_insert_statement(sql, r"Descriptor_Base_Templates")


def extract_generic_table(sql: str, table_name: str) -> List[Dict[str, Any]]:
    """Extract data from any table matching the pattern."""
    return parse_insert_statement(sql, table_name)


def export_to_csv(data: List[Dict[str, Any]], output_path: Path, name: str) -> Path:
    """Export data to CSV file."""
    if not data:
        return None

    output_file = output_path / f"{name}.csv"

    # Get all unique keys (columns)
    all_keys = set()
    for row in data:
        all_keys.update(row.keys())

    # Remove internal keys
    all_keys.discard('_table')
    columns = sorted(all_keys)

    with open(output_file, 'w', newline='', encoding='utf-8') as f:
        writer = csv.DictWriter(f, fieldnames=columns, extrasaction='ignore')
        writer.writeheader()
        for row in data:
            # Clean row (remove _table, handle None)
            clean_row = {k: (v if v is not None else '') for k, v in row.items() if k != '_table'}
            writer.writerow(clean_row)

    return output_file


def export_to_markdown(data: List[Dict[str, Any]], output_path: Path, name: str, title: str = None) -> Path:
    """Export data to Markdown file suitable for Notion."""
    if not data:
        return None

    output_file = output_path / f"{name}.md"

    # Get all unique keys (columns)
    all_keys = set()
    for row in data:
        all_keys.update(row.keys())
    all_keys.discard('_table')
    columns = sorted(all_keys)

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(f"# {title or name.replace('_', ' ').title()}\n\n")
        f.write(f"*Exported: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}*\n\n")
        f.write(f"**Total entries:** {len(data)}\n\n")
        f.write("---\n\n")

        # Group by category if available
        if 'category' in columns:
            grouped = {}
            for row in data:
                cat = row.get('category', 'Uncategorized')
                if cat not in grouped:
                    grouped[cat] = []
                grouped[cat].append(row)

            for category, rows in sorted(grouped.items()):
                f.write(f"## {category}\n\n")
                for row in rows:
                    write_row_as_markdown(f, row, columns)
                f.write("\n")
        else:
            for row in data:
                write_row_as_markdown(f, row, columns)

    return output_file


def write_row_as_markdown(f, row: Dict[str, Any], columns: List[str]):
    """Write a single row as Markdown."""
    # Try to find a good title field
    title_fields = ['descriptor_text', 'fragment_text', 'function_name', 'modifier_name',
                    'template_name', 'bark_text', 'description', 'text']
    title = None
    for field in title_fields:
        if field in row and row[field]:
            title = str(row[field])[:80]
            if len(str(row[field])) > 80:
                title += "..."
            break

    if title:
        f.write(f"### {title}\n\n")

    # Write properties as a list
    for col in columns:
        if col in row and row[col] is not None and col not in title_fields[:2]:
            value = row[col]
            # Format JSON arrays nicely
            if isinstance(value, str) and value.startswith('['):
                try:
                    parsed = json.loads(value)
                    value = ', '.join(str(v) for v in parsed)
                except:
                    pass
            f.write(f"- **{col.replace('_', ' ').title()}:** {value}\n")
    f.write("\n")


def export_to_json(data: List[Dict[str, Any]], output_path: Path, name: str) -> Path:
    """Export data to JSON file."""
    if not data:
        return None

    output_file = output_path / f"{name}.json"

    # Clean data (remove internal keys)
    clean_data = []
    for row in data:
        clean_row = {k: v for k, v in row.items() if k != '_table'}
        clean_data.append(clean_row)

    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump({
            "name": name,
            "exported_at": datetime.now().isoformat(),
            "count": len(clean_data),
            "data": clean_data
        }, f, indent=2, ensure_ascii=False)

    return output_file


def export_notion_database_template(data: List[Dict[str, Any]], output_path: Path, name: str) -> Path:
    """
    Export a Notion database template with property definitions.
    This creates a markdown file that describes the database schema.
    """
    if not data:
        return None

    output_file = output_path / f"{name}_notion_template.md"

    # Analyze data to determine property types
    all_keys = set()
    for row in data:
        all_keys.update(row.keys())
    all_keys.discard('_table')

    property_types = {}
    for key in all_keys:
        # Sample values to determine type
        values = [row.get(key) for row in data[:10] if row.get(key) is not None]
        if not values:
            property_types[key] = "Text"
        elif all(isinstance(v, (int, float)) for v in values):
            property_types[key] = "Number"
        elif all(isinstance(v, str) and v.startswith('[') for v in values):
            property_types[key] = "Multi-select"
        elif all(isinstance(v, str) and len(v) > 100 for v in values):
            property_types[key] = "Text (Long)"
        else:
            # Check if it's a select (limited unique values)
            unique_values = set(str(v) for v in values)
            if len(unique_values) <= 10:
                property_types[key] = "Select"
            else:
                property_types[key] = "Text"

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(f"# Notion Database Template: {name.replace('_', ' ').title()}\n\n")
        f.write("## Database Properties\n\n")
        f.write("Use this template to create a Notion database with the following properties:\n\n")
        f.write("| Property Name | Type | Description |\n")
        f.write("|--------------|------|-------------|\n")

        for key in sorted(all_keys):
            prop_type = property_types[key]
            f.write(f"| {key.replace('_', ' ').title()} | {prop_type} | - |\n")

        f.write("\n## Import Instructions\n\n")
        f.write("1. Create a new Notion database\n")
        f.write("2. Add the properties listed above\n")
        f.write(f"3. Import the CSV file: `{name}.csv`\n")
        f.write("4. Map the CSV columns to database properties\n")
        f.write("\n## Sample Data\n\n")
        f.write("First 3 entries from the export:\n\n")

        for i, row in enumerate(data[:3]):
            f.write(f"### Entry {i+1}\n")
            for key in sorted(all_keys):
                if key in row and row[key] is not None:
                    f.write(f"- **{key}:** {row[key]}\n")
            f.write("\n")

    return output_file


def create_notion_master_index(exported_files: Dict[str, List[Path]], output_path: Path) -> Path:
    """Create a master index page for Notion."""
    output_file = output_path / "00_descriptor_catalog_index.md"

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write("# Rune & Rust: Descriptor Catalog\n\n")
        f.write(f"*Exported: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}*\n\n")
        f.write("This catalog contains all procedural content descriptors used by the Dynamic Room Engine.\n\n")
        f.write("---\n\n")

        f.write("## Quick Links\n\n")
        for name, files in sorted(exported_files.items()):
            f.write(f"- [{name.replace('_', ' ').title()}]({name}.md)\n")

        f.write("\n---\n\n")
        f.write("## Descriptor Categories\n\n")

        categories = {
            "Room Generation": ["descriptor_fragments", "room_function_variants"],
            "Atmosphere & Environment": ["atmospheric_descriptors", "ambient_environmental"],
            "Thematic System": ["thematic_modifiers", "base_templates"],
            "Combat": ["combat_flavor", "combat_mechanics"],
            "Magic (Galdr)": ["galdr_actions", "galdr_outcomes"],
            "Status Effects": ["status_effects"],
            "NPCs": ["npc_barks"],
            "Trauma & Psychology": ["trauma_types", "trauma_triggers", "breaking_points", "recovery_descriptors"],
            "Skills & Examination": ["examination_descriptors", "skill_usage"],
        }

        for category, names in categories.items():
            f.write(f"### {category}\n\n")
            for name in names:
                if name in exported_files:
                    f.write(f"- [{name.replace('_', ' ').title()}]({name}.md)\n")
            f.write("\n")

        f.write("---\n\n")
        f.write("## Database Import Instructions\n\n")
        f.write("To import into Notion as databases:\n\n")
        f.write("1. Create a new Notion page for your catalog\n")
        f.write("2. Add linked databases for each descriptor type\n")
        f.write("3. Use the CSV files in the `csv/` folder to import data\n")
        f.write("4. Reference the `*_notion_template.md` files for property setup\n")

    return output_file


def main():
    parser = argparse.ArgumentParser(description="Export descriptors to Notion-compatible formats")
    parser.add_argument('--format', choices=['csv', 'md', 'json', 'all'], default='all',
                        help="Export format (default: all)")
    parser.add_argument('--output-dir', type=str, default='./exports',
                        help="Output directory (default: ./exports)")
    parser.add_argument('--data-dir', type=str, default=None,
                        help="Data directory containing SQL files")
    args = parser.parse_args()

    # Setup directories
    if args.data_dir:
        data_dir = Path(args.data_dir)
    else:
        data_dir = find_data_directory()

    output_dir = Path(args.output_dir)
    output_dir.mkdir(parents=True, exist_ok=True)

    # Create subdirectories for each format
    csv_dir = output_dir / "csv"
    md_dir = output_dir / "markdown"
    json_dir = output_dir / "json"

    if args.format in ('csv', 'all'):
        csv_dir.mkdir(exist_ok=True)
    if args.format in ('md', 'all'):
        md_dir.mkdir(exist_ok=True)
    if args.format in ('json', 'all'):
        json_dir.mkdir(exist_ok=True)

    exported_files = {}

    print(f"Reading SQL files from: {data_dir}")
    print(f"Exporting to: {output_dir}")
    print()

    # Process each SQL file
    for name, filename in SQL_FILES.items():
        filepath = data_dir / filename
        if not filepath.exists():
            print(f"  Skipping {name}: file not found ({filename})")
            continue

        print(f"Processing {name}...")

        with open(filepath, 'r', encoding='utf-8') as f:
            sql = f.read()

        # Extract data based on the type
        data = []
        if name == "descriptor_fragments":
            data = extract_descriptor_fragments(sql)
        elif name == "room_function_variants":
            data = extract_room_function_variants(sql)
        elif name == "atmospheric_descriptors":
            data = extract_atmospheric_descriptors(sql)
        elif name == "thematic_modifiers":
            data = extract_thematic_modifiers(sql)
            data.extend(extract_base_templates(sql))
        else:
            # Try generic extraction for common table patterns
            common_tables = [
                r"\w+_Descriptors?",
                r"\w+_Variants?",
                r"\w+_Profiles?",
                r"\w+_Barks?",
                r"\w+_Actions?",
                r"\w+_Outcomes?",
            ]
            for pattern in common_tables:
                extracted = parse_insert_statement(sql, pattern)
                if extracted:
                    data.extend(extracted)

        if not data:
            print(f"  No data extracted from {name}")
            continue

        print(f"  Found {len(data)} entries")

        exported = []

        # Export to requested formats
        if args.format in ('csv', 'all'):
            path = export_to_csv(data, csv_dir, name)
            if path:
                exported.append(path)
                print(f"  Exported CSV: {path}")

        if args.format in ('md', 'all'):
            path = export_to_markdown(data, md_dir, name)
            if path:
                exported.append(path)
                print(f"  Exported Markdown: {path}")

            # Also create Notion template
            template_path = export_notion_database_template(data, md_dir, name)
            if template_path:
                exported.append(template_path)

        if args.format in ('json', 'all'):
            path = export_to_json(data, json_dir, name)
            if path:
                exported.append(path)
                print(f"  Exported JSON: {path}")

        exported_files[name] = exported

    # Create master index for Notion
    if args.format in ('md', 'all') and exported_files:
        index_path = create_notion_master_index(exported_files, md_dir)
        print(f"\nCreated master index: {index_path}")

    print("\n" + "="*60)
    print("Export complete!")
    print(f"Total descriptor types exported: {len(exported_files)}")
    print(f"Output directory: {output_dir.absolute()}")
    print("\nTo import into Notion:")
    print("  1. CSV files can be imported directly into Notion databases")
    print("  2. Markdown files can be imported as Notion pages")
    print("  3. Use the *_notion_template.md files for database schema reference")


if __name__ == "__main__":
    main()
