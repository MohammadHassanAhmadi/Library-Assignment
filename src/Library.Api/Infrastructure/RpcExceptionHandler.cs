using Grpc.Core;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Library.Api.Infrastructure;

public sealed class RpcExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<RpcExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not RpcException rpcException)
            return false;

        var statusCode = ToHttpStatusCode(rpcException.StatusCode);
        if (statusCode >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "{Method} {Path} failed with {StatusCode} ({GrpcStatus}).",
                httpContext.Request.Method, httpContext.Request.Path, statusCode, rpcException.StatusCode);

        httpContext.Response.StatusCode = statusCode;
        
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = "The library service rejected the request",
                Detail = rpcException.Status.Detail
            }
        });
        return true;
    }

    private static int ToHttpStatusCode(StatusCode rpcStatusCode)
    {
        return rpcStatusCode switch
        {
            StatusCode.InvalidArgument => StatusCodes.Status400BadRequest,
            StatusCode.NotFound => StatusCodes.Status404NotFound,
            StatusCode.FailedPrecondition => StatusCodes.Status409Conflict,
            StatusCode.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}