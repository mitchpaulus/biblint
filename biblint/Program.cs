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

        List<string> toRemove = new();

        List<string> files = new();

        for (var i = 0; i < argv.Length; i++)
        {
            var arg = argv[i];

            if (arg == "-h" || arg == "--help")
            {
                Console.Write("Usage: biblint [options] [file]\n");
                Console.Write("Options:\n");
                Console.Write("  -h, --help    Show this help message and exit\n");
                Console.Write("  -v, --version Show version information and exit\n");
                return;
            }
            else if (arg == "-f" || arg == "--format")
            {
                format = true;
            }
            else if (arg == "-r" || arg == "--remove")
            {
                if (i + 1 >= argv.Length)
                    throw new InvalidOperationException("Not enough arguments provided to remove option");
                toRemove.Add(argv[i + 1]);
                i++;
            }
            else if (arg == "-s" || arg == "--sort")
            {
                sort = true;
            }
            else if (arg == "-v" || arg == "--version")
            {
                Console.Write("0.1\n");
                return;
            }
            else
            {
                files.Add(arg);
            }
        }

        if (files.Count == 0)
        {
            files.Add("-");
        }

        foreach (string file in files)
        {
            if (file == "-")
            {
                using Stream standardInputStream = Console.OpenStandardInput();
                Process(standardInputStream, format, sort, toRemove);
            }
            else
            {
                using FileStream stream = new(file, FileMode.Open);
                Process(stream, format, sort, toRemove);
            }
        }
    }

    public static void Process(Stream stream, bool format, bool sort, List<string> toRemove)
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

        var warnings = Checks.List.SelectMany(check => check.Check(listener.entries)).OrderBy(warning => warning.Line).ToList();
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
