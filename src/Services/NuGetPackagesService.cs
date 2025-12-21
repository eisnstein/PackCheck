using NuGet.Versioning;
using PackCheck.Data;
using Spectre.Console;

namespace PackCheck.Services;

public class NuGetPackagesService(NuGetApiService nuGetApiService)
{
    private readonly object _lock = new();

    public async Task GetPackagesDataFromNugetRepositoryAsync(List<Package> packages)
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
                package.LatestVersion = NuGetVersionService.GetLatestVersion(versions);
            }

            lock (_lock)
            {
                task.Increment(incrementBy);
            }
        });
    }
}
