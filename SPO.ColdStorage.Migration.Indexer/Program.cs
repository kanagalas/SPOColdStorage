// dotnet user-secrets set "AzureAd:ClientID" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "AzureAd:Secret" "" --project "SPO.ColdStorage.Migration.Indexer"
// dotnet user-secrets set "AzureAd:TenantId" "" --project "SPO.ColdStorage.Migration.Indexer"

using Microsoft.Extensions.Configuration;
using SPO.ColdStorage.Migration.Engine;
using System.Reflection;

Console.WriteLine("Hello, World!");

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddUserSecrets(Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", true);

var config = builder.Build(); 
var aadConfig = new AzureAdConfig(config);

var discovery = new SharePointDiscovery(new Azure.Identity.ClientSecretCredential(aadConfig.TenantId, aadConfig.ClientID, aadConfig.Secret));

await discovery.StartAsync();
Console.WriteLine("Done");

