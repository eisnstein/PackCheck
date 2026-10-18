using System.Text;
using PackCheck.Data;
using PackCheck.Services;
using PackCheck.Tests.Factories;
using Target = PackCheck.Data.Target;

namespace PackCheck.Tests.Services;

/// <summary>
/// Upgrading a .csproj or Directory.Packages.props file must only change the
/// version, and keep the XML declaration, BOM and line endings as they were.
/// </summary>
public class XmlFormattingTests
{
    [Test]
    [MatrixDataSource]
    public async Task UpgradeOnlyChangesTheVersion(
        [Matrix("csproj", "cpm")] string fileType,
        [Matrix(true, false)] bool withDeclaration,
        [Matrix(true, false)] bool withBom,
        [Matrix("\n", "\r\n")] string newLine)
    {
        var element = fileType == "csproj" ? "PackageReference" : "PackageVersion";
        string Content(string version)
        {
            List<string> lines =
            [
                "<Project>",
                "  <!-- Packages -->",
                "  <ItemGroup>",
                $"    <{element} Include=\"Pack1\" Version=\"{version}\" />",
                "  </ItemGroup>",
                "</Project>",
                "",
            ];
            if (withDeclaration)
            {
                lines.Insert(0, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            }

            return string.Join(newLine, lines);
        }

        var path = Path.Combine(Directory.GetCurrentDirectory(), $"formatting.{fileType}.xml");
        await File.WriteAllBytesAsync(path, Encode(Content("1.0.0"), withBom));

        try
        {
            List<Package> packages = [PackageFactory.Create("Pack1", "1.0.0", latestStableVersion: "1.1.0")];
            packages = PackagesService.PreparePackagesForUpgrade(packages, Target.Stable);

            if (fileType == "csproj")
            {
                await CsProjFileService.UpgradePackageVersionsAsync(path, packages, false);
            }
            else
            {
                await CentralPackageMgmtService.UpgradePackageVersionsAsync(path, packages, false);
            }

            var expected = Encode(Content("1.1.0"), withBom);
            var actual = await File.ReadAllBytesAsync(path);
            await Assert.That(Encoding.UTF8.GetString(actual)).IsEqualTo(Encoding.UTF8.GetString(expected));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static byte[] Encode(string content, bool withBom)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return withBom ? [.. Encoding.UTF8.GetPreamble(), .. bytes] : bytes;
    }
}
