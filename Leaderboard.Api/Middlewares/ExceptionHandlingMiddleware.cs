using System.Net.Mime;
using Leaderboard.Application.Resources;
using Leaderboard.Domain.Results;
using ILogger = Serilog.ILogger;

namespace Leaderboard.Api.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
{
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await next(httpContext);
        }
        catch (OperationCanceledException) when (httpContext.RequestAborted.IsCancellationRequested)
        {
            // The request was canceled by the client, there is nobody to answer
        }
        catch (Exception exception)
        {
            logger.Error(exception, "Unhandled exception");

            var response = BaseResult.Failure($"{ErrorMessage.InternalServerError}: {exception.Message}",
                StatusCodes.Status500InternalServerError);

            httpContext.Response.ContentType = MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode = response.ErrorCode ?? StatusCodes.Status500InternalServerError;
            await httpContext.Response.WriteAsJsonAsync(response, CancellationToken.None);
        }
    }
}