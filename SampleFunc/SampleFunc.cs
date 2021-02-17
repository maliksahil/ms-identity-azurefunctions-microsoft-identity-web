using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System.Net.Http;
using Microsoft.Identity.Web.Resource;

namespace SampleFunc
{
    public class SampleFunc
    {
        private readonly ILogger<SampleFunc> _logger;
        private readonly IDownstreamWebApi _downstreamWebApi;

        // The web API will only accept tokens 1) for users, and 2) having the "api-scope" scope for this API
        static readonly string[] scopeRequiredByApi = new string[] { "api-scope" };

        public SampleFunc(ILogger<SampleFunc> logger,
            IDownstreamWebApi downstreamWebApi)
        {
            _downstreamWebApi = downstreamWebApi;
            _logger = logger;
        }

        [FunctionName("SampleFunc")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var (authenticationStatus, authenticationResponse) =
                await req.HttpContext.AuthenticateAzureFunctionAsync();
            if (!authenticationStatus) return authenticationResponse;

            req.HttpContext.VerifyUserHasAnyAcceptedScope(scopeRequiredByApi);
            using var response = await _downstreamWebApi.CallWebApiForUserAsync("DownstreamAPI").ConfigureAwait(false);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                // Do something with apiResult
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            }

            string name = req.HttpContext.User.Identity.IsAuthenticated ? req.HttpContext.User.Identity.Name : null;

            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                : $"Hello, {name}. This HTTP triggered function executed successfully.";

            return new OkObjectResult(responseMessage);
        }
    }
}
