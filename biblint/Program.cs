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
                Process(standardInputStream, format, sort);
            }
            else
            {
                using FileStream stream = new(file, FileMode.Open);
                Process(stream, format, sort);
            }
        }
    }

    public static void Process(Stream stream, bool format, bool sort)
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

        BibFileListener listener = new();
        ParseTreeWalker walker = new();
        walker.Walk(listener, file);

        if (sort) listener.entries.Sort((a, b) => string.Compare(a.Id, b.Id, StringComparison.Ordinal));

        if (format)
        {
            foreach (var entry in listener.entries)
            {
                Console.Write(entry.PrettyPrint());
            }
        }
    }
}
