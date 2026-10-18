namespace PackCheck.Data;

/// <summary>
/// The values of the --output option. Json and Markdown are "structured":
/// stdout then only contains the one document with the result.
/// </summary>
public static class OutputFormat
{
    public const string Table = "table";
    public const string Json = "json";
    public const string Markdown = "markdown";

    public static readonly string[] All = [Table, Json, Markdown];

    public static bool IsStructured(string? output)
    {
        return output is Json or Markdown;
    }
}
