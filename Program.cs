string path = ReadValidDirectoryPath();
string input = ReadValidSearchText();

var options = new EnumerationOptions
{
    IgnoreInaccessible = true
};

var skippedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".cache",
    ".gradle",
    ".git",
    ".github",
    ".idea",
    ".next",
    ".nuget",
    ".pnpm-store",
    ".pytest_cache",
    ".svn",
    ".turbo",
    ".vscode",
    ".vs",
    "__pycache__",
    "bin",
    "bower_components",
    "build",
    "coverage",
    "debug",
    "dist",
    "logs",
    "obj",
    "node_modules",
    "out",
    "packages",
    "release",
    "target",
    "temp",
    "tmp",
    "vendor"
};
var files = EnumerateSearchFiles(path, options, skippedDirectories);
var textExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
{
    ".txt",
    ".md",
    ".csv",
    ".json",
    ".xml",
    ".html",
    ".css",
    ".js",
    ".ts",
    ".cs",
    ".csproj",
    ".sln",
    ".config",
    ".log"
};
int totalMatchesFound = 0;
int filesWithMatches = 0;
object consoleLock = new();
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

Parallel.ForEach(files, file =>
{
    if (!textExtensions.Contains(Path.GetExtension(file)))
    {
        return;
    }

    var matches = new List<(int LineNumber, int MatchCount, string Snippet)>();
    int lineNumber = 0;

    try
    {
        foreach (var line in File.ReadLines(file))
        {
            lineNumber++;
            int matchCount = CountMatches(line, input);

            if (matchCount > 0)
            {
                string snippet = CreateSnippet(line, input);
                matches.Add((lineNumber, matchCount, snippet));
            }
        }
    }
    catch (UnauthorizedAccessException)
    {
        return;
    }
    catch (IOException)
    {
        return;
    }

    if (matches.Count == 0)
    {
        return;
    }

    int totalMatches = matches.Sum(match => match.MatchCount);

    lock (consoleLock)
    {
        totalMatchesFound += totalMatches;
        filesWithMatches++;

        Console.WriteLine();
        Console.WriteLine($"Directory: {Path.GetDirectoryName(file)}");
        Console.WriteLine($"File:      {Path.GetFileName(file)}");
        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Matches: {totalMatches}");
        Console.WriteLine("Lines:");

        foreach (var match in matches)
        {
            Console.WriteLine($"  Line {match.LineNumber} ({match.MatchCount} match{FormatPlural(match.MatchCount)}):");
            Console.Write("    ");
            WriteHighlightedText(match.Snippet, input);
            Console.WriteLine();
        }
    }
});

stopwatch.Stop();

Console.WriteLine();
Console.WriteLine("Search summary");
Console.WriteLine("--------------");
Console.WriteLine($"Files with matches: {filesWithMatches}");
Console.WriteLine($"Total matches: {totalMatchesFound}");
Console.WriteLine($"Search time: {stopwatch.Elapsed.TotalSeconds:0.000} seconds");
Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static int CountMatches(string text, string searchText)
{
    int count = 0;
    int index = 0;

    while ((index = FindWholeTextMatch(text, searchText, index)) >= 0)
    {
        count++;
        index += searchText.Length;
    }

    return count;
}

static string FormatPlural(int count)
{
    return count == 1 ? "" : "es";
}

static IEnumerable<string> EnumerateSearchFiles(
    string rootPath,
    EnumerationOptions options,
    HashSet<string> skippedDirectories)
{
    var directories = new Stack<string>();
    directories.Push(rootPath);

    while (directories.Count > 0)
    {
        string currentDirectory = directories.Pop();

        IEnumerable<string> files;
        IEnumerable<string> childDirectories;

        try
        {
            files = Directory.EnumerateFiles(currentDirectory, "*", options);
            childDirectories = Directory.EnumerateDirectories(currentDirectory, "*", options);
        }
        catch (UnauthorizedAccessException)
        {
            continue;
        }
        catch (IOException)
        {
            continue;
        }

        foreach (var file in files)
        {
            yield return file;
        }

        foreach (var childDirectory in childDirectories)
        {
            string directoryName = Path.GetFileName(childDirectory);

            if (!skippedDirectories.Contains(directoryName))
            {
                directories.Push(childDirectory);
            }
        }
    }
}

static string ReadValidDirectoryPath()
{
    while (true)
    {
        Console.Write("Enter a directory path: ");
        string? path = NormalizeDirectoryPath(Console.ReadLine());

        if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
        {
            return path;
        }

        Console.WriteLine("Directory does not exist. Please try again.");
    }
}

static string ReadValidSearchText()
{
    while (true)
    {
        Console.Write("Enter text to search: ");
        string? input = Console.ReadLine();

        if (!string.IsNullOrWhiteSpace(input))
        {
            return input.Trim();
        }

        Console.WriteLine("Search text cannot be empty. Please try again.");
    }
}

static string? NormalizeDirectoryPath(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return path;
    }

    path = path.Trim().Trim('"', '\'');

    if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == ':')
    {
        return path + Path.DirectorySeparatorChar;
    }

    return path;
}

static string CreateSnippet(string text, string searchText)
{
    const int contextLength = 40;
    int matchIndex = FindWholeTextMatch(text, searchText, 0);

    if (matchIndex < 0)
    {
        return text;
    }

    int start = Math.Max(0, matchIndex - contextLength);
    int end = Math.Min(text.Length, matchIndex + searchText.Length + contextLength);
    string prefix = start > 0 ? "..." : "";
    string suffix = end < text.Length ? "..." : "";

    return $"{prefix}{text[start..end].Trim()}{suffix}";
}

static int FindWholeTextMatch(string text, string searchText, int startIndex)
{
    int index = startIndex;

    while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) >= 0)
    {
        if (HasWordBoundary(text, index, searchText.Length))
        {
            return index;
        }

        index += searchText.Length;
    }

    return -1;
}

static bool HasWordBoundary(string text, int matchIndex, int matchLength)
{
    int beforeIndex = matchIndex - 1;
    int afterIndex = matchIndex + matchLength;

    bool startsAtWordBoundary = beforeIndex < 0 || !IsWordCharacter(text[beforeIndex]);
    bool endsAtWordBoundary = afterIndex >= text.Length || !IsWordCharacter(text[afterIndex]);

    return startsAtWordBoundary && endsAtWordBoundary;
}

static bool IsWordCharacter(char character)
{
    return char.IsLetterOrDigit(character) || character == '_';
}

static void WriteHighlightedText(string text, string searchText)
{
    int index = 0;
    ConsoleColor originalColor = Console.ForegroundColor;

    while (index < text.Length)
    {
        int matchIndex = FindWholeTextMatch(text, searchText, index);

        if (matchIndex < 0)
        {
            Console.Write(text[index..]);
            break;
        }

        Console.Write(text[index..matchIndex]);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write(text.AsSpan(matchIndex, searchText.Length));
        Console.ForegroundColor = originalColor;

        index = matchIndex + searchText.Length;
    }
}
