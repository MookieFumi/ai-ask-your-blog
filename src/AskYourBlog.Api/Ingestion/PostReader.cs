using System.Security.Cryptography;
using System.Text;

namespace AskYourBlog.Api.Ingestion;

// Reads Astro posts (src/content/posts/yyyy-mm-dd-slug.md|mdx). Only published,
// indexable posts are returned: the public URL is /{file name without extension}.
public sealed class PostReader
{
    public async Task<IReadOnlyList<BlogPost>> ReadAsync(string postsDir, string baseUrl, CancellationToken ct)
    {
        var posts = new List<BlogPost>();
        var files = Directory.EnumerateFiles(postsDir, "*.*", SearchOption.TopDirectoryOnly)
            .Where(f => f.EndsWith(".md", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".mdx", StringComparison.OrdinalIgnoreCase))
            .OrderBy(f => f, StringComparer.Ordinal);

        foreach (var file in files)
        {
            var raw = await File.ReadAllTextAsync(file, ct);
            var post = Read(file, raw, baseUrl);
            if (post is not null)
            {
                posts.Add(post);
            }
        }

        return posts;
    }

    public static BlogPost? Read(string path, string raw, string baseUrl)
    {
        var fm = FrontMatterParser.Parse(raw);

        if (IsFalse(fm.Get("published", "true")) || IsTrue(fm.Get("noindex", "false")))
        {
            return null;
        }

        var id = Path.GetFileNameWithoutExtension(path);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

        return new BlogPost(
            id,
            fm.Get("title", id),
            $"{baseUrl.TrimEnd('/')}/{id}",
            fm.Get("date"),
            fm.Get("topic"),
            fm.Get("series"),
            fm.Get("lang", "es"),
            MdxCleaner.Clean(fm.Body),
            hash);
    }

    private static bool IsFalse(string value)
    {
        return value.Equals("false", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsTrue(string value)
    {
        return value.Equals("true", StringComparison.OrdinalIgnoreCase);
    }
}