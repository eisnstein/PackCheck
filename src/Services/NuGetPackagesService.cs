using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using PackCheck.Data;
using Spectre.Console;

namespace PackCheck.Services;

public class NuGetPackagesService
{
    private readonly NuGetVersionService _nuGetVersionService;

    public NuGetPackagesService(NuGetVersionService nuGetVersionService)
    {
        _nuGetVersionService = nuGetVersionService;
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
        var logger = NullLogger.Instance;
        var cancellationToken = CancellationToken.None;

        SourceCacheContext cache = new();
        var repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

        var incrementBy = 100 / packages.Count;

        foreach (var package in packages)
        {
            var versions = await resource.GetAllVersionsAsync(package.PackageName, cache, logger, cancellationToken);
            if (versions.Any())
            {
                package.LatestStableVersion = _nuGetVersionService.GetLatestStableVersion(versions);
                package.LatestVersion = _nuGetVersionService.GetLatestVersion(versions);
            }

            task.Increment(incrementBy);
        }
    }
}
