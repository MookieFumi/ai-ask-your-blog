using System.Text.RegularExpressions;

namespace AskYourBlog.Api.Ingestion;

// Removes MDX-only syntax (import/export lines) and standalone HTML tags outside code fences.
public static class MdxCleaner
{
    private static readonly Regex ImportExport = new(@"^(import|export)\s.*$", RegexOptions.Compiled);
    private static readonly Regex HtmlTag = new(@"</?[A-Za-z][^>\n]*>", RegexOptions.Compiled);

    public static string Clean(string body)
    {
        var output = new List<string>();
        var inCode = false;

        foreach (var line in body.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
                output.Add(line);
                continue;
            }

            if (inCode)
            {
                output.Add(line);
                continue;
            }

            if (ImportExport.IsMatch(line))
            {
                continue;
            }

            output.Add(HtmlTag.Replace(line, ""));
        }

        return string.Join('\n', output);
    }
}