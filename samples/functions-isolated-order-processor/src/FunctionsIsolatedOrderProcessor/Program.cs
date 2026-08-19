using FunctionsIsolatedOrderProcessor;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.Services.AddSingleton<IOrderReceiptStore, InMemoryOrderReceiptStore>();
builder.Services.AddSingleton<OrderProcessor>();
builder.Build().Run();
