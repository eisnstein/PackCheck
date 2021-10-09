﻿using PackCheck.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.SetDefaultCommand<CheckCommand>();

app.Configure(
    config =>
    {
        config.SetApplicationName("packcheck");
        config.ValidateExamples();

        config.AddCommand<CheckCommand>("check")
            .WithDescription("Check for newer versions. (default command)")
            .WithExample(new[] { "check", "--csprojFile", ".\\examples\\csproj.xml" });

        config.AddCommand<UpgradeCommand>("upgrade")
            .WithDescription("Upgrade the *.csproj file")
            .WithExample(new[] { "upgrade" })
            .WithExample(new[] { "upgrade", "--version", "stable" })
            .WithExample(new[] { "upgrade", "--version", "latest" })
            .WithExample(new[] { "upgrade", "Microsoft.Extensions.Logging", "--version", "stable" })
            .WithExample(new[] { "upgrade", "Microsoft.Extensions.Logging", "--version", "latest" })
            .WithExample(new[] { "upgrade", "--dry-run" });
    });

return await app.RunAsync(args)
    .ConfigureAwait(false);
