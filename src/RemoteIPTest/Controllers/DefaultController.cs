using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace RemoteIPTest.Controllers
{
    public class DefaultController : Controller
    {
        private readonly ILogger<DefaultController> _logger;

        public DefaultController(ILogger<DefaultController> logger)
        {
            _logger = logger;
        }

        [Route("/")]
        [HttpGet]
        public ContentResult Index()
        {
            _logger.LogWarning($"Request from remote address {HttpContext.Connection.RemoteIpAddress}");
            string[] allowed = {"proxy", "limit"};

            ImmutableDictionary<string, string> filteredEnvironmentVariables =
                EnvironmentVariables.All
                    .Where(s => allowed.Any(a => s.Key.Equals(a, StringComparison.OrdinalIgnoreCase)) || s.Key.Contains("dotnet", StringComparison.OrdinalIgnoreCase))
                    .ToImmutableDictionary();

            string request = JsonConvert.SerializeObject(new
            {
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers,
                    Url = Request.GetEncodedUrl(),
                    Request.Method,
                    Request.ContentLength,
                    EnvironmentVariables = filteredEnvironmentVariables
                },
                Formatting.Indented);

            return new ContentResult
            {
                Content = request,
                ContentType = "text/plain",
                StatusCode = 200
            };
        }
    }
}
