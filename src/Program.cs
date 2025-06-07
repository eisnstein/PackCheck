using PackCheck.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.SetDefaultCommand<CheckCommand>();

app.Configure(
    config =>
    {
        config.SetApplicationName("packcheck");

#if DEBUG
        config.PropagateExceptions();
        config.ValidateExamples();
#endif

        config.AddCommand<CheckCommand>("check")
            .WithAlias("c")
            .WithDescription("Check for newer versions. (default command)")
            .WithExample(new[] { "check", "--csprojFile", ".\\examples\\csproj.xml" });

        config.AddCommand<UpgradeCommand>("upgrade")
            .WithAlias("u")
            .WithDescription("Upgrade the *.csproj (or Directory.Packages.props or file-based app file) file")
            .WithExample(new[] { "upgrade" })
            .WithExample(new[] { "upgrade", "--target", "stable" })
            .WithExample(new[] { "upgrade", "--target", "latest" })
            .WithExample(new[] { "upgrade", "Microsoft.Extensions.Logging", "--target", "stable" })
            .WithExample(new[] { "upgrade", "Microsoft.Extensions.Logging", "--target", "latest" })
            .WithExample(new[] { "upgrade", "--dry-run" })
            .WithExample(new[] { "upgrade", "-i" })
            .WithExample(new[] { "upgrade", "--target", "latest", "-i" });
    });

return await app.RunAsync(args)
    .ConfigureAwait(false);
