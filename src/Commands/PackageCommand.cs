using System.Text.Json;
using PackCheck.Commands.Settings;
using PackCheck.Data;
using PackCheck.Exceptions;
using PackCheck.PackageFiles;
using PackCheck.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace PackCheck.Commands;

/// <summary>
/// The flow shared by check and upgrade: build the context once (config,
/// settings, files), then for each file read the packages, apply filter and
/// exclude and fetch the versions from NuGet. What happens with the packages
/// after that is up to the command.
/// </summary>
public abstract class PackageCommand<TSettings> : AsyncCommand<TSettings> where TSettings : CommonSettings
{
    private readonly NuGetPackagesService _nuGetPackagesService;

    protected PackageCommand(IAnsiConsole console, INuGetApiService nuGetApiService)
    {
        Output = console;
        NuGetApiService = nuGetApiService;
        _nuGetPackagesService = new NuGetPackagesService(nuGetApiService);
    }

    protected IAnsiConsole Output { get; }
    protected INuGetApiService NuGetApiService { get; }

    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            return await RunAsync(settings);
        }
        catch (Exception ex) when (ex is CsProjFileException or CpmFileException or FileBasedAppFileException
                                       or SolutionFileException or SolutionXFileException or NuGetFeedException)
        {
            // The messages of these exceptions contain markup
            Output.MarkupLine($"[dim]WARN:[/] [red]{ex.Message}[/]");
            return -1;
        }
    }

    private async Task<int> RunAsync(TSettings settings)
    {
        var exitCode = await BeforeRunAsync(settings);
        if (exitCode is not null)
        {
            return exitCode.Value;
        }

        string? pathToConfigFile;
        try
        {
            (settings, pathToConfigFile) = PackCheckContextBuilder.LoadSettings(settings);
        }
        catch (JsonException e)
        {
            Output.MarkupLine(
                $"[dim]WARN:[/] Your .packcheckrc[[.json]] configuration file seems to be invalid: {e.Message.EscapeMarkup()}");
            return -1;
        }

        // Printed before the files are resolved, as a missing file may come from the config
        if (pathToConfigFile is not null)
        {
            Output.MarkupLine($"Reading config from [grey]{pathToConfigFile.EscapeMarkup()}[/]");
        }

        var (files, isSolution) = PackCheckContextBuilder.ResolveFiles(settings);
        var context = new PackCheckContext<TSettings>(settings, pathToConfigFile, files, isSolution);

        foreach (var file in context.Files)
        {
            // A warning (e.g. no packages in a project) is not an error, so the run goes on
            if (await ProcessFileAsync(file, context) == Result.Error)
            {
                return -1;
            }
        }

        AfterRun(context);

        return 0;
    }

    private async Task<Result> ProcessFileAsync(IPackageFile file, PackCheckContext<TSettings> context)
    {
        Output.MarkupLine(DescribeFile(file, context.Settings));

        var packages = await file.ReadPackagesAsync();
        if (packages.Count == 0)
        {
            Output.MarkupLine($"Could not find any packages in [grey]{file.Path.EscapeMarkup()}[/]");
            return Result.Warning;
        }

        packages = PackagesService.ApplySettings(packages, context.Settings);
        if (packages.Count == 0)
        {
            Output.MarkupLine("No packages to check. Check your 'filter' or 'exclude' in settings/config.");
            return Result.Warning;
        }

        packages = SelectPackages(packages, context.Settings);

        // Fetch data for each package from nuget and store data on each package
        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages);

        return await ProcessPackagesAsync(file, packages, context);
    }

    /// <summary>
    /// Runs before anything else. Returning an exit code ends the command.
    /// </summary>
    protected virtual Task<int?> BeforeRunAsync(TSettings settings)
    {
        return Task.FromResult<int?>(null);
    }

    /// <summary>
    /// The line (markup) printed before a file is processed.
    /// </summary>
    protected abstract string DescribeFile(IPackageFile file, TSettings settings);

    /// <summary>
    /// Narrows down the packages after filter and exclude, before the versions are fetched.
    /// </summary>
    protected virtual List<Package> SelectPackages(List<Package> packages, TSettings settings)
    {
        return packages;
    }

    /// <summary>
    /// Does the actual work with the packages of a file, whose versions are fetched.
    /// </summary>
    protected abstract Task<Result> ProcessPackagesAsync(
        IPackageFile file, List<Package> packages, PackCheckContext<TSettings> context);

    /// <summary>
    /// Runs after all files were processed successfully.
    /// </summary>
    protected virtual void AfterRun(PackCheckContext<TSettings> context)
    {
    }
}
