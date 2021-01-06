using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SampleFunc
{
    public static class FunctionsAuthenticationHttpContextExtension
    {
        public static async Task<(bool, IActionResult)> AuthenticateAzureFunctionAsync(
            this HttpContext httpContext, string schemaName)
        {
            var result = await httpContext.AuthenticateAsync(schemaName);            
            if (result.Succeeded)
            {
                httpContext.User = result.Principal;
                return (true, null);
            }
            else
            {
                return (false, new UnauthorizedObjectResult(new ProblemDetails
                {
                    Title = "Authorization failed.",
                    Detail = result.Failure?.Message
                }));
            }
        }
    }
}
