// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using biblint;

public class Program
{
    public static void Main(string[] argv)
    {
        bool format = false;
        bool sort = false;
        bool checkUrls = false;

        List<string> toRemove = new();

        List<string> files = new();

        for (var i = 0; i < argv.Length; i++)
        {
            var arg = argv[i];

            if (arg is "-h" or "--help")
            {
                Console.Write("Usage: biblint [options] [file]\n");
                Console.Write("Options:\n");
                Console.Write("  -f, --format         Apply formatting.\n");
                Console.Write("  -h, --help           Show this help message and exit.\n");
                Console.Write("  -r, --remove <field> Remove the named field from the output. Can be used multiple times.\n");
                Console.Write("  -s, --sort           Sort the output by the entry id.\n");
                Console.Write("  -v, --version        Show version information and exit.\n");
                Console.Write("  -u, --urls           Check URLs");
                Console.Write("\n");
                Console.Write("If no file is specified, or the file is '-', the input is read from standard input.\n");

                return;
            }
            else if (arg is "-f" or "--format")
            {
                format = true;
            }
            else if (arg is "-r" or "--remove")
            {
                if (i + 1 >= argv.Length)
                    throw new InvalidOperationException("Not enough arguments provided to remove option");
                toRemove.Add(argv[i + 1]);
                i++;
            }
            else if (arg is "-s" or "--sort")
            {
                sort = true;
            }
            else if (arg is "-v" or "--version")
            {
                Console.Write("0.1\n");
                return;
            }
            else if (arg is "-u" or "--urls")
            {
                checkUrls = true;
            }
            else
            {
                files.Add(arg);
            }
        }

        if (files.Count == 0)
        {
            // Search for any files with the extension .bib
            files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bib").ToList();
        }

        foreach (string file in files)
        {
            if (file == "-")
            {
                using Stream standardInputStream = Console.OpenStandardInput();
                Process(standardInputStream, format, sort, toRemove, checkUrls);
            }
            else
            {
                try
                {
                    using FileStream stream = new(file, FileMode.Open);
                    Process(stream, format, sort, toRemove, checkUrls);
                }
                catch (ArgumentException)
                {
                    Console.Error.Write($"Could not open file '{file}'. Are there illegal characters in the name?\n");
                    Environment.ExitCode = 1;
                    return;
                }
                catch (NotSupportedException)
                {
                    Console.Error.Write($"Could not open file '{file}'. Path possibly refers to non-file device in non-NTFS environment.\n");
                    Environment.ExitCode = 1;
                    return;
                }
                catch (FileNotFoundException)
                {
                    Console.Error.Write($"Could not find file '{file}'.\n");
                    Environment.ExitCode = 1;
                    return;
                }
                catch (Exception)
                {
                    Console.Error.Write($"Error opening file '{file}'.\n");
                    Environment.ExitCode = 1;
                    return;
                }
            }
        }
    }

    public static void Process(Stream stream, bool format, bool sort, List<string> toRemove, bool checkUrls)
    {
        AntlrInputStream inputStream = new(stream);

        BibErrorListener errorListener = new();

        BibLexer lexer = new(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        CommonTokenStream tokenStream = new(lexer);
        BibParser parser = new(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        BibParser.FileContext? file = parser.file();

        if (errorListener.LexerErrors.Any() || errorListener.ParserErrors.Any())
        {
            foreach (var error in errorListener.LexerErrors)  Console.Error.Write($"{error.Print()}\n");
            foreach (var error in errorListener.ParserErrors) Console.Error.Write($"{error.Print()}\n");
            return;
        }

        BibFileListener listener = new(toRemove);
        ParseTreeWalker walker = new();
        walker.Walk(listener, file);

        var warnings = Checks.List(checkUrls).SelectMany(check => check.Check(listener.entries)).OrderBy(warning => warning.Line).ToList();
        foreach (var warning in warnings) Console.Error.Write($"{warning.Line}: {warning.Warning}\n");

        if (sort) listener.entries.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.CurrentCultureIgnoreCase));

        if (format)
        {
            foreach (var entry in listener.entries)
            {
                Console.Write(entry.PrettyPrint());
            }
        }
    }
}
