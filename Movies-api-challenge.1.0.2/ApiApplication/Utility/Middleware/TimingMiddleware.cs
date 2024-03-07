using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ApiApplication.Utility.Middleware
{
    public class TimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TimingMiddleware> _logger;

        public TimingMiddleware(RequestDelegate next, ILogger<TimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var sw = new Stopwatch();
            sw.Start();
            _logger.LogInformation($"Request {context.Request.Method} {context.Request.Path} Start");
            // Call the next middleware in the pipeline
            await _next(context);

            sw.Stop();
            var elapsedMilliseconds = sw.ElapsedMilliseconds;

            // Log the execution time
            _logger.LogInformation($"Request {context.Request.Method} {context.Request.Path} executed in {elapsedMilliseconds}ms");
        }
    }
}
