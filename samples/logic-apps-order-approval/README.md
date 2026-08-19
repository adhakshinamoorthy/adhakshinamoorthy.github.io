# Logic Apps Order Approval

A source-controlled approval workflow and a .NET 10 callback API. The workflow accepts an order, makes a deterministic sample decision, calls the API with bounded retries, and returns an asynchronous result. The callback is idempotent by workflow run ID, so a retry cannot overwrite the first accepted decision.

## Run locally

```powershell
dotnet run --project src/LogicAppsOrderApproval
```

Post `{"workflowRunId":"run-42","orderId":"11111111-1111-1111-1111-111111111111","decision":"approved"}` to `/callbacks/approvals`. Repeat the request to observe idempotent replay behavior.

## Test and deploy

```powershell
dotnet test LogicAppsOrderApproval.slnx
az bicep build --file infra/main.bicep
```

Deploy the workflow with a protected HTTPS callback URL. Replace the sample decision with a managed connector or approval action as appropriate. For production, authenticate the callback with managed identity, use Key Vault-backed settings, set explicit timeouts and run-after branches, protect sensitive run history, define resubmission rules, and alert on failed, throttled, long-running, and abandoned runs.
