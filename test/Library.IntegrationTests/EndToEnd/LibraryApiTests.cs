using System.Net;
using System.Net.Http.Json;
using Library.Api;
using Library.Api.Contracts;
using Library.Contracts.Reports;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace Library.IntegrationTests.EndToEnd;

[Collection(LibraryDatabaseCollection.Name)]
public sealed class LibraryApiTests(LibraryDatabaseFixture fixture) : IAsyncLifetime
{
    private WebApplicationFactory<Program> _api = null!;
    private HttpClient _httpClient = null!;
    private WebApplicationFactory<Service.Program> _service = null!;


    public Task InitializeAsync()
    {
        _service = new WebApplicationFactory<Service.Program>()
            .WithWebHostBuilder(builder =>
                builder.UseSetting("ConnectionStrings:LibraryDb",
                    fixture.ConnectionString));

        var serviceHandler = _service.Server.CreateHandler();

        _api = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
                builder.ConfigureTestServices(services =>
                    services.AddGrpcClient<LibraryReportsRpc.LibraryReportsRpcClient>(options =>
                            options.Address = new Uri("http://localhost"))
                        .ConfigurePrimaryHttpMessageHandler(() => serviceHandler)
                ));

        _httpClient = _api.CreateClient();
        return Task.CompletedTask;
    }


    public async Task DisposeAsync()
    {
        _httpClient.Dispose();
        await _api.DisposeAsync();
        await _service.DisposeAsync();
    }

    [Fact]
    public async Task GetMostBorrowedBooks_ReturnsRankedBooks()
    {
        var books = await _httpClient.GetFromJsonAsync<BorrowedBookDto[]>("/api/reports/most-borrowed-books?limit=10");

        Assert.NotNull(books);
        Assert.Equal("Lord of the Rings - Fellowship of the Rings", books[0].Title);
        Assert.Equal(3, books[0].BorrowCount);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_ReturnsBorrowersInsideTheWindow()
    {
        var borrowers = await _httpClient.GetFromJsonAsync<ActiveBorrowerDto[]>(
            "/api/reports/most-active-borrowers?from=2026-02-01T00:00:00Z&to=2026-03-01T00:00:00Z");
        
        Assert.NotNull(borrowers);
        Assert.Collection(borrowers,
            borrower => Assert.Equal(("Carol", 2), (borrower.Name, borrower.BorrowCount)),
            borrower => Assert.Equal(("Dave", 2), (borrower.Name, borrower.BorrowCount)));
    }

    [Fact]
    public async Task GetReadingPace_ReturnsPagesPerDay()
    {
        var pace = await _httpClient.GetFromJsonAsync<ReadingPaceDto>("/api/reports/loans/1/reading-pace");

        Assert.NotNull(pace);
        Assert.Equal(50d, pace.PagesPerDay);
    }

    [Fact]
    public async Task GetReadingPace_WhenLoanDoesNotExist_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync("/api/reports/loans/999/reading-pace");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_WhenLoanIsStillOut_ReturnsConflict()
    {
        var response = await _httpClient.GetAsync("/api/reports/loans/6/reading-pace");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task GetMostBorrowedBooks_WhenLimitIsInvalid_ReturnsBadRequest()
    {
        var response = await _httpClient.GetAsync("/api/reports/most-borrowed-books?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_ReturnsBooksSharedByTheSameReaders()
    {
        var books = await _httpClient.GetFromJsonAsync<ReadersAlsoBorrowedDto[]>(
            "/api/reports/books/1/readers-also-borrowed?limit=10");

        Assert.NotNull(books);
        Assert.Equal("SQL Basics", books[0].Title);
        Assert.Equal(2, books[0].ReaderCount);
    }

    // Invalid requests
    [Fact]
    public async Task UnknownRoute_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync("/api/reports/typo");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReadingPace_WhenLoanIdIsNotAnInteger_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync("/api/reports/loans/abc/reading-pace");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_WhenBookIdIsNotAnInteger_ReturnsNotFound()
    {
        var response = await _httpClient.GetAsync("/api/reports/books/abc/readers-also-borrowed");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_WhenFromIsNotBeforeTo_ReturnsBadRequest()
    {
        var response = await _httpClient.GetAsync(
            "/api/reports/most-active-borrowers?from=2026-03-01T00:00:00Z&to=2026-02-01T00:00:00Z");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetMostActiveBorrowers_WhenLimitIsInvalid_ReturnsBadRequest()
    {
        var response = await _httpClient.GetAsync(
            "/api/reports/most-active-borrowers?from=2026-02-01T00:00:00Z&to=2026-03-01T00:00:00Z&limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetReadersAlsoBorrowed_WhenLimitIsInvalid_ReturnsBadRequest()
    {
        var response = await _httpClient.GetAsync("/api/reports/books/1/readers-also-borrowed?limit=0");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostToGetEndpoint_ReturnsMethodNotAllowed()
    {
        var response = await _httpClient.PostAsync("/api/reports/most-borrowed-books", null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}