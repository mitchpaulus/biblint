using System.Text;
using System.Text.RegularExpressions;
using Antlr4.Runtime.Tree;

namespace biblint;

public class BibFileListener : BibParserBaseListener
{
    private readonly HashSet<string> _toRemove;
    public List<BibEntry> entries = new();

    public List<BibWarning> BibWarnings { get; } = new();

    public BibFileListener(List<string> toRemove)
    {
        _toRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {  };
        foreach (var item in toRemove) _toRemove.Add(item);
    }

    public override void EnterBibentry(BibParser.BibentryContext context)
    {
        string? entryType = context.WORD(0).GetText();
        string id = context.WORD(1).GetText();
        List<BibField> fields = context.fields().field().Select(BibFieldFromContext).Where(bibField => !_toRemove.Contains(bibField.FieldName)).ToList();
        entries.Add(new BibEntry(id, entryType, fields, context));
    }

    public BibField BibFieldFromContext(BibParser.FieldContext fieldContext)
    {
        var fieldName = fieldContext.WORD().GetText();

        string fieldValue;

        var valueContext = fieldContext.value();

        if (valueContext.FIELD_VALUE_WORD() != null)
        {
            fieldValue = valueContext.FIELD_VALUE_WORD().GetText();
        }
        else if (valueContext.TEXCONTENT() != null)
        {
            fieldValue = valueContext.TEXCONTENT().GetText();
        }
        else
        {
            throw new NotImplementedException("The context for the field value has not been implemented.");
        }

        return new BibField(fieldName, fieldValue, fieldContext);
    }
}

public class BibEntry
{
    public string Id { get; }
    public string EntryType { get; }
    public List<BibField> Fields { get; }

    public BibParser.BibentryContext Context { get; }
    public BibEntry(string id, string entryType, List<BibField> fields, BibParser.BibentryContext context)
    {
        Id = id;
        EntryType = entryType;
        Fields = fields;
        Context = context;
    }

    public string PrettyPrint()
    {
        StringBuilder builder = new();
        builder.Append($"@{EntryType.ToLower()}{{{Id},\n");
        foreach (BibField field in Fields) builder.Append($"  {field.PrettyPrint()},\n");
        builder.Append("}\n");
        return builder.ToString();
    }

}

public class BibWarning
{
    public int Line { get;  }
   public string Warning { get; }

   public BibWarning(string warning, int line)
   {
       Line = line;
       Warning = warning;
   }
}

public class BibField
{
    public string FieldName { get; }
    public string FieldValue { get; }
    public BibParser.FieldContext FieldContext { get; }

    public BibField(string fieldName, string fieldValue, BibParser.FieldContext fieldContext)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
        FieldContext = fieldContext;
    }

    public bool Match(string fieldName) => string.Equals(FieldName, fieldName, StringComparison.InvariantCultureIgnoreCase);

    public string PrettyPrint()
    {
        return $"{FieldName.ToLower()} = {FieldValue}";
    }
}