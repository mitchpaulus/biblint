using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace biblint;

public class BibErrorListener : IAntlrErrorListener<int>, IAntlrErrorListener<IToken>
{
    public readonly List<LexerError> LexerErrors = new();
    public readonly List<ParserError> ParserErrors = new();

    public void SyntaxError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        LexerErrors.Add(new LexerError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e));
    }

    public void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        ParserErrors.Add(new ParserError(output, recognizer, offendingSymbol, line, charPositionInLine, msg, e));
    }
}

public class LexerError : IBibError
{
    public TextWriter Output { get; }
    public IRecognizer Recognizer { get; }
    public  int OffendingSymbol { get; }
    public int Line { get;  }
    public int CharPositionInLine { get; }
    public string Msg { get; }
    public RecognitionException E { get; }

    public LexerError(TextWriter output, IRecognizer recognizer, int offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Output = output;
        Recognizer = recognizer;
        OffendingSymbol = offendingSymbol;
        Line = line;
        CharPositionInLine = charPositionInLine;
        Msg = msg;
        E = e;
    }

    public string Print()
    {
        return $"{Line}:{CharPositionInLine}: {Msg}";
    }
}

public interface IBibError
{
    string Print();
}

public class ParserError : IBibError
{
    public TextWriter Output { get; }
    public IRecognizer Recognizer { get; }
    public IToken OffendingSymbol { get; }
    public int Line { get;  }
    public int CharPositionInLine { get; }
    public string Msg { get; }
    public RecognitionException E { get; }

    public ParserError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
    {
        Output = output;
        Recognizer = recognizer;
        OffendingSymbol = offendingSymbol;
        Line = line;
        CharPositionInLine = charPositionInLine;
        Msg = msg;
        E = e;
    }

    public string Print()
    {
        return $"{Line}:{CharPositionInLine}: {Msg}";
    }
}
