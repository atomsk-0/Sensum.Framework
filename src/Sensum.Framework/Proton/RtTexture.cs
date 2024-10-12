using System.Runtime.InteropServices;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sensum.Framework.Proton;

public static unsafe class RtTexture
{
    public static Image<Rgba32>? Unpack(byte* buffer)
    {
        bool allocated = false;
        byte* data = null;
        if (ResourceUtils.IsPackedFile(buffer))
        {
            uint decompressedSize = 0;
            byte* decompressedBuffer = ResourceUtils.DecompressRtPackToMemory(buffer, &decompressedSize);
            if (decompressedBuffer != null)
            {
                data = (byte*)NativeMemory.Alloc(decompressedSize);
                NativeMemory.Copy(decompressedBuffer, data, decompressedSize);
                allocated = true;
            }
        }
        else
        {
            data = buffer;
        }

        if (data == null)
        {
            if (allocated) NativeMemory.Free(data);
            return null;
        }

        if (Encoding.ASCII.GetString(data, RtFileFormat.RT_FILE_PACKAGE_HEADER_BYTE_SIZE) == RtFileFormat.RTFILE_TEXTURE_HEADER)
        {
            int width = *(int*)(data + 12);
            int height = *(int*)(data + 8);
            ReadOnlySpan<byte> imageBuffer = new ReadOnlySpan<byte>(data + 0x7c, width * height * 4);
            Image<Rgba32> image = Image.LoadPixelData<Rgba32>(imageBuffer, width, height);
            image.Mutate(x => x.Flip(FlipMode.Vertical));
            if (allocated) NativeMemory.Free(data);
            return image;
        }

        if (allocated) NativeMemory.Free(data);

        return null;
    }
}

internal static class RtFileFormat
{
    internal const string RTFILE_PACKAGE_HEADER = "RTPACK";
    internal const byte RT_FILE_PACKAGE_HEADER_BYTE_SIZE = 6;
    internal const string RTFILE_TEXTURE_HEADER = "RTTXTR";
}