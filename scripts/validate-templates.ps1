#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates all capture template JSON files against the schema.
.DESCRIPTION
    This script checks all JSON files in data/capture-templates/categories/
    for structural validity, required fields, and Domain 4 compliance.
.PARAMETER SchemaPath
    Path to the JSON schema file.
.PARAMETER TemplatesPath
    Path to the directory containing template JSON files.
.EXAMPLE
    pwsh scripts/validate-templates.ps1
.NOTES
    v0.3.25a: Initial implementation for externalized capture templates.
#>

param(
    [string]$SchemaPath = "data/capture-templates/schema/capture-template.schema.json",
    [string]$TemplatesPath = "data/capture-templates/categories"
)

Write-Host "=== Capture Template Validator ===" -ForegroundColor Cyan
Write-Host "v0.3.25a - Rune & Rust" -ForegroundColor Gray
Write-Host ""

# Valid CaptureType enum values
$ValidTypes = @(
    "TextFragment",
    "EchoRecording",
    "VisualRecord",
    "Specimen",
    "OralHistory",
    "RunicTrace"
)

# Domain 4 violation patterns (precision measurements)
$Domain4Patterns = @(
    '\d+%',           # Percentages (e.g., "95%")
    '\d+\.\d+',       # Decimal numbers (e.g., "18.5")
    '\d+\s*(meters?|km|kilometers?|miles?|feet|ft)', # Distance units
    '\d+\s*(degrees?|C|F|celsius|fahrenheit)',       # Temperature
    '\d+\s*(seconds?|minutes?|hours?|days?|years?)', # Precise time
    '\d+\s*(ppm|dB|Hz|kg|lbs?|pounds?)',             # Technical units
    '\d{4,}',         # Large precise numbers (populations, etc.)
    'API|CPU|GPU|RAM|ROM|HTTP|URL'  # Modern tech acronyms
)

# Check if schema exists
if (-not (Test-Path $SchemaPath)) {
    Write-Error "Schema not found: $SchemaPath"
    exit 1
}

$schema = Get-Content $SchemaPath -Raw | ConvertFrom-Json
Write-Host "Loaded schema: $SchemaPath" -ForegroundColor Green

# Get all template files
$templateFiles = Get-ChildItem -Path $TemplatesPath -Filter "*.json" -ErrorAction SilentlyContinue

if ($null -eq $templateFiles -or $templateFiles.Count -eq 0) {
    Write-Error "No template files found in: $TemplatesPath"
    exit 1
}

$errors = @()
$warnings = @()
$templateCount = 0
$allTemplateIds = @()

foreach ($file in $templateFiles) {
    Write-Host "`nValidating: $($file.Name)" -ForegroundColor Yellow

    try {
        $content = Get-Content $file.FullName -Raw | ConvertFrom-Json

        # Check required top-level fields
        if (-not $content.'$schema') {
            $errors += "$($file.Name): Missing '`$schema' field"
        }
        if (-not $content.category) {
            $errors += "$($file.Name): Missing 'category' field"
        }
        if (-not $content.version) {
            $errors += "$($file.Name): Missing 'version' field"
        }
        if (-not $content.templates -or $content.templates.Count -eq 0) {
            $errors += "$($file.Name): Missing or empty 'templates' array"
            continue
        }

        # Check each template
        foreach ($template in $content.templates) {
            $templateCount++

            # Required fields
            if (-not $template.id) {
                $errors += "$($file.Name): Template missing 'id'"
                continue
            }

            # Check for duplicate IDs
            if ($allTemplateIds -contains $template.id) {
                $errors += "$($file.Name): Duplicate template ID '$($template.id)'"
            }
            $allTemplateIds += $template.id

            # Check ID format (kebab-case)
            if ($template.id -notmatch '^[a-z][a-z0-9-]*$') {
                $errors += "$($file.Name): Template '$($template.id)' has invalid ID format (must be kebab-case)"
            }

            if (-not $template.type) {
                $errors += "$($file.Name): Template '$($template.id)' missing 'type'"
            }
            elseif ($ValidTypes -notcontains $template.type) {
                $errors += "$($file.Name): Template '$($template.id)' has invalid type '$($template.type)'. Valid types: $($ValidTypes -join ', ')"
            }

            if (-not $template.fragmentContent) {
                $errors += "$($file.Name): Template '$($template.id)' missing 'fragmentContent'"
            }
            elseif ($template.fragmentContent.Length -lt 20) {
                $errors += "$($file.Name): Template '$($template.id)' fragmentContent too short ($($template.fragmentContent.Length) chars, minimum 20)"
            }
            elseif ($template.fragmentContent.Length -gt 1000) {
                $errors += "$($file.Name): Template '$($template.id)' fragmentContent too long ($($template.fragmentContent.Length) chars, maximum 1000)"
            }

            if (-not $template.source) {
                $errors += "$($file.Name): Template '$($template.id)' missing 'source'"
            }
            elseif ($template.source.Length -lt 5) {
                $errors += "$($file.Name): Template '$($template.id)' source too short ($($template.source.Length) chars, minimum 5)"
            }

            # Domain 4 compliance check
            foreach ($pattern in $Domain4Patterns) {
                if ($template.fragmentContent -match $pattern) {
                    $match = [regex]::Match($template.fragmentContent, $pattern).Value
                    $warnings += "$($file.Name): Template '$($template.id)' may violate Domain 4 (found '$match')"
                    break  # Only report first violation per template
                }
            }

            # Quality range check
            if ($null -ne $template.quality) {
                if ($template.quality -lt 1 -or $template.quality -gt 100) {
                    $errors += "$($file.Name): Template '$($template.id)' quality out of range ($($template.quality), must be 1-100)"
                }
            }
        }

        Write-Host "  OK $($content.templates.Count) templates found" -ForegroundColor Green

    }
    catch {
        $errors += "$($file.Name): JSON parse error - $($_.Exception.Message)"
    }
}

# Summary
Write-Host "`n=== Summary ===" -ForegroundColor Cyan
Write-Host "Total template files: $($templateFiles.Count)"
Write-Host "Total templates: $templateCount"

if ($warnings.Count -gt 0) {
    Write-Host "`nWarnings: $($warnings.Count)" -ForegroundColor Yellow
    foreach ($warn in $warnings) {
        Write-Host "  ! $warn" -ForegroundColor Yellow
    }
}

if ($errors.Count -gt 0) {
    Write-Host "`nErrors: $($errors.Count)" -ForegroundColor Red
    foreach ($err in $errors) {
        Write-Host "  X $err" -ForegroundColor Red
    }
    exit 1
}
else {
    Write-Host "`nOK All templates valid!" -ForegroundColor Green
    exit 0
}
