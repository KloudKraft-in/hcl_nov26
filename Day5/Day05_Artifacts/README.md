# Day 5 — Azure App Service, Managed Identity and Queue Processing

This artifact pack supports the three Day 5 labs. It contains source projects and configuration templates; it is not a claim that the projects have been compiled or deployed in the training subscription.

## Contents

- `Lab_5.1_5.2_ContosoOrders_Web/ContosoOrders.Web`: one evolving ASP.NET Core Razor Pages project used in Lab 5.1 and extended in Lab 5.2.
- `Lab_5.3_ContosoOrders_Processor/ContosoOrders.Processor`: .NET 10 isolated Azure Functions Queue Trigger project.
- `Configuration`: example App Service and Function App settings.
- `Test_Data`: valid and invalid order message payloads.

## Prerequisites

- .NET 10 SDK.
- Azure Functions Core Tools v4 compatible with .NET 10 isolated projects.
- VS Code with C# Dev Kit, Azure App Service and Azure Functions extensions.
- Azure CLI for local developer authentication (`az login`) or another supported developer credential.
- Azure Storage Account with an `orders` queue.

## Resource naming pattern

Use the participant suffix assigned by the instructor. Example names:

- App Service Plan: `asp-user01-d5`
- Web App: `web-user01-orders-d5`
- Storage Account: `stuser01ordersd5`
- Function App: `fn-user01-orderproc-d5`
- Queue: `orders`

Storage Account names must be globally unique, lowercase and 3–24 characters. Use the actual name in all configuration values.

## Lab 5.1 — Build and deploy the web application

```powershell
cd Lab_5.1_5.2_ContosoOrders_Web/ContosoOrders.Web
dotnet restore
dotnet build
dotnet run
```

Open the local URL printed by ASP.NET Core. The project provides a home page and a Submit Order page. The Queue Storage service is registered in the application, but queue submission requires the Storage Account and permissions configured in Lab 5.2.

Deploy the project to the participant's App Service using the VS Code Azure App Service extension. Use the existing shared Resource Group and participant-owned App Service Plan/Web App.

## Lab 5.2 — Configure Queue Storage and Managed Identity

1. Create a Storage Account and the `orders` queue.
2. Enable the system-assigned identity on the production Web App.
3. Assign **Storage Queue Data Message Sender** to that identity at the Storage Account scope (or narrower queue scope if your RBAC workflow supports it).
4. Add the settings from `Configuration/app-service-settings.example.txt` to the Web App.
5. Deploy the same evolving project again.
6. Submit a test order and verify the message in the `orders` queue.
7. Create a `staging` deployment slot on the Standard S1 (or higher) App Service Plan.
8. Enable system-assigned identity on the staging slot and separately assign **Storage Queue Data Message Sender** to the staging slot identity.
9. Configure the same queue settings on staging, set `APP_ENVIRONMENT=STAGING`, deploy and test it.
10. Set production `APP_ENVIRONMENT=PRODUCTION`; mark `APP_ENVIRONMENT` as a deployment slot setting on both slots, then perform the slot swap as described in the participant guide.

`DefaultAzureCredential` uses the App Service managed identity in Azure. For local development, it uses a supported developer credential such as Azure CLI after `az login`. Do not place storage account keys or connection strings in the cloud App Service settings.

## Lab 5.3 — Build and deploy the Function processor

```powershell
cd Lab_5.3_ContosoOrders_Processor/ContosoOrders.Processor
dotnet restore
dotnet build
```

The Function listens to the `orders` queue, validates each JSON message and logs successful processing. Invalid messages throw an exception so the Queue Trigger retry policy can retry them and eventually move them to `orders-poison`.

### Local settings

Copy `local.settings.sample.json` to `local.settings.json` for local Azurite testing. Ensure Azurite is running and the `orders` queue exists. `local.settings.json` is ignored by Git. For local testing against Azure Storage instead, use a developer-authenticated setup and the appropriate local connection settings; do not copy production managed-identity settings into a local process.

### Azure Function App settings

Create a .NET 10 isolated Function App on Functions v4. For Linux, use Flex Consumption. Enable its system-assigned identity and configure the values in `Configuration/function-app-settings.example.txt`.

Assign the Function App identity:

- **Storage Blob Data Owner** on the host storage account for `AzureWebJobsStorage`.
- **Storage Queue Data Contributor** on the queue/storage account for the Queue Trigger connection (this covers reading, deleting and poison-queue writes for this training scenario).

If you want diagnostic events persisted to Table Storage, also assign **Storage Table Data Contributor** as documented by Microsoft. Allow RBAC assignments time to propagate before testing.

Deploy the project using VS Code's Azure Functions extension or:

```powershell
func azure functionapp publish <FUNCTION_APP_NAME>
```

Verify that `ProcessOrder` appears in the Function App. Submit a valid order from the Web App and confirm the Order ID in Function logs/Application Insights. Then use the invalid payload to test retries and the poison queue.

## Message encoding contract

The Web App configures `QueueClientOptions.MessageEncoding = QueueMessageEncoding.Base64`. The Function Queue extension is configured with `messageEncoding: base64`. Keep both settings aligned. The Azure Storage Queues .NET SDK does not encode messages by default, while Azure Functions queue processing commonly expects Base64 unless configured otherwise.

## Important validation status

Package baseline checked against current NuGet listings and Microsoft guidance: Azure.Identity 1.21.0, Azure.Storage.Queues 12.28.1, Microsoft.Azure.Functions.Worker 2.52.0, Azure Functions SDK 1.0.0 and Queue extension 5.5.5. Microsoft guidance specifies Worker 2.50.0 or later and Azure.Functions.Sdk 1.0.0 or later for .NET 10. The current execution environment used to assemble this ZIP does not contain the .NET SDK or Azure Functions Core Tools. Therefore:

- ZIP structure and JSON/XML configuration syntax can be checked here.
- .NET restore/build, Azure deployment, Managed Identity authentication, RBAC propagation, slot swap and end-to-end queue processing must still be validated in the actual training environment.
- Treat package versions in the project files as pinned starting points; run `dotnet restore` and `dotnet build` on the target training VM before distribution. If NuGet reports incompatibility, stop and reconcile the package set rather than upgrading packages independently.

## Official technical references

- Azure Functions identity-based connections: https://learn.microsoft.com/azure/azure-functions/manage-connections
- Azure Functions isolated worker guide: https://learn.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide
- Azure Queue Storage .NET SDK and message encoding: https://learn.microsoft.com/dotnet/api/overview/azure/storage.queues-readme
- App Service deployment slots: https://learn.microsoft.com/azure/app-service/deploy-staging-slots
