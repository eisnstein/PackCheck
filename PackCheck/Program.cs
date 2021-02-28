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
            .WithDescription("Check for newer versions.")
            .WithExample(new[] { "check", "--path", ".\\examples\\csproj.xml" });
    });

return await app.RunAsync(args);