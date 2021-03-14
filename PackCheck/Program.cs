using Microsoft.Extensions.DependencyInjection;
using PackCheck.Commands;
using Spectre.Console.Cli;
using Spectre.Cli.Extensions.DependencyInjection;

var serviceCollection = new ServiceCollection();
using var registrar = new DependencyInjectionRegistrar(serviceCollection);
var app = new CommandApp(registrar);

app.SetDefaultCommand<CheckCommand>();

app.Configure(
    config =>
    {
        config.ValidateExamples();

        config.AddCommand<CheckCommand>("check")
            .WithDescription("Check for newer versions. (default command)")
            .WithExample(new[] { "check", "--csprojFile", ".\\examples\\csproj.xml" });

        config.AddCommand<UpgradeCommand>("upgrade")
            .WithDescription("Upgrade the *.csproj file")
            .WithExample(new[] { "upgrade" })
            .WithExample(new[] { "upgrade", "Microsoft.Extensions.Logging", "--version", "latest"});
    });

return await app.RunAsync(args)
    .ConfigureAwait(false);
