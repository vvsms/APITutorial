using Microsoft.AspNetCore.Diagnostics;

namespace APITutorial.API.Middleware;

public sealed class GlobalExceptionHandler(IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception=exception,
            ProblemDetails = new()
            {
                Title = "An unexpected error occurred!",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred! Please try again later or contact support if the problem persists.",
                Instance = httpContext.Request.Path
            }
        });
    }
}
