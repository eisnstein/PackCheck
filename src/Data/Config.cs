using System.Collections.Generic;

namespace PackCheck.Data;

public record Config
{
    public string? CsProjFile { get; init; }
    public string? SlnFile { get; init; }
    public string? SlnxFile { get; init; }
    public string? CpmFile { get; init; }
    public string? FbaFile { get; init; }
    public List<string>? Exclude { get; init; }
    public List<string>? Filter { get; init; }
    public string? Format { get; init; }
    public bool Pre { get; init; } = true;
}
