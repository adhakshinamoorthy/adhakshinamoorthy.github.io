using ArchitectureBoundaryLab.Application;
using ArchitectureBoundaryLab.Infrastructure;

var store = new InMemoryAccountStore();
var account = await new OpenAccountHandler(store).HandleAsync("Ada Lovelace");
Console.WriteLine($"Opened account {account.Id} for {account.Owner}");
