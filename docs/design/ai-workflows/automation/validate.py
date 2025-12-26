"""
Rune & Rust - AI Validation Automation Script

This script scans documentation files and prepares validation prompts for an LLM.
It uses the 'Validator' persona to check for Domain 4 compliance and structural integrity.

Usage:
    python validate.py [options]

Options:
    --dir <path>        Directory to scan (default: docs/)
    --output <path>     Output directory for generated prompts (default: ./prompts_out)
    --api               (Optional) Attempt to call LLM API (requires env vars)

Dependencies:
    None (Standard Library only for prompt generation)
    openai/anthropic (if --api is used)
"""

import os
import argparse
import glob
import json

# --- Configuration ---
PERSONA_FILE = "../personas/validator.md"
DOMAIN_RULE_FILE = "../../../.validation/checks/domain-04-technology.md"

def load_file(path):
    try:
        with open(path, 'r', encoding='utf-8') as f:
            return f.read()
    except FileNotFoundError:
        print(f"Error: Could not find file {path}")
        return ""

def generate_prompt(content, filename, validator_persona, domain_rules):
    """Constructs the full prompt for the LLM."""

    prompt = f"""
{validator_persona}

---

# INSTRUCTIONS

You are running a validation check on the file: **{filename}**

## RULES TO APPLY
{domain_rules}

## CONTENT TO CHECK
```markdown
{content}
```

## YOUR TASK
Generate a **Validation Report** for this file.
1. Check for Domain 4 violations (Precision, Vocabulary, Verbs).
2. Check for Structural issues (Frontmatter, missing sections).
3. Output PASS/FAIL and specific line-by-line violations.
"""
    return prompt

def main():
    parser = argparse.ArgumentParser(description="AI Validation Generator")
    parser.add_argument('--dir', default='docs', help="Directory to scan")
    parser.add_argument('--output', default='prompts_out', help="Output directory")
    args = parser.parse_args()

    # Resolve paths relative to script location if running from elsewhere
    script_dir = os.path.dirname(os.path.abspath(__file__))

    # Load Context
    validator_path = os.path.join(script_dir, PERSONA_FILE)
    domain_path = os.path.join(script_dir, DOMAIN_RULE_FILE)

    validator_persona = load_file(validator_path)
    domain_rules = load_file(domain_path)

    if not validator_persona or not domain_rules:
        print("Failed to load context files. Check paths.")
        return

    # Prepare Output Dir
    if not os.path.exists(args.output):
        os.makedirs(args.output)

    # Scan Files
    root_path = os.path.join(os.getcwd(), args.dir)
    print(f"Scanning {root_path}...")

    count = 0
    for root, dirs, files in os.walk(root_path):
        for file in files:
            if file.endswith(".md"):
                filepath = os.path.join(root, file)
                content = load_file(filepath)

                # Generate Prompt
                prompt = generate_prompt(content, file, validator_persona, domain_rules)

                # Save to file
                out_filename = f"validate_{file}_{count}.txt"
                out_path = os.path.join(args.output, out_filename)

                with open(out_path, 'w', encoding='utf-8') as f:
                    f.write(prompt)

                count += 1

    print(f"Generated {count} validation prompts in '{args.output}'.")
    print("You can now feed these text files to Claude/Gemini manually or via CLI.")

if __name__ == "__main__":
    main()
