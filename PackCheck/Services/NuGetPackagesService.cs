using System;
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
                    var task = ctx.AddTask(">");

                    while (!ctx.IsFinished)
                    {
                        await FetchPackagesData(task, packages);
                    }
                });
        }

        private async Task FetchPackagesData(ProgressTask task, List<Package> packages)
        {
            // Number of concurrent requests
            // TODO: make variable via settings
            var numberConcurrentRequests = 4;
            // Number of total runs needed to make all requests
            var numberRunsTotal = Math.Ceiling((double)packages.Count / numberConcurrentRequests);
            // Number to increment progress bar = 100% / number of runs
            var incrementBy = 100d / numberRunsTotal;

            ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;

            SourceCacheContext cache = new();
            SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            FindPackageByIdResource resource = await repository.GetResourceAsync<FindPackageByIdResource>();

            for (var i = 0; i < numberRunsTotal; i++)
            {
                // Only take "number of allowed concurrent requests" packages at a time
                var currentRunPackages = packages
                    .Skip(i * numberConcurrentRequests)
                    .Take(numberConcurrentRequests);

                var tasks = currentRunPackages
                    .Select(p => resource.GetAllVersionsAsync(p.PackageName, cache, logger, cancellationToken));

                await Task.WhenAll(tasks);

                foreach (var (p, index) in currentRunPackages.Select((item, index) => (item, index)))
                {
                    var versions = tasks.ElementAt(index).Result;
                    if (versions.Any())
                    {
                        p.LatestStableVersion = _nuGetVersionService.GetLatestStableVersion(p, versions);
                        p.LatestVersion = _nuGetVersionService.GetLatestVersion(p, versions);
                    }
                }

                task.Increment(incrementBy);
            }
        }
    }
}
