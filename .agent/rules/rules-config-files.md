---
trigger: always_on
---

# Configuration File Rules

## 1.1 JSON Configuration
- Store configuration in `config/` directory
- Use kebab-case for file names (e.g., `abilities.json`, `damage-types.json`)
- Provide JSON schemas where applicable (e.g., `abilities.schema.json`)
- Use lowercase kebab-case for IDs (e.g., `"shield-bash"`, `"flame-bolt"`)

## 1.2 Configuration Structure

```json
{
  "abilities": [
    {
      "id": "shield-bash",
      "name": "Shield Bash",
      "description": "Slam your shield into the enemy...",
      "classIds": ["warrior", "paladin"],
      "cost": { "resourceTypeId": "rage", "amount": 15 },
      "cooldown": 2,
      "effects": [...]
    }
  ]
}
```