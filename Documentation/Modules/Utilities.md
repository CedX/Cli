# TODO Utilities Module

The Utilities module contains general-purpose functions and validation tools used throughout the CLI.

## Commands

### ConvertTo-Encoding

Converts text file encoding.

```powershell
# Convert to UTF-8
ConvertTo-Encoding -Path "file.txt" -Encoding "UTF-8" -OutputPath "file_utf8.txt"

# Convert multiple files
Get-ChildItem "*.txt" | ConvertTo-Encoding -Encoding "UTF-8"
```

**Parameters:**
- `-Path`: Input file path
- `-Encoding`: Target encoding (UTF-8, ASCII, etc.)
- `-OutputPath`: Output file path (optional, defaults to overwriting input)

## Usage Examples

### File Processing Pipeline

```powershell
# Convert encoding and backup
Get-ChildItem "src" -Filter "*.cs" | ForEach-Object {
    $backupPath = Join-Path "backup" $_.Name
    ConvertTo-Encoding -Path $_.FullName -Encoding "UTF-8" -OutputPath $backupPath
}
```
