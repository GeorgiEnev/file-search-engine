Console.Write("Enter a directory: ");
string dir = Console.ReadLine();

if (!Directory.Exists(dir))
{
    throw new DirectoryNotFoundException($"Directory does not exist: { dir }");
}

Console.Write("Enter text to search: ");
string input = Console.ReadLine();


