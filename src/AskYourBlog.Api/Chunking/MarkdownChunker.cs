using System.Text;
using System.Text.RegularExpressions;
using AskYourBlog.Api.Ingestion;

namespace AskYourBlog.Api.Chunking;

// Splits by ## / ### headings; fenced code blocks are never cut; each chunk is
// embedded with the prefix "Post title > Section".
public sealed class MarkdownChunker(ChunkOptions options)
{
    private static readonly Regex Heading = new(@"^#{2,3}\s+(.+)$", RegexOptions.Compiled);

    public IReadOnlyList<BlogChunk> Chunk(BlogPost post)
    {
        var chunks = new List<BlogChunk>();

        foreach (var (title, text) in SplitSections(post.Body))
        {
            foreach (var part in SplitBySize(text))
            {
                chunks.Add(new BlogChunk
                {
                    Id = $"{post.Id}#{chunks.Count:D2}-{Slugify(title)}",
                    PostId = post.Id,
                    ContentHash = post.ContentHash,
                    PostTitle = post.Title,
                    Url = post.Url,
                    Date = post.Date,
                    Topic = post.Topic,
                    Series = post.Series,
                    Section = title,
                    Text = part,
                    Embedding = $"{post.Title} > {title}\n{part}"
                });
            }
        }

        foreach (var chunk in chunks)
        {
            chunk.ChunkCount = chunks.Count;
        }

        return chunks;
    }

    private static List<(string Title, string Text)> SplitSections(string body)
    {
        var sections = new List<(string Title, StringBuilder Text)> { ("", new StringBuilder()) };
        var inCode = false;

        foreach (var line in body.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
            }

            var match = inCode ? Match.Empty : Heading.Match(line);
            if (match.Success)
            {
                sections.Add((match.Groups[1].Value.Trim(), new StringBuilder()));
            }
            else
            {
                sections[^1].Text.AppendLine(line);
            }
        }

        return sections.Select(s => (s.Title, s.Text.ToString().Trim())).Where(s => s.Item2.Length > 0).ToList();
    }

    private IEnumerable<string> SplitBySize(string text)
    {
        var buffer = new StringBuilder();

        foreach (var block in SplitBlocks(text))
        {
            if (buffer.Length > 0 && buffer.Length + block.Length > options.MaxChars)
            {
                yield return buffer.ToString().Trim();
                buffer.Clear();
            }

            buffer.Append(block).Append("\n\n");
        }

        if (buffer.Length > 0)
        {
            yield return buffer.ToString().Trim();
        }
    }

    // Blank-line separated blocks. A fenced code block is a single block.
    private static List<string> SplitBlocks(string text)
    {
        var blocks = new List<string>();
        var current = new StringBuilder();
        var inCode = false;

        foreach (var line in text.Split('\n'))
        {
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                inCode = !inCode;
            }

            if (!inCode && line.Trim().Length == 0)
            {
                if (current.Length > 0)
                {
                    blocks.Add(current.ToString().Trim());
                    current.Clear();
                }
            }
            else
            {
                current.AppendLine(line);
            }
        }

        if (current.Length > 0)
        {
            blocks.Add(current.ToString().Trim());
        }

        return blocks;
    }

    private static string Slugify(string value)
    {
        var slug = Regex.Replace(value.ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
        return slug.Length > 0 ? slug : "intro";
    }
}