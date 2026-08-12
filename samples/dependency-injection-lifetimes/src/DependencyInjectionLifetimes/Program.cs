using DependencyInjectionLifetimes.Composition;
using DependencyInjectionLifetimes.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddFulfillment(builder.Configuration);
builder.Services.AddSingleton<FulfillmentRunner>();

using var host = builder.Build();
await host.StartAsync();

await host.Services.GetRequiredService<FulfillmentRunner>()
    .RunAsync(CancellationToken.None);

await host.StopAsync();
