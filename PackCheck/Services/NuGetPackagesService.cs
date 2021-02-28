using System.Threading.Tasks;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using PackCheck.Data;
using Spectre.Console;

namespace PackCheck.Services
{
    public class NuGetPackagesService
    {
        private readonly NuGetVersionService _nuGetVersionService;

        public NuGetPackagesService(NuGetVersionService nuGetVersionService)
        {
            _nuGetVersionService = nuGetVersionService;
        }

        public async Task GetPackagesDataFromCsProjFile(string pathToCsProjFile, List<Package> packages)
        {
            var settings = new XmlReaderSettings { Async = true };
            XmlReader reader = XmlReader.Create(pathToCsProjFile, settings);

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

                string packageName = "";
                string currentVersion = "";

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
        }

        public async Task GetPackagesDataFromNugetRepository(string pathToCsProjFile, List<Package> packages)
        {
            AnsiConsole.MarkupLine($"Checking newest versions for [silver]{pathToCsProjFile}[/]");

            await AnsiConsole.Progress()
                .Columns(new ProgressColumn[]
                {
                    new TaskDescriptionColumn(),
                    new ProgressBarColumn(),
                    new PercentageColumn()
                })
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask("Fetching data");
                    var incrementBy = 100d / packages.Count;

                    while (!ctx.IsFinished)
                    {
                        await FetchPackagesData(task, incrementBy, packages);
                    }
                });
        }

        private async Task FetchPackagesData(ProgressTask task, double incrementBy, List<Package> packages)
        {
            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            SourceCacheContext cache = new();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            foreach (Package p in packages)
            {
                var versions = await resource.GetAllVersionsAsync(p.PackageName, cache, logger, cancellationToken);

                if (versions is null || !versions.Any())
                {
                    task.Increment(incrementBy);
                    continue;
                }

                p.LatestStableVersion = _nuGetVersionService.GetLatestStableVersion(p, versions);
                p.LatestVersion = _nuGetVersionService.GetLatestVersion(p, versions);

                task.Increment(incrementBy);
            }
        }
    }
}
