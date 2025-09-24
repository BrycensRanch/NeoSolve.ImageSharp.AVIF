using SixLabors.ImageSharp.Formats;
using System;
using System.Diagnostics.CodeAnalysis;

namespace NeoSolve.ImageSharp.AVIF;

public class AVIFImageFormatDetector : IImageFormatDetector
{
    public int HeaderSize => 12;

    public bool TryDetectFormat(ReadOnlySpan<byte> header, [NotNullWhen(true)] out IImageFormat format)
    {
        bool isAVIF = header.Length <= HeaderSize && IsAvif(header);
        format = isAVIF ? AVIFFormat.Instance : null;
        return isAVIF;
    }

    private static bool IsAvif(ReadOnlySpan<byte> header)
    {
        // Check for the 'ftyp' box type at byte offset 4.
        if (header[4] != 'f' || header[5] != 't' || header[6] != 'y' || header[7] != 'p')
        {
            return false;
        }

        // Check for the 'avif' brand at byte offset 8.
        if (header[8] == 'a' && header[9] == 'v' && header[10] == 'i' && header[11] == 'f')
        {
            return true;
        }

        return false;
    }
}
