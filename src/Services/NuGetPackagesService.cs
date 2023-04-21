using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Versioning;
using PackCheck.Data;
using Spectre.Console;

namespace PackCheck.Services;

public class NuGetPackagesService
{
    private readonly NuGetApiService _nuGetApiService;

    public NuGetPackagesService(NuGetApiService nuGetApiService)
    {
        _nuGetApiService = nuGetApiService;
    }

    public async Task GetPackagesDataFromCsProjFileAsync(string pathToCsProjFile, List<Package> packages)
    {
        var settings = new XmlReaderSettings { Async = true };
        var reader = XmlReader.Create(pathToCsProjFile, settings);

        while (await reader.ReadAsync())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != "PackageReference")
            {
                continue;
            }

            if (!reader.HasAttributes)
            {
                continue;
            }

            var packageName = "";
            var currentVersion = "";

            while (reader.MoveToNextAttribute())
            {
                if (reader.Name == "Include")
                {
                    packageName = reader.Value;
                }

                if (reader.Name == "Version")
                {
                    currentVersion = reader.Value;
                }
            }

            if (string.IsNullOrEmpty(packageName) || string.IsNullOrEmpty(currentVersion))
            {
                continue;
            }

            packages.Add(new Package(packageName, NuGetVersion.Parse(currentVersion)));
        }

        reader.Close();
    }

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
        var incrementBy = 100 / packages.Count;

        foreach (var package in packages)
        {
            IEnumerable<NuGetVersion> result = await _nuGetApiService.GetPackageVersions(package.PackageName);
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
