// dotnet user-secrets set "AzureAd:ClientID" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "AzureAd:Secret" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "AzureAd:TenantId" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "ConnectionStrings:ColdStorageDbContext" "Server=(localdb)\\mssqllocaldb;Database=SPOColdStorageDbContextDev;Trusted_Connection=True;MultipleActiveResultSets=true" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "ConnectionStrings:ServiceBus" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "ConnectionStrings:Storage" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "Dev:DefaultStorageConnection" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "Dev:DefaultSharePointSite" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "KeyVaultUrl" "https://spocoldstoragedev.vault.azure.net" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "BaseServerAddress" "https://m365x352268.sharepoint.com" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "BaseServerAddress" "https://m365x352268.sharepoint.com" --project "SPO.ColdStorage.Migration.Indexer"
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine;
using SPO.ColdStorage.Migration.Engine.Utils;

Console.WriteLine("SPO Cold Storage - SharePoint Indexer");

var config = ConsoleUtils.GetConfigurationBuilder();

// Send to application insights or just the stdout?
DebugTracer tracer;
if (config.HaveAppInsightsConfigured)
{
    tracer = new DebugTracer(config.AppInsightsInstrumentationKey, "Indexer");
}
else
    tracer = DebugTracer.ConsoleOnlyTracer();


// Init DB
using (var db = new SPOColdStorageDbContext(config))
{
    await DbInitializer.Init(db, config.DevConfig);
}

// Start discovery
var discovery = new SharePointContentIndexer(config, tracer);
await discovery.StartMigrateAllSites();


Console.WriteLine("\nAll sites scanned. Finished indexing.");
