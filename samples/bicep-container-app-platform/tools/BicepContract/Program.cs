using BicepContract;

var directory = args.Length == 1 ? args[0] : "infra";
var result = BicepInspector.Inspect(directory);
foreach (var check in result.Checks) Console.WriteLine($"{(check.Passed ? "PASS" : "FAIL")} {check.Name}");
return result.IsValid ? 0 : 1;
