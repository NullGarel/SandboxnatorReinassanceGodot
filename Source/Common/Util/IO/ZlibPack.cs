using System.IO;
using System.IO.Compression;

namespace NullGarel.Util.IO;
public static class ZlibPack
{
    public static byte[] Pack(byte[] inputData)
    {
        using var outputStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(outputStream, CompressionLevel.Optimal))
        {
            zlibStream.Write(inputData, 0, inputData.Length);
        }
        return outputStream.ToArray();
    }

    public static byte[] Unpack(byte[] compressedData)
    {
        using var inputStream = new MemoryStream(compressedData);
        using var outputStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(inputStream, CompressionMode.Decompress))
        {
            zlibStream.CopyTo(outputStream);
        }
        return outputStream.ToArray();
    }

}
