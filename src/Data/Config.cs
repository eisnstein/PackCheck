using System.Collections.Generic;

namespace PackCheck.Data;

public record Config
{
    public string? CsProjFile { get; init; }
    public string? SlnFile { get; init; }
    public string? CpmFile { get; init; }
    public List<string>? Exclude { get; init; }
    public List<string>? Filter { get; init; }
}
