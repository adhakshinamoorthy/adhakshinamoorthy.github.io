# ARM Secure Storage

A native Azure Resource Manager JSON template for a security-focused storage baseline with deterministic naming, validated parameters, governance tags, HTTPS-only access, TLS 1.2, OAuth-first authorization, deny-by-default networking, infrastructure encryption, recovery, versioning, change feed, and lifecycle policy.

## Inspect locally

```powershell
dotnet run --project tools/ArmTemplateInspector -- infra/azuredeploy.json
```

## Test

```powershell
dotnet test ArmSecureStorage.slnx
```

## Preview and deploy

```powershell
az deployment group what-if --resource-group <resource-group> --template-file infra/azuredeploy.json --parameters infra/azuredeploy.parameters.json
az deployment group create --resource-group <resource-group> --template-file infra/azuredeploy.json --parameters infra/azuredeploy.parameters.json
```

The deny-by-default network rule intentionally requires a private endpoint or approved network rule before application access. Use deployment stacks or another deliberate lifecycle mechanism when resource deletion ownership matters.
