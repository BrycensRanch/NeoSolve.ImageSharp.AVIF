using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace NeoSolve.ImageSharp.AVIF;

public class AVIFDecoder : IImageDecoder
{
    public static IImageDecoder Instance = new AVIFDecoder();

    public Image<TPixel> Decode<TPixel>(DecoderOptions options, Stream stream)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        return DecodeAsync<TPixel>(options, stream, CancellationToken.None).Result;
    }

    public Image Decode(DecoderOptions options, Stream stream)
    {
        return DecodeAsync(options, stream, CancellationToken.None).Result;
    }

    /// <summary>
    /// Decodes an AVIF image from a stream into an ImageSharp image,
    /// piping the data through avifdec.
    /// </summary>
    /// <typeparam name="TPixel">The pixel format.</typeparam>
    /// <param name="options">The decoder options.</param>
    /// <param name="stream">The input stream containing the AVIF data.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A decoded image.</returns>
    public async Task<Image<TPixel>> DecodeAsync<TPixel>(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
    where TPixel : unmanaged, IPixel<TPixel>
    {
        string inputFilePath = Path.GetTempFileName();
        string outputFilePath = Path.GetTempFileName() + ".png";

        try
        {
            using (var fileStream = new FileStream(inputFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            var arguments = new List<string> { inputFilePath, outputFilePath };

            var psi = new ProcessStartInfo
            {
                FileName = Native.CAVIFDEC,
                Arguments = string.Join(' ', arguments),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true
            };

            var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start avifdec process.");

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                string error = await process.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"AVIF decoding failed with exit code {process.ExitCode}. Error: {error}");
            }

            using (var outputStream = new FileStream(outputFilePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096, true))
            {
                return await Image.LoadAsync<TPixel>(outputStream, cancellationToken);
            }
        }
        finally
        {
            File.Delete(inputFilePath);
            File.Delete(outputFilePath);
        }
    }


    public async Task<Image> DecodeAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
    {
        return await DecodeAsync<Rgba32>(options, stream, cancellationToken);
    }


    public ImageInfo Identify(DecoderOptions options, Stream stream)
    {
        return IdentifyAsync(options, stream, CancellationToken.None).Result;
    }

    public async Task<ImageInfo> IdentifyAsync(DecoderOptions options, Stream stream, CancellationToken cancellationToken = default)
    {
        string tempFilePath = Path.GetTempFileName();

        try
        {
            await using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
            {
                await stream.CopyToAsync(fileStream, cancellationToken);
            }

            stream.Position = 0;

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Native.CAVIFDEC,
                    Arguments = $"--info \"{tempFilePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {                
                string output = await process.StandardOutput.ReadToEndAsync();
                var avifInfo = ParseAVIFInfo(output);
                var imageMetadata = new ImageMetadata(); 
                
                Size size = new(avifInfo.Width, avifInfo.Height);
                return new ImageInfo(new PixelTypeInfo(avifInfo.BitDepth), size, imageMetadata);
            }
            else
            {
                string error = await process.StandardError.ReadToEndAsync();
                return null;
            }
        }
        finally
        {
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    public static AVIFInfo ParseAVIFInfo(string text)
    {
        var avifInfo = new AVIFInfo();
        var reader = new StringReader(text);
        string line;

        while ((line = reader.ReadLine()) != null)
        {
            if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
            {
                continue;
            }

            var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
            var key = parts[0].Trim().Replace("*", "").Trim();
            var value = parts[1].Trim();

            switch (key)
            {
                case "Resolution":
                    var resolutionParts = value.Split('x');
                    if (resolutionParts.Length == 2)
                    {
                        avifInfo.Width = int.Parse(resolutionParts[0].Trim());
                        avifInfo.Height = int.Parse(resolutionParts[1].Trim());
                    }
                    break;
                case "Bit Depth":
                    avifInfo.BitDepth = int.Parse(value);
                    break;
                case "Format":
                    avifInfo.Format = value;
                    break;
                case "Chroma Sam. Pos":
                    avifInfo.ChromaSamPos = int.Parse(value);
                    break;
                case "Alpha":
                    avifInfo.Alpha = value;
                    break;
                case "Range":
                    avifInfo.Range = value;
                    break;
                case "Color Primaries":
                    avifInfo.ColorPrimaries = int.Parse(value);
                    break;
                case "Transfer Char.":
                    avifInfo.TransferChar = int.Parse(value);
                    break;
                case "Matrix Coeffs.":
                    avifInfo.MatrixCoeffs = int.Parse(value);
                    break;
                case "ICC Profile":
                    avifInfo.IccProfile = value;
                    break;
                case "XMP Metadata":
                    avifInfo.XmpMetadata = value;
                    break;
                case "Exif Metadata":
                    avifInfo.ExifMetadata = value;
                    break;
                case "Transformations":
                    avifInfo.Transformations = value;
                    break;
                case "Progressive":
                    avifInfo.Progressive = value;
                    break;
                case "Gain map":
                    avifInfo.GainMap = value;
                    break;
            }
        }

        return avifInfo;
    }
}
