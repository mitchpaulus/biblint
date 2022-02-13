using System.Text;
using Antlr4.Runtime.Tree;

namespace biblint;

public class BibFileListener : BibParserBaseListener
{
    public List<BibEntry> entries = new();
    public override void EnterBibentry(BibParser.BibentryContext context)
    {
        string? entryType = context.WORD(0).GetText();
        string id = context.WORD(1).GetText();
        List<BibField> fields = context.fields().field().Select(BibFieldFromContext).ToList();
        entries.Add(new BibEntry(id, entryType, fields));
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

        return new BibField(fieldName, fieldValue);
    }
}

public class BibEntry
{
    public string Id { get; }
    public string EntryType { get; }
    public List<BibField> Fields { get; }

    public BibEntry(string id, string entryType, List<BibField> fields)
    {
        Id = id;
        EntryType = entryType;
        Fields = fields;
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

public class BibField
{
    public string FieldName { get; }
    public string FieldValue { get; }

    public BibField(string fieldName, string fieldValue)
    {
        FieldName = fieldName;
        FieldValue = fieldValue;
    }

    public string PrettyPrint()
    {
        return $"{FieldName.ToLower()} = {FieldValue}";
    }
}