# File Search Engine

A small C# console app for searching text inside readable files in a folder.

The app asks for a directory path and search text, then scans supported text-based files recursively. Matching results are grouped by file and include the matching line number, match count, highlighted snippet, total matches, and search time.

## Features

- Searches inside files, not just file names
- Scans subdirectories
- Skips inaccessible files and folders
- Skips common build, dependency, cache, and IDE folders
- Ignores non-text formats
- Shows matching files, line numbers, highlighted snippets, total matches, and search time
- Searches case-insensitively
- Matches whole search text only, so `pawn` does not match `spawn`
- Allows another search before exiting
- Supports quoted drive input like `"D:"`

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

You can also enter a drive like:

```text
D:
"D:"
```

Both are treated as `D:\`.

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

Press Enter to search another word, or any other key to exit...
```

## Standalone EXE

To create a Windows executable that works without installing .NET:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The executable is created in:

```text
bin\Release\net10.0\win-x64\publish\
```

## License

MIT
