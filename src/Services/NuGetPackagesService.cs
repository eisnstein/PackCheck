using NuGet.Versioning;
using PackCheck.Data;
using PackCheck.Exceptions;
using Spectre.Console;

namespace PackCheck.Services;

public class NuGetPackagesService(INuGetApiService nuGetApiService)
{
    private readonly object _lock = new();

    public async Task GetPackagesDataFromNugetRepositoryAsync(List<Package> packages)
    {
        try
        {
            await AnsiConsole.Progress()
                .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask(">");

                    await FetchPackagesDataAsync(task, packages);

                    task.StopTask();
                });
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // E.g. offline, behind a proxy or nuget.org is down. The message is
            // printed as markup by the commands, so the details are escaped.
            var baseMessage = ex.GetBaseException().Message;
            var details = ex.Message.Contains(baseMessage)
                ? ex.Message
                : $"{ex.Message} {baseMessage}";

            throw new NuGetFeedException(
                $"Could not fetch package versions from NuGet. {details.EscapeMarkup()}", ex);
        }
    }

    private async Task FetchPackagesDataAsync(ProgressTask task, List<Package> packages)
    {
        var incrementBy = 100.0 / packages.Count;
        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 4
        };

        await Parallel.ForEachAsync(packages, options, async (package, cancellationToken) =>
        {
            IEnumerable<NuGetVersion> result = await nuGetApiService.GetPackageVersions(package.PackageName, cancellationToken);
            List<NuGetVersion> versions = result.ToList();
            if (versions is { Count: > 0 })
            {
                package.LatestStableVersion = NuGetVersionService.GetLatestStableVersion(versions);
                package.LatestPrereleaseVersion = NuGetVersionService.GetLatestPrereleaseVersion(versions);
                package.LatestVersion = NuGetVersionService.GetLatestVersion(versions);
            }

            lock (_lock)
            {
                task.Increment(incrementBy);
            }
        });
    }
}
