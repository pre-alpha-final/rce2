using System.IO.Compression;

namespace FileSyncAgent.Helpers;

public static class GZipHelpers
{
    public static byte[] GZip(byte[] data)
    {
        using var uncompressedMemoryStream = new MemoryStream(data);
        using var compressedMemoryStream = new MemoryStream();
        using var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Compress);
        uncompressedMemoryStream.CopyTo(gzipStream);
        gzipStream.Flush();

        return compressedMemoryStream.ToArray();
    }

    public static byte[] GUnZip(byte[] data)
    {
        using var compressedMemoryStream = new MemoryStream(data);
        using var uncompressedMemoryStream = new MemoryStream();
        using var gzipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
        gzipStream.CopyTo(uncompressedMemoryStream);
        uncompressedMemoryStream.Flush();

        return uncompressedMemoryStream.ToArray();
    }
}
