using Markdig;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Storage;
using System.IO;
using SenseNet.ContentRepository.Storage.Security;
using File = SenseNet.ContentRepository.File;
using Task = System.Threading.Tasks.Task;

namespace kavics.cloud;
public interface IFileService
{
    Task ServeFileAsync(HttpContext httpContext);
}

public class FileService : IFileService
{
    private static readonly string DocumentRoot = "/Root/Content/Documentations";
    private static readonly string MarkdownTemplate = "/Root/System/Templates/Markdown.html";

    public async Task ServeFileAsync(HttpContext httpContext)
    {
        var snPath = DocumentRoot + httpContext.Request.Path.Value;
        var file = await Node.LoadAsync<File>(snPath, httpContext.RequestAborted);
        if (file != null)
        {
            if (file.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                await ServeMarkdownFileAsync(file, httpContext);
                return;
            }
            if (file.Name.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                await WriteFileAsync(file, httpContext);
                return;
            }
            if (file.Name.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
            {
                await WriteFileAsync(file, httpContext);
                return;
            }
            if (file.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
            {
                await WriteFileAsync(file, httpContext);
                return;
            }
        }

        httpContext.Response.StatusCode = 404;
    }
    private async Task ServeMarkdownFileAsync(File file, HttpContext httpContext)
    {
        var template = await GetTemplateAsync(httpContext.RequestAborted);

        //var pipeline = new MarkdownPipelineBuilder()
        //    .UseAdvancedExtensions()
        //    .Build();
        //var html = Markdown.ToHtml(RepositoryTools.GetStreamString(file.Binary.GetStream()), pipeline);
        var renderedHtml = Markdown.ToHtml(ReadStringFromStream(file.Binary.GetStream()));

        var html = template
            .Replace("{path}", httpContext.Request.Path.Value?.Trim('/'))
            .Replace("{markdown}", renderedHtml);

        await httpContext.Response.WriteAsync(html);
    }
    private async Task<string> GetTemplateAsync(CancellationToken cancel)
    {
        using var _ = new SystemAccount();
        var file = await Node.LoadAsync<File>(MarkdownTemplate, cancel);
        var template = file == null
            ? string.Empty
            : RepositoryTools.GetStreamString(file.Binary.GetStream());

        return template;
    }

    private string ReadStringFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        var content = reader.ReadToEnd();
        return content;
    }

    private async Task WriteFileAsync(File file, HttpContext httpContext)
    {
        var stream = file.Binary.GetStream();
        await stream.CopyToAsync(httpContext.Response.Body);
        await httpContext.Response.Body.FlushAsync();
    }
}
