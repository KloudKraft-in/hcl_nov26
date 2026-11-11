# Contoso Orders Processor

.NET 10 isolated Azure Functions Queue Trigger. Uses `Azure.Functions.Sdk` 1.0.0, `Microsoft.Azure.Functions.Worker` 2.52.0 and `Microsoft.Azure.Functions.Worker.Extensions.Storage.Queues` 5.5.5.

Build with .NET 10 SDK:

```powershell
dotnet restore
dotnet build
```

The queue trigger connection name is `OrdersQueue`. For Azure deployment, configure `OrdersQueue__queueServiceUri` and `OrdersQueue__credential=managedidentity`. Configure the host storage separately with `AzureWebJobsStorage__accountName` and `AzureWebJobsStorage__credential=managedidentity`.
