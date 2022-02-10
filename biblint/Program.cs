// See https://aka.ms/new-console-template for more information
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using biblint;

public class Program
{
    public static void Main(string[] argv)
    {
        AntlrInputStream inputStream = new AntlrFileStream(argv[0]);

        BibErrorListener errorListener = new();

        BibLexer lexer = new BibLexer(inputStream);
        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(errorListener);

        CommonTokenStream tokenStream = new CommonTokenStream(lexer);
        BibParser parser = new BibParser(tokenStream);
        parser.RemoveErrorListeners();
        parser.AddErrorListener(errorListener);

        BibParser.FileContext? file = parser.file();

        if (errorListener.LexerErrors.Any() || errorListener.ParserErrors.Any())
        {
            foreach (var error in errorListener.LexerErrors) Console.Write($"{error.Print()}\n");
            foreach (var error in errorListener.ParserErrors) Console.Write($"{error.Print()}\n");
            return;
        }

        BibFileListener listener = new();
        ParseTreeWalker walker = new();
        walker.Walk(listener, file);

        foreach (var entry in listener.entries.OrderBy(entry => entry.Id))
        {
            Console.Write(entry.PrettyPrint());
        }
    }
}
