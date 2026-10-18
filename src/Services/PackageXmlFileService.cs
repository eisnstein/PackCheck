using System.Text;
using System.Xml;
using NuGet.Versioning;
using PackCheck.Data;

namespace PackCheck.Services;

/// <summary>
/// Upgrades the Version attributes of package elements (PackageReference in a
/// .csproj, PackageVersion in Directory.Packages.props) in place. Only the
/// version text is replaced in the original file content, so everything else
/// (XML declaration, BOM, line endings, final newline, attribute layout,
/// quotes, comments) stays exactly as it was.
/// </summary>
public static class PackageXmlFileService
{
    public static async Task UpgradePackageVersionsAsync(
        string path, string elementName, IReadOnlyList<Package> packages, bool dryRun)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        var encoding = DetectEncoding(bytes, out var preambleLength);
        var content = encoding.GetString(bytes, preambleLength, bytes.Length - preambleLength);

        var replacements = FindVersionReplacements(content, elementName, packages);

        // Apply from the end, so the offsets of earlier replacements stay valid
        var newContent = new StringBuilder(content);
        foreach (var (start, length, value) in replacements.OrderByDescending(r => r.Start))
        {
            newContent.Remove(start, length).Insert(start, value);
        }

        if (dryRun)
        {
            Console.WriteLine(newContent);
            return;
        }

        await File.WriteAllBytesAsync(path, [.. bytes.AsSpan(0, preambleLength), .. encoding.GetBytes(newContent.ToString())]);
    }

    private static List<(int Start, int Length, string Value)> FindVersionReplacements(
        string content, string elementName, IReadOnlyList<Package> packages)
    {
        var replacements = new List<(int Start, int Length, string Value)>();
        var lineStarts = GetLineStarts(content);

        using var reader = XmlReader.Create(new StringReader(content));
        var lineInfo = (IXmlLineInfo)reader;

        while (reader.Read())
        {
            if (reader.NodeType != XmlNodeType.Element || reader.Name != elementName)
            {
                continue;
            }

            var packageName = reader.GetAttribute("Include");
            if (string.IsNullOrEmpty(packageName) || !reader.MoveToAttribute("Version"))
            {
                continue;
            }

            // Skip values that are not a plain version, e.g. ranges or MSBuild properties
            if (!NuGetVersion.TryParse(reader.Value, out var currentVersion))
            {
                continue;
            }

            // Match the version too, a package can be listed more than once,
            // e.g. in conditional ItemGroups per target framework.
            var package = packages.FirstOrDefault(p => p.PackageName == packageName && p.CurrentVersion == currentVersion);
            if (package?.NewVersion is null || package.NewVersion == currentVersion)
            {
                continue;
            }

            // The line position of an attribute points to its name.
            // The value is between the quotes that follow it.
            var nameStart = lineStarts[lineInfo.LineNumber - 1] + lineInfo.LinePosition - 1;
            var quoteIndex = content.IndexOfAny(['"', '\''], nameStart);
            if (quoteIndex < 0)
            {
                continue;
            }

            var valueStart = quoteIndex + 1;
            var valueEnd = content.IndexOf(content[quoteIndex], valueStart);

            // Only replace if the raw text is exactly the version that was read,
            // otherwise (e.g. entities in the value) leave the file untouched.
            if (valueEnd < 0 || content[valueStart..valueEnd] != reader.Value)
            {
                continue;
            }

            replacements.Add((valueStart, valueEnd - valueStart, package.NewVersion.ToString()));
        }

        return replacements;
    }

    private static List<int> GetLineStarts(string content)
    {
        var lineStarts = new List<int> { 0 };
        for (var i = 0; i < content.Length; i++)
        {
            // Covers \n and \r\n line endings
            if (content[i] == '\n')
            {
                lineStarts.Add(i + 1);
            }
        }

        return lineStarts;
    }

    private static Encoding DetectEncoding(byte[] bytes, out int preambleLength)
    {
        if (bytes is [0xEF, 0xBB, 0xBF, ..])
        {
            preambleLength = 3;
            return new UTF8Encoding(false);
        }

        if (bytes is [0xFF, 0xFE, ..])
        {
            preambleLength = 2;
            return new UnicodeEncoding(bigEndian: false, byteOrderMark: false);
        }

        if (bytes is [0xFE, 0xFF, ..])
        {
            preambleLength = 2;
            return new UnicodeEncoding(bigEndian: true, byteOrderMark: false);
        }

        preambleLength = 0;
        return new UTF8Encoding(false);
    }
}
