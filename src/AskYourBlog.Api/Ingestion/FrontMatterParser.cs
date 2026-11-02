using YamlDotNet.Serialization;

namespace AskYourBlog.Api.Ingestion;

public static class FrontMatterParser
{
    private static readonly IDeserializer Yaml = new DeserializerBuilder().Build();

    public static FrontMatter Parse(string raw)
    {
        var text = raw.Replace("\r\n", "\n");
        if (!text.StartsWith("---\n", StringComparison.Ordinal))
        {
            return new FrontMatter(new Dictionary<string, string>(), text);
        }

        var end = text.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0)
        {
            return new FrontMatter(new Dictionary<string, string>(), text);
        }

        var yaml = text[4..end];
        var body = text[(end + 4)..].TrimStart('\n');
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var parsed = Yaml.Deserialize<Dictionary<string, object?>>(yaml);
        if (parsed is not null)
        {
            foreach (var (key, value) in parsed)
            {
                values[key] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            }
        }

        return new FrontMatter(values, body);
    }
}