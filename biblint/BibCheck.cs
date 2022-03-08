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

public class Checks
{
    public static List<BibCheck> List = new()
    {
        new QuoteCheck(),
        new DuplicateKeyCheck(),
    };
}