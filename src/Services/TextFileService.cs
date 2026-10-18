using System.Text;

namespace PackCheck.Services;

/// <summary>
/// Decodes and encodes file contents while keeping the original encoding and
/// BOM, so files changed by an upgrade only differ in the changed versions.
/// </summary>
public static class TextFileService
{
    public static (string Content, Encoding Encoding, byte[] Preamble) Decode(byte[] bytes)
    {
        var (encoding, preambleLength) = bytes switch
        {
            [0xEF, 0xBB, 0xBF, ..] => ((Encoding)new UTF8Encoding(false), 3),
            [0xFF, 0xFE, ..] => (new UnicodeEncoding(bigEndian: false, byteOrderMark: false), 2),
            [0xFE, 0xFF, ..] => (new UnicodeEncoding(bigEndian: true, byteOrderMark: false), 2),
            _ => (new UTF8Encoding(false), 0),
        };

        var content = encoding.GetString(bytes, preambleLength, bytes.Length - preambleLength);

        return (content, encoding, bytes[..preambleLength]);
    }

    public static byte[] Encode(string content, Encoding encoding, byte[] preamble)
    {
        return [.. preamble, .. encoding.GetBytes(content)];
    }
}
