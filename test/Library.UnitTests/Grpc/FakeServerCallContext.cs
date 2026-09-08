using Grpc.Core;

namespace Library.UnitTests.Grpc;

public class FakeServerCallContext(CancellationToken cancellationToken) : ServerCallContext
{
    protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
    {
        return Task.CompletedTask;
    }

    protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
    {
        throw new NotSupportedException();
    }

    public static FakeServerCallContext Create(CancellationToken cancellationToken) => new(cancellationToken);
    public static FakeServerCallContext Create() => new(CancellationToken.None);

    protected override string MethodCore => "/library.reports.v1.LibraryReportsRpc/Test";
    protected override string HostCore => "localhost";
    protected override string PeerCore => "localhost";
    protected override DateTime DeadlineCore => DateTime.MaxValue;
    protected override Metadata RequestHeadersCore { get; } = new();
    protected override CancellationToken CancellationTokenCore => cancellationToken;
    protected override Metadata ResponseTrailersCore { get; } = new();
    protected override Status StatusCore { get; set; }
    protected override WriteOptions? WriteOptionsCore { get; set; }
    protected override AuthContext AuthContextCore { get; } = new(string.Empty, new Dictionary<string, List<AuthProperty>>());
}