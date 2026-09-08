using Library.Api.Endpoints;
using Library.Api.Infrastructure;
using Library.Contracts.Reports;

var builder = WebApplication.CreateBuilder(args);

var serviceAddress = builder.Configuration["LibraryService:Address"] ??
    throw new InvalidOperationException("'LibraryService:Address' is missing from appsettings.json ");

builder.Services.AddGrpcClient<LibraryReportsRpc.LibraryReportsRpcClient>(option =>
    option.Address = new Uri(serviceAddress));

builder.Services.AddExceptionHandler<RpcExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

app.MapReportEndpoints();

app.Run();

public partial class Program;
