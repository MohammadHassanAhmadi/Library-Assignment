using System.Diagnostics;
using Library.Api.Endpoints;
using Library.Api.Infrastructure;
using Library.Contracts.Reports;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var serviceAddress = builder.Configuration["LibraryService:Address"] ??
                     throw new InvalidOperationException("'LibraryService:Address' is missing from appsettings.json ");

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

builder.Services.AddGrpcClient<LibraryReportsRpc.LibraryReportsRpcClient>(option =>
    option.Address = new Uri(serviceAddress));

builder.Services.AddExceptionHandler<RpcExceptionHandler>();

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = context =>
        context.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? context.HttpContext.TraceIdentifier);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSerilogRequestLogging();

app.UseExceptionHandler();

app.UseStatusCodePages();
app.UseSwagger();
app.UseSwaggerUI();

app.MapReportEndpoints();

try
{
    await app.RunAsync();
}
finally
{
    await Log.CloseAndFlushAsync();
}

namespace Library.Api
{
    public partial class Program;
}