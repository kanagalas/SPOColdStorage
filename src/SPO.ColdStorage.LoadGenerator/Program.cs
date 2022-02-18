// Run program to see required args

using CommandLine;
using SPO.ColdStorage.LoadGenerator;
using SPO.ColdStorage.Migration.Engine;

await Parser.Default.ParseArguments<Options>(args).WithParsedAsync<Options>(async o =>
{
    Console.WriteLine($"Running against {o.TargetWeb}");

    // Warn if just running the app
    if (!System.Diagnostics.Debugger.IsAttached)
    {
        Console.WriteLine($"\nLAST WARNING! This program will be highly destructive to your web '{o.TargetWeb}'. " +
        $"\nPress any key to confirm you understand '{o.TargetWeb}' will be destroyed...");
        Console.ReadKey();
    }


    var ctx = await AuthUtils.GetClientContext(o.TargetWeb!, o.TenantId!, o.ClientID!, o.ClientSecret!, o.KeyVaultUrl!, o.BaseServerAddress!, DebugTracer.ConsoleOnlyTracer());
    var gen = new LoadGenerator(o);
    await gen.Go(o.FileCount);

});


Console.WriteLine("All done");
