using System.Text.RegularExpressions;

namespace biblint;

public interface BibCheck
{
    List<BibWarning> Check(List<BibEntry> entries);
}


public class QuoteCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        foreach (BibField bibField in entries.SelectMany(entry => entry.Fields))
        {
            if (Regex.IsMatch(bibField.FieldValue, "[^\\\\]\""))
            {
                BibWarning warning = new($"The field value for {bibField.FieldName} contains a quote character, replace with `` and ''.", bibField.FieldContext.Start.Line);
                warnings.Add(warning);
            }
        }

        return warnings;
    }
}

public class DuplicateKeyCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        Dictionary<string, List<BibEntry>> entryMapping = new();
        foreach (var entry in entries)
        {
            var lowerId = entry.Id.ToLowerInvariant();
            if (! entryMapping.ContainsKey(lowerId))
            {
                entryMapping[lowerId] = new List<BibEntry>();
            }

            entryMapping[lowerId].Add(entry);
        }

        foreach (var key in entryMapping.Keys)
        {
            if (entryMapping[key].Count > 1)
            {
                entries = entryMapping[key];
                IEnumerable<int> lines = entries.Select(entry => entry.Context.Start.Line);
                warnings.Add(new BibWarning($"Duplicate key '{key}' on lines {string.Join(", ", lines)}.", lines.First()));
            }
        }

        return warnings;
    }
}

public class IssnFormatCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        foreach (var field in entries.Fields().Where(field => field.Match("issn")))
        {
            string trimmedValue = field.FieldValue.StripBraces().Trim();
            if (!Regex.IsMatch(trimmedValue, "^[0-9]{4}-[0-9]{4}$"))
            {
                warnings.Add(new BibWarning($"The ISSN number '{trimmedValue}' is in non standard format (xxxx-xxxx).", field.FieldContext.Start.Line));
            }
        }

        return warnings;
    }
}

public class Checks
{
    public static List<BibCheck> List = new()
    {
        new QuoteCheck(),
        new DuplicateKeyCheck(),
        new IssnFormatCheck(),
    };
}

public static class Extensions
{
    public static List<BibField> Fields(this List<BibEntry> entries) =>
        entries.SelectMany(entry => entry.Fields).ToList();

    public static string StripBraces(this string input)
    {
        if (input.Length < 2) return input;
        if (input[0] == '{' && input[^1] == '}') return input.Substring(1, input.Length - 2);
        return input;
    }
}