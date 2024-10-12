using System.Diagnostics;
using System.Runtime.InteropServices;
using Ionic.Zlib;

namespace Sensum.Framework.Proton;

public static unsafe class ResourceUtils
{
    // More safe than older method
    public static byte* ZLibInflateToMemory(byte* pInput, int compressedSize, int decompressedSize)
    {
        byte* pOutput = (byte*)NativeMemory.Alloc((UIntPtr)decompressedSize);

        try
        {
            using var compressedStream = new UnmanagedMemoryStream(pInput, compressedSize);
            using var decompressorStream = new ZlibStream(compressedStream, CompressionMode.Decompress);
            byte[] managedBuffer = new byte[decompressedSize];
            int bytesRead = decompressorStream.Read(managedBuffer, 0, decompressedSize);
            if (bytesRead != decompressedSize)
            {
                NativeMemory.Free(pOutput);
                return null;
            }

            for (int i = 0; i < bytesRead; i++)
            {
                pOutput[i] = managedBuffer[i];
            }

            return pOutput;
        }
        catch
        {
            NativeMemory.Free(pOutput);
            return null;
        }
    }

    public static byte* DecompressRtPackToMemory(byte* pMem, uint* decompressedSizePtr)
    {
        if (IsPackedFile(pMem) == false)
        {
            Debug.Assert(false, "Not a packed file");
            return null;
        }

        const int rt_file_header_size = 0x8;
        const int rt_pack_header_size = rt_file_header_size + 0x18;
        int compressedSize = *(int*)(pMem + rt_file_header_size);
        int decompressedSizeH = *(int*)(pMem + rt_file_header_size + 4);
        byte* pDeCompressed = ZLibInflateToMemory(pMem + rt_pack_header_size, compressedSize, decompressedSizeH);
        *decompressedSizePtr = (uint)decompressedSizeH;
        return pDeCompressed;
    }


    /// <summary>
    /// Requires manual freeing of memory after use (NativeMemory.Free)
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static byte* LoadFileToMemory(string filePath)
    {
        using var fs = File.Open(filePath, FileMode.Open, FileAccess.Read);
        byte* buffer = (byte*)NativeMemory.Alloc((nuint)fs.Length);
        _ = fs.Read(new Span<byte>(buffer, (int)fs.Length));
        return buffer;
    }

    public static bool IsPackedFile(byte* pFile)
    {
        string fileHeader = Marshal.PtrToStringAnsi((nint)pFile, RtFileFormat.RT_FILE_PACKAGE_HEADER_BYTE_SIZE);
        return fileHeader == RtFileFormat.RTFILE_PACKAGE_HEADER;
    }
}