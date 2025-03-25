using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using SenseNet.BusinessSolutions.Common;
using SenseNet.ContentRepository;
using SenseNet.ContentRepository.Security.ApiKeys;
using SenseNet.ContentRepository.Storage;
using SenseNet.ContentRepository.Storage.Security;
using SenseNet.Services.Core.Diagnostics;
using System.Net.Http;
using File = SenseNet.ContentRepository.File;
using Task = System.Threading.Tasks.Task;

namespace kavics.cloud
{
    public class DataRecorderMiddleware
    {
        private readonly RequestDelegate? _next;
        private ILogger<DataRecorderMiddleware> _logger;

        public DataRecorderMiddleware(RequestDelegate next, ILogger<DataRecorderMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext httpContext, WebTransferRegistrator statistics)
        {
            var statData = statistics?.RegisterWebRequest(httpContext);

            await ProcessRequestAsync(httpContext).ConfigureAwait(false);

            statistics?.RegisterWebResponse(statData, httpContext);

            // Call next in the chain if exists
            if (_next != null)
                await _next(httpContext).ConfigureAwait(false);
        }

        private async Task ProcessRequestAsync(HttpContext httpContext)
        {
            
            if (User.Current.Id == User.Visitor.Id)
            {
                if (httpContext.Request.Method == "POST")
                {
                    httpContext.Request.Form.TryGetValue("user", out var u);
                    httpContext.Request.Form.TryGetValue("password", out var p);
                    var user = u.FirstOrDefault();
                    var password = p.FirstOrDefault();
                    if (user != null && password != null)
                    {
                        var apiKey = await Login(user, password, httpContext, _logger);
                        if (apiKey != null)
                        {
                            //httpContext.Response.Headers.Append("apikey", apiKey);
                            //httpContext.Response.Redirect(httpContext.Request.Path + "?apikey=" + apiKey, true);
                            await WriteForm(httpContext, apiKey);
                            return;
                        }
                    }
                }

                await WriteTextFile("/Root/Content/DataRecorder/UI/Login.html", httpContext);
            }

            if (httpContext.Request.Path.Equals("/recorder/bloodpressure", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessBloodPressure(httpContext);
                return;
            }

            await httpContext.Response.WriteAsync("RECORDER: " + httpContext.Request.Path);
        }

        private async Task<string?> Login(string user, string password, HttpContext httpContext, ILogger logger)
        {
            var loggedInUser = await BsTools.LoginByNameOrEmailAsync<User>(user, password, null, httpContext, logger, httpContext.RequestAborted);
            if (loggedInUser == null)
                return null;
            User.Current = loggedInUser;
            var apiKey = AccessTokenVault.GetAllTokens(loggedInUser.Id).FirstOrDefault();
            return apiKey?.Value;
        }

        private async Task WriteTextFile(string path, HttpContext httpContext)
        {
            var html = await LoadFileContent(path, httpContext.RequestAborted);
            if (html == null)
            {
                httpContext.Response.StatusCode = 404;
                return;
            }
            await httpContext.Response.WriteAsync(html ?? string.Empty);
        }
        private async Task<string?> LoadFileContent(string path, CancellationToken cancel)
        {
            var file = await Node.LoadAsync<File>(path, cancel);
            if(file == null)
                return null;
            var stream = file.Binary.GetStream();
            if(stream == null)
                return null;
            var text = RepositoryTools.GetStreamString(stream);
            return text;
        }

        private async Task ProcessBloodPressure(HttpContext httpContext)
        {
            if (httpContext.Request.Method == "GET")
            {
                await WriteForm(httpContext);
            }

            if (httpContext.Request.Method == "POST")
            {
                httpContext.Request.Form.TryGetValue("sys", out var sys);
                httpContext.Request.Form.TryGetValue("dia", out var dia);
                httpContext.Request.Form.TryGetValue("pul", out var pul);

                // process sys, dia, pul

                //await WriteForm(httpContext);
                await httpContext.Response.WriteAsync("ok");
            }
        }

        private async Task WriteForm(HttpContext httpContext, string? apiKey = null)
        {
            apiKey ??= httpContext.Request.Headers["apikey"];

            httpContext.Response.Cookies.Append("apikey2", apiKey);

            await WriteTextFile("/Root/Content/DataRecorder/UI/BloodPressure.html", httpContext);
        }
    }
}
