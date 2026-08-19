var arrivalPerSecond = 750d;
var p95Seconds = .18;
var observedConcurrency = arrivalPerSecond * p95Seconds;
var perInstance = 110d;
var headroom = 1.35;
var instances = (int)Math.Ceiling(arrivalPerSecond * headroom / perInstance);
Console.WriteLine($"concurrency≈{observedConcurrency:F0} instances={instances} headroom={headroom:P0}");
if (args.Contains("--self-test") && (instances < 10 || observedConcurrency <= 0)) return 1;
return 0;
