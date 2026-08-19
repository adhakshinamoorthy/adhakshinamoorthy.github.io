# Terraform Azure Web Platform

A Terraform configuration for a governed .NET 10 App Service platform with environment validation, mandatory tags, Log Analytics, Application Insights, managed identity, HTTPS, TLS, health checks, and explicit outputs.

## Inspect locally

```powershell
dotnet run --project tools/TerraformContract -- infra
```

## Test

```powershell
dotnet test TerraformAzureWebPlatform.slnx
```

## Use Terraform

```powershell
terraform -chdir=infra init
terraform -chdir=infra fmt -check
terraform -chdir=infra validate
terraform -chdir=infra plan -out=tfplan
terraform -chdir=infra apply tfplan
```

For team use, configure an Azure Storage remote backend with state locking, authenticate through workload identity, commit the dependency lock file produced by `terraform init`, review the saved plan, and keep state and plan artifacts out of source control.
