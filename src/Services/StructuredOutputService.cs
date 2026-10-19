using System.Text.Json;
using System.Text.Json.Serialization;
using PackCheck.Commands.Settings;
using PackCheck.Data;

namespace PackCheck.Services;

/// <summary>
/// Writes the result of a run as one JSON or Markdown document (--output json/markdown).
/// The document is written as raw text, as Spectre would wrap long lines at the console width.
/// </summary>
public static class StructuredOutputService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) }
    };

    public static void WriteCheck(
        TextWriter writer, IReadOnlyList<FileReport> files, CheckSettings settings, string? error)
    {
        if (settings.Output == OutputFormat.Json)
        {
            var document = new CheckDocument(
                "check",
                files.Select(f => new CheckFile(
                    f.Path,
                    f.Packages.Select(p => new CheckedPackage(
                        p.PackageName,
                        p.CurrentVersion.ToString(),
                        PackagesService.GetStableTargetVersion(p)?.ToString(),
                        p.LatestVersion?.ToString(),
                        p.UpgradeType)).ToList())).ToList(),
                error);

            writer.WriteLine(JsonSerializer.Serialize(document, JsonOptions));
            return;
        }

        var showLatest = settings.Pre == true || settings.Target == Target.Latest;
        var lines = new List<string>();

        foreach (var file in files)
        {
            lines.Add($"## `{file.Path}`");
            lines.Add("");

            if (file.Packages.Count == 0)
            {
                lines.Add("No outdated packages.");
                lines.Add("");
                continue;
            }

            var groups = settings.Format == "group"
                ? CheckGroups
                    .Select(g => (g.Title, g.Description, Packages: file.Packages.Where(p => p.UpgradeType == g.Type).ToList()))
                    .Where(g => g.Packages.Count > 0)
                    .ToList()
                : [];

            // Like the console output: without patch, minor or major upgrades, the full table is shown
            // to see potential prerelease versions.
            if (groups.Count == 0)
            {
                AddCheckTable(lines, file.Packages, showLatest);
                continue;
            }

            foreach (var group in groups)
            {
                lines.Add($"### {group.Title}");
                lines.Add("");
                lines.Add($"_{group.Description}_");
                lines.Add("");
                AddCheckTable(lines, group.Packages, showLatest);
            }
        }

        WriteMarkdown(writer, lines, error);
    }

    private static readonly (EUpgradeType Type, string Title, string Description)[] CheckGroups =
    [
        (EUpgradeType.Patch, "Patch", "Backwards compatible - bug fixes"),
        (EUpgradeType.Minor, "Minor", "Backwards compatible - new features"),
        (EUpgradeType.Major, "Major", "Possibly breaking changes - check Changelog")
    ];

    private static void AddCheckTable(List<string> lines, IEnumerable<Package> packages, bool showLatest)
    {
        lines.Add(showLatest
            ? "| Package | Current | Latest stable | Latest | Upgrade |"
            : "| Package | Current | Latest stable | Upgrade |");
        lines.Add(showLatest
            ? "|---|--:|--:|--:|---|"
            : "|---|--:|--:|---|");

        foreach (var p in packages)
        {
            var stable = PackagesService.GetStableTargetVersion(p)?.ToString() ?? "-";
            lines.Add(showLatest
                ? Row(p.PackageName, p.CurrentVersion.ToString(), stable, p.LatestVersion?.ToString() ?? "-", Name(p.UpgradeType))
                : Row(p.PackageName, p.CurrentVersion.ToString(), stable, Name(p.UpgradeType)));
        }

        lines.Add("");
    }

    public static void WriteUpgrade(
        TextWriter writer, IReadOnlyList<FileReport> files, UpgradeSettings settings, string? error)
    {
        if (settings.Output == OutputFormat.Json)
        {
            var document = new UpgradeDocument(
                "upgrade",
                settings.Target ?? Target.Stable,
                settings.DryRun,
                files.Select(f => new UpgradeFile(
                    f.Path,
                    f.Packages.Select(p => new UpgradedPackage(
                        p.PackageName,
                        p.CurrentVersion.ToString(),
                        p.NewVersion?.ToString(),
                        PackagesService.GetUpgradeType(p.CurrentVersion, p.NewVersion))).ToList())).ToList(),
                error);

            writer.WriteLine(JsonSerializer.Serialize(document, JsonOptions));
            return;
        }

        var lines = new List<string>();

        if (settings.DryRun)
        {
            lines.Add("_Dry run: no files were changed._");
            lines.Add("");
        }

        foreach (var file in files)
        {
            lines.Add($"## `{file.Path}`");
            lines.Add("");

            if (file.Packages.Count == 0)
            {
                lines.Add("Nothing to upgrade.");
                lines.Add("");
                continue;
            }

            lines.Add("| Package | Current | New | Upgrade |");
            lines.Add("|---|--:|--:|---|");

            foreach (var p in file.Packages)
            {
                lines.Add(Row(
                    p.PackageName,
                    p.CurrentVersion.ToString(),
                    p.NewVersion?.ToString() ?? "-",
                    Name(PackagesService.GetUpgradeType(p.CurrentVersion, p.NewVersion))));
            }

            lines.Add("");
        }

        WriteMarkdown(writer, lines, error);
    }

    private static void WriteMarkdown(TextWriter writer, List<string> lines, string? error)
    {
        if (error is not null)
        {
            lines.Add($"**Error:** {error}");
            lines.Add("");
        }

        // No trailing empty line
        while (lines.Count > 0 && lines[^1].Length == 0)
        {
            lines.RemoveAt(lines.Count - 1);
        }

        foreach (var line in lines)
        {
            writer.WriteLine(line);
        }
    }

    private static string Row(params string[] cells)
    {
        return "| " + string.Join(" | ", cells.Select(c => c.Replace("|", "\\|"))) + " |";
    }

    private static string Name(EUpgradeType upgradeType)
    {
        return JsonNamingPolicy.CamelCase.ConvertName(upgradeType.ToString());
    }

    private sealed record CheckDocument(string Command, IReadOnlyList<CheckFile> Files, string? Error);

    private sealed record CheckFile(string Path, IReadOnlyList<CheckedPackage> Packages);

    private sealed record CheckedPackage(
        string Name,
        string CurrentVersion,
        string? LatestStableVersion,
        string? LatestVersion,
        EUpgradeType UpgradeType);

    private sealed record UpgradeDocument(
        string Command, string Target, bool DryRun, IReadOnlyList<UpgradeFile> Files, string? Error);

    private sealed record UpgradeFile(string Path, IReadOnlyList<UpgradedPackage> Packages);

    private sealed record UpgradedPackage(
        string Name,
        string CurrentVersion,
        string? NewVersion,
        EUpgradeType UpgradeType);
}
