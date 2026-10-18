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
/// With --output json/markdown, nothing but one document with the result is
/// printed, at the end of the run or on an error.
/// </summary>
public abstract class PackageCommand<TSettings> : AsyncCommand<TSettings> where TSettings : CommonSettings
{
    private readonly NuGetPackagesService _nuGetPackagesService;
    private readonly List<FileReport> _fileReports = [];
    private TSettings? _settings;

    protected PackageCommand(IAnsiConsole console, INuGetApiService nuGetApiService)
    {
        Output = console;
        NuGetApiService = nuGetApiService;
        _nuGetPackagesService = new NuGetPackagesService(nuGetApiService);
    }

    protected IAnsiConsole Output { get; }
    protected INuGetApiService NuGetApiService { get; }

    /// <summary>
    /// Whether the result is printed as one JSON/Markdown document,
    /// so the command must not print anything itself.
    /// </summary>
    protected bool IsStructuredOutput => OutputFormat.IsStructured(_settings?.Output);

    public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings, CancellationToken cancellationToken)
    {
        _settings = settings;

        try
        {
            return await RunAsync(settings);
        }
        catch (Exception ex) when (ex is CsProjFileException or CpmFileException or FileBasedAppFileException
                                       or SolutionFileException or SolutionXFileException or NuGetFeedException)
        {
            // The messages of these exceptions contain markup
            return Fail(ex.Message, $"[dim]WARN:[/] [red]{ex.Message}[/]");
        }
    }

    private async Task<int> RunAsync(TSettings settings)
    {
        if (settings.Version == true)
        {
            OutputService.PrintVersionInfo(Output);
            return 0;
        }

        string? pathToConfigFile;
        try
        {
            (settings, pathToConfigFile) = PackCheckContextBuilder.LoadSettings(settings);
        }
        catch (JsonException e)
        {
            var message = $"Your .packcheckrc[[.json]] configuration file seems to be invalid: {e.Message.EscapeMarkup()}";
            return Fail(message, $"[dim]WARN:[/] {message}");
        }

        _settings = settings;

        // The value from the config file is not validated by Spectre
        if (settings.Output is not null && !OutputFormat.All.Contains(settings.Output))
        {
            var message = $"Output value '{settings.Output.EscapeMarkup()}' in the config file is not valid. " +
                          $"Valid values are: {string.Join(", ", OutputFormat.All)}";
            return Fail(message, $"[dim]WARN:[/] [red]{message}[/]");
        }

        var settingsError = ValidateSettings(settings);
        if (settingsError is not null)
        {
            return Fail(settingsError, $"[dim]WARN:[/] [red]{settingsError}[/]");
        }

        var exitCode = await BeforeRunAsync(settings);
        if (exitCode is not null)
        {
            return exitCode.Value;
        }

        // Printed before the files are resolved, as a missing file may come from the config
        if (pathToConfigFile is not null)
        {
            Status($"Reading config from [grey]{pathToConfigFile.EscapeMarkup()}[/]");
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

        if (IsStructuredOutput)
        {
            WriteStructuredOutput(Output.Profile.Out.Writer, _fileReports, settings, null);
        }
        else
        {
            AfterRun(context);
        }

        return 0;
    }

    private async Task<Result> ProcessFileAsync(IPackageFile file, PackCheckContext<TSettings> context)
    {
        Status(DescribeFile(file, context.Settings));

        var packages = await file.ReadPackagesAsync();
        if (packages.Count == 0)
        {
            Status($"Could not find any packages in [grey]{file.Path.EscapeMarkup()}[/]");
            Report(file, []);
            return Result.Warning;
        }

        packages = PackagesService.ApplySettings(packages, context.Settings);
        if (packages.Count == 0)
        {
            Status("No packages to check. Check your 'filter' or 'exclude' in settings/config.");
            Report(file, []);
            return Result.Warning;
        }

        packages = SelectPackages(packages, context.Settings);

        // Fetch data for each package from nuget and store data on each package
        await _nuGetPackagesService.GetPackagesDataFromNugetRepositoryAsync(packages, showProgress: !IsStructuredOutput);

        return await ProcessPackagesAsync(file, packages, context);
    }

    /// <summary>
    /// Prints a status line (markup), unless the output is a JSON/Markdown document.
    /// </summary>
    protected void Status(string markup)
    {
        if (!IsStructuredOutput)
        {
            Output.MarkupLine(markup);
        }
    }

    /// <summary>
    /// Adds the result of a file to the JSON/Markdown document.
    /// </summary>
    protected void Report(IPackageFile file, IReadOnlyList<Package> packages)
    {
        _fileReports.Add(new FileReport(Path.GetRelativePath(Directory.GetCurrentDirectory(), file.Path), packages));
    }

    /// <param name="message">The error (markup).</param>
    /// <param name="line">The line (markup) printed with the table output.</param>
    private int Fail(string message, string line)
    {
        if (IsStructuredOutput)
        {
            WriteStructuredOutput(Output.Profile.Out.Writer, _fileReports, _settings!, Markup.Remove(message));
        }
        else
        {
            Output.MarkupLine(line);
        }

        return -1;
    }

    /// <summary>
    /// Checks the settings, merged with the config. Returns an error (markup) to end the command.
    /// </summary>
    protected virtual string? ValidateSettings(TSettings settings)
    {
        return null;
    }

    /// <summary>
    /// Runs after the settings are loaded, before any file is processed. Returning an exit code ends the command.
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
    /// With structured output, the result goes to <see cref="Report"/> instead of the console.
    /// </summary>
    protected abstract Task<Result> ProcessPackagesAsync(
        IPackageFile file, List<Package> packages, PackCheckContext<TSettings> context);

    /// <summary>
    /// Writes the JSON/Markdown document.
    /// </summary>
    /// <param name="error">The error (plain text) that ended the run, if any.</param>
    protected abstract void WriteStructuredOutput(
        TextWriter writer, IReadOnlyList<FileReport> files, TSettings settings, string? error);

    /// <summary>
    /// Runs after all files were processed successfully, only with the table output.
    /// </summary>
    protected virtual void AfterRun(PackCheckContext<TSettings> context)
    {
    }
}
