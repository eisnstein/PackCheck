using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NuGet.Versioning;
using PackCheck.Data;
using Spectre.Console;

namespace PackCheck.Services;

public class NuGetPackagesService(NuGetApiService nuGetApiService)
{
    public async Task GetPackagesDataFromNugetRepositoryAsync(List<Package> packages)
    {
        await AnsiConsole.Progress()
            .Columns(new TaskDescriptionColumn(), new ProgressBarColumn(), new PercentageColumn())
            .StartAsync(async ctx =>
            {
                var task = ctx.AddTask(">");

                while (!ctx.IsFinished)
                {
                    await FetchPackagesDataAsync(task, packages);
                }

                task.StopTask();
            });
    }

    private async Task FetchPackagesDataAsync(ProgressTask task, List<Package> packages)
    {
        var incrementBy = 100.0 / packages.Count;

        foreach (var package in packages)
        {
            IEnumerable<NuGetVersion> result = await nuGetApiService.GetPackageVersions(package.PackageName);
            List<NuGetVersion> versions = result.ToList();
            if (versions is { Count: > 0 })
            {
                package.LatestStableVersion = NuGetVersionService.GetLatestStableVersion(versions);
                package.LatestVersion = NuGetVersionService.GetLatestVersion(versions);
            }

            task.Increment(incrementBy);
        }
    }
}
