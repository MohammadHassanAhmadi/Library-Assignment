using Grpc.Core;
using Library.Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Library.UnitTests.Grpc;

public class RpcExceptionHandlerTests
{
    private static RpcExceptionHandler CreateHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();

        return new RpcExceptionHandler(services.BuildServiceProvider().GetRequiredService<IProblemDetailsService>());
    }

    [Theory]
    [InlineData(StatusCode.InvalidArgument, StatusCodes.Status400BadRequest)]
    [InlineData(StatusCode.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(StatusCode.FailedPrecondition, StatusCodes.Status409Conflict)]
    [InlineData(StatusCode.Unavailable, StatusCodes.Status503ServiceUnavailable)]
    [InlineData(StatusCode.Internal, StatusCodes.Status500InternalServerError)]
    public async Task TryHandleAsync_MapGrpcStatusToHttpStatus(StatusCode grpcStatusCode, int expectedHttpStatusCode)
    {
        var httpContext = new DefaultHttpContext
        {
            Response = {Body = new MemoryStream()}
        };

        var handled = await CreateHandler().TryHandleAsync(httpContext,
            new RpcException(new Status(grpcStatusCode, "failed")), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(expectedHttpStatusCode, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WhenExceptionIsNotRpc_DoesNotHandleIt()
    {
        var httpContext = new DefaultHttpContext();

        var handled = await CreateHandler()
            .TryHandleAsync(httpContext, new InvalidOperationException(), CancellationToken.None);

        Assert.False(handled);
    }
}