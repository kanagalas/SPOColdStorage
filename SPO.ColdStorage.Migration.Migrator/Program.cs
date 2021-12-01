// dotnet user-secrets set "SearchServiceEndPoint" "" --project "SPO.ColdStorage.Migration.Migrator"
// dotnet user-secrets set "SearchServiceAdminApiKey" "" --project "SPO.ColdStorage.Migration.Migrator"
// dotnet user-secrets set "SearchServiceQueryApiKey" "" --project "SPO.ColdStorage.Migration.Migrator"

using Microsoft.Extensions.Configuration;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine;

Console.WriteLine("SPO Cold Storage - Migrator Listener");
Console.WriteLine("This app will listen for messages from service-bus and handle them when they arrive, untill you close this application.");

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddUserSecrets(System.Reflection.Assembly.GetExecutingAssembly())
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", true);


var config = builder.Build();
var allConfig = new Config(config);

var listener = new ServiceBusMigrationListener(allConfig);
await listener.ListenForFilesToMigrate();
