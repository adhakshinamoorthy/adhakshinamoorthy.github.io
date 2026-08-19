using ArmTemplateInspector;

var path = args.Length == 1 ? args[0] : Path.Combine("infra", "azuredeploy.json");
var result = TemplateInspector.Inspect(path);
foreach (var check in result.Checks) Console.WriteLine($"{(check.Passed ? "PASS" : "FAIL")} {check.Name}");
return result.IsValid ? 0 : 1;
