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

            ImmutableDictionary<string, string> filteredEnvironmentVariables =
                EnvironmentVariables.All
                    .Where(s => s.Key.Contains("dotnet", StringComparison.OrdinalIgnoreCase))
                    .ToImmutableDictionary();

            string request = JsonConvert.SerializeObject(new
            {
                Request.Headers,
                RemoteIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                Url = Request.GetEncodedUrl(),
                Request.Method,
                Request.ContentLength,
                EnvironmentVariables = filteredEnvironmentVariables
            });

            return new ContentResult
            {
                Content = request,
                ContentType = "text/plain",
                StatusCode = 200
            };
        }
    }
}
