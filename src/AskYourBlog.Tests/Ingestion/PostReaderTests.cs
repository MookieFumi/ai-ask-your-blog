using AskYourBlog.Api.Ingestion;

namespace AskYourBlog.Tests.Ingestion;

public sealed class PostReaderTests
{
    private const string BaseUrl = "https://mookiefumi.com/";

    [Fact]
    public void Url_uses_the_file_name_with_the_date()
    {
        var raw = "---\ntitle: \"Saga: rollback\"\ndate: 2026-12-22\ntopic: desarrollo\nseries: concepts\npublished: true\n---\n\nHola";

        var post = PostReader.Read("/x/2026-12-22-saga.md", raw, BaseUrl);

        Assert.NotNull(post);
        Assert.Equal("https://mookiefumi.com/2026-12-22-saga", post.Url);
        Assert.Equal("Saga: rollback", post.Title);
        Assert.Equal("concepts", post.Series);
    }

    [Fact]
    public void Unpublished_posts_are_skipped()
    {
        var raw = "---\ntitle: T\npublished: false\n---\n\nHola";

        Assert.Null(PostReader.Read("/x/2026-12-22-saga.md", raw, BaseUrl));
    }

    [Fact]
    public void Noindex_posts_are_skipped()
    {
        var raw = "---\ntitle: T\nnoindex: true\n---\n\nHola";

        Assert.Null(PostReader.Read("/x/2026-01-01-a.md", raw, BaseUrl));
    }

    [Fact]
    public void Missing_published_means_published()
    {
        var raw = "---\ntitle: T\n---\n\nHola";

        Assert.NotNull(PostReader.Read("/x/2026-01-01-a.md", raw, BaseUrl));
    }

    [Fact]
    public void Same_content_gives_same_hash()
    {
        var raw = "---\ntitle: T\n---\n\nHola";

        var a = PostReader.Read("/x/2026-01-01-a.md", raw, BaseUrl);
        var b = PostReader.Read("/x/2026-01-01-a.md", raw + " ", BaseUrl);

        Assert.NotEqual(a!.ContentHash, b!.ContentHash);
    }

    [Fact]
    public void Mdx_imports_and_tags_are_removed_but_code_is_kept()
    {
        var body = "import Foo from './Foo.astro'\n\n<img src=\"a.png\" alt=\"x\"/>Texto\n\n```html\n<div>keep</div>\n```";

        var clean = MdxCleaner.Clean(body);

        Assert.DoesNotContain("import Foo", clean);
        Assert.DoesNotContain("<img", clean);
        Assert.Contains("<div>keep</div>", clean);
        Assert.Contains("Texto", clean);
    }
}