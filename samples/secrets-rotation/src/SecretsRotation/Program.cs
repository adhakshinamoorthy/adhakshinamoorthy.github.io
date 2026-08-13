using SecretsRotation;

var time = TimeProvider.System;
var vault = new InMemorySecretVault(time);
vault.Rotate("OrdersDbPassword", "development-value-one");
var provider = new CachedSecretProvider(vault, time, TimeSpan.FromSeconds(2));

var first = await provider.GetAsync("OrdersDbPassword", CancellationToken.None);
Console.WriteLine($"Loaded OrdersDbPassword version {first.Version}; value is redacted.");

vault.Rotate("OrdersDbPassword", "development-value-two");
await Task.Delay(TimeSpan.FromSeconds(2.1));
var rotated = await provider.GetAsync("OrdersDbPassword", CancellationToken.None);
Console.WriteLine($"Refreshed OrdersDbPassword version {rotated.Version}; value is redacted.");
