# File Search Engine

A small C# console app for searching text inside readable files in a folder.

The app asks for a directory path and search text, then scans supported text-based files recursively. Matching results are grouped by file and include the matching line number, match count, and a short snippet.

## Features

- Searches inside files, not just file names
- Scans subdirectories
- Skips inaccessible files and folders
- Ignores non-text formats
- Shows matching files, line numbers, snippets, total matches, and search time
- Searches case-insensitively

## Supported File Types

Currently searched extensions:

```text
.txt, .md, .csv, .json, .xml, .html, .css, .js, .ts,
.cs, .csproj, .sln, .config, .log
```

## Requirements

- .NET 10 SDK

## Run

From the project folder:

```bash
dotnet run
```

Then enter:

```text
Enter a directory path: C:\example\folder
Enter text to search: hello
```

## Example Output

```text
Directory: C:\example\folder
File:      notes.txt
--------------------------------------------------------------------------------
Matches: 2
Lines:
  Line 4 (1 match):
    ...this is a short snippet with hello inside...
  Line 12 (1 match):
    ...another line where hello appears...

Search summary
--------------
Files with matches: 1
Total matches: 2
Search time: 0.042 seconds
```
