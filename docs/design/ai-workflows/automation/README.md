# AI Workflow Automation

This directory contains scripts to automate the interaction with AI models for the Rune & Rust project.

## `validate.py`

This script generates validation prompts for every Markdown file in a specified directory. It packages the file content with the **Validator Persona** and **Domain 4 Rules** into a single text file that can be pasted into an LLM.

### Usage

1.  Navigate to this directory.
2.  Run the script:

```bash
python validate.py --dir ../../docs --output ./generated_prompts
```

3.  The script will create a `.txt` file for every document found.
4.  **Action:** Copy the content of a generated text file and paste it into Claude/Gemini.
5.  **Result:** The AI will act as the "Validator" and produce a compliance report.

### Future Expansion

*   Add `--api` flag to directly call OpenAI/Anthropic APIs.
*   Add filtering to only scan changed files (git integration).
