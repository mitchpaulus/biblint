using System.Net;
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

public class PageRangeCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        var pageFields = entries.Fields().Where(field => field.FieldName.ToLower() == "pages");
        var notValidPageFields = pageFields.Where(field => !IsValidPageField(field.FieldValue));
        return notValidPageFields.Select(field => new BibWarning($"The field value for {field.FieldName} contains a invalid page range '{field.FieldValue}'.", field.FieldContext.Start.Line)).ToList();
    }

    public bool IsValidPageField(string content)
    {
        // Verify that trimmed field is either single integer or two integers separated by two hyphens
        string trimmedValue = content.StripBraces().Trim();
        if (Regex.IsMatch(trimmedValue, "^[0-9]+$"))
        {
            return true;
        }

        if (Regex.IsMatch(trimmedValue, "^[0-9]+--[0-9]+$"))
        {
            return true;
        }

        return false;
    }
}

public class UrlCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        foreach (var field in entries.Fields().Where(field => field.FieldName.ToLower() == "url"))
        {
            string trimmedValue = field.FieldValue.StripBraces().Trim();
            if (!Regex.IsMatch(trimmedValue, "^https?://"))
            {
                warnings.Add(new BibWarning($"The URL '{trimmedValue}' is not in a valid format.", field.FieldContext.Start.Line));
            }

            // Attempt to connect to the URL and check for 200 response. Use HttpClient. Follow redirects.
            using HttpClientHandler handler = new();
            handler.AllowAutoRedirect = true;
            using HttpClient client = new(handler);
            try
            {
                var response = client.GetAsync(trimmedValue).Result;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    warnings.Add(new BibWarning($"The URL '{trimmedValue}' returned a status code of {response.StatusCode}.", field.FieldContext.Start.Line));
                }
            }
            catch (Exception)
            {
                warnings.Add(new BibWarning($"The URL '{trimmedValue}' could not be accessed.", field.FieldContext.Start.Line));
            }
        }

        return warnings;
    }
}

public class DuplicateDoiUrl : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        foreach (var entry in entries)
        {
            var doiField = entry.Fields.FirstOrDefault(field => field.FieldName.ToLower() == "doi");
            var urlField = entry.Fields.FirstOrDefault(field => field.FieldName.ToLower() == "url");
            // Check if DOI and URL are both present and are the same. Canonicalize the DOI and URL to remove protocol and domain portion. Allow any subdomain.
            if (doiField != null && urlField != null)
            {
                var doi = doiField.FieldValue.StripBraces().Trim();
                var url = urlField.FieldValue.StripBraces().Trim();

                // Remove any start match for 'http.*doi.org/'
                var match = Regex.Match(doi, "^http.*doi.org/");
                if (match.Success)
                {
                    doi = doi.Substring(match.Length);
                }

                match = Regex.Match(url, "^http.*doi.org/");
                if (match.Success)
                {
                    url = url.Substring(match.Length);
                }

                if (doi.Equals(url, StringComparison.OrdinalIgnoreCase))
                {
                    warnings.Add(new BibWarning($"The DOI '{doi}' and URL '{url}' are the same.", doiField.FieldContext.Start.Line));
                }

            }


        }

        return warnings;
    }
}

public class NoDoiOrUrlCheck : BibCheck
{
    public List<BibWarning> Check(List<BibEntry> entries)
    {
        List<BibWarning> warnings = new();
        // Check entries that are not books
        foreach (var entry in entries.Where(entry => !entry.EntryType.Equals("book", StringComparison.OrdinalIgnoreCase)))
        {
            var doiField = entry.Fields.FirstOrDefault(field => field.FieldName.ToLower() == "doi");
            var urlField = entry.Fields.FirstOrDefault(field => field.FieldName.ToLower() == "url");
            if (doiField == null && urlField == null)
            {
                warnings.Add(new BibWarning($"The entry '{entry.Id}' does not have a DOI or URL.", entry.Context.Start.Line));
            }
        }

        return warnings;
    }
}

public class Checks
{
    public static List<BibCheck> List(bool checkUrls)
    {
        var list = new List<BibCheck>()
        {
            new QuoteCheck(),
            new DuplicateKeyCheck(),
            new IssnFormatCheck(),
            new PageRangeCheck(),
            new DuplicateDoiUrl(),
            new NoDoiOrUrlCheck(),
        };

        if (checkUrls) list.Add(new UrlCheck());
        return list;
    }
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
