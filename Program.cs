string path = ReadValidDirectoryPath();

Console.Write("Enter text to search: ");
string? input = Console.ReadLine();

if (string.IsNullOrWhiteSpace(input))
{
    throw new ArgumentException("Search text cannot be empty.", nameof(input));
}

var options = new EnumerationOptions
{
    RecurseSubdirectories = true,
    IgnoreInaccessible = true
};

var files = Directory.EnumerateFiles(path, searchPattern: "*", options);
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

foreach (var file in files)
{
    if (!textExtensions.Contains(Path.GetExtension(file)))
    {
        continue;
    }

    var matches = new List<(int LineNumber, int MatchCount, string Snippet)>();
    string[] lines;

    try
    {
        lines = File.ReadAllLines(file);
    }
    catch (UnauthorizedAccessException)
    {
        continue;
    }
    catch (IOException)
    {
        continue;
    }

    for (int i = 0; i < lines.Length; i++)
    {
        int matchCount = CountMatches(lines[i], input);

        if (matchCount > 0)
        {
            string snippet = CreateSnippet(lines[i], input);
            matches.Add((i + 1, matchCount, snippet));
        }
    }

    if (matches.Count == 0)
    {
        continue;
    }

    int totalMatches = matches.Sum(match => match.MatchCount);
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
        Console.WriteLine($"    {match.Snippet}");
    }
}

Console.WriteLine();
Console.WriteLine("Search summary");
Console.WriteLine("--------------");
Console.WriteLine($"Files with matches: {filesWithMatches}");
Console.WriteLine($"Total matches: {totalMatchesFound}");

static int CountMatches(string text, string searchText)
{
    int count = 0;
    int index = 0;

    while ((index = text.IndexOf(searchText, index, StringComparison.OrdinalIgnoreCase)) >= 0)
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

static string? NormalizeDirectoryPath(string? path)
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return path;
    }

    path = path.Trim();

    if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == ':')
    {
        return path + Path.DirectorySeparatorChar;
    }

    return path;
}

static string CreateSnippet(string text, string searchText)
{
    const int contextLength = 40;
    int matchIndex = text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase);

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
