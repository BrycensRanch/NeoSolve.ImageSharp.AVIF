using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace NeoSolve.ImageSharp.AVIF;

public class AVIFEncoder : IImageEncoder {
    public static AVIFEncoder Instance { get; } = new AVIFEncoder();

    //
    // Summary:
    //     Gets or sets the quality, that will be used to encode the image. Quality index
    //     must be between 0 and 100 (compression from max to min). Defaults to null (lossless).
    public bool Lossless { get; set; }

    public bool SkipMetadata { get; init; } = false;

    /// <summary>
    /// CQLevel from 0 to 63 (default = 18) lower value = better quality (controls amount of quantization)
    /// If Lossless is set to true, this value will be ignored.
    /// </summary>
    public int CQLevel { get; set; } = 18;

    public void Encode<TPixel>(Image<TPixel> image, Stream stream) where TPixel : unmanaged, IPixel<TPixel> =>
        EncodeAsync(image, stream, CancellationToken.None).Wait();

    public async Task EncodeAsync<TPixel>(Image<TPixel> image, Stream stream, CancellationToken cancellationToken) where TPixel : unmanaged, IPixel<TPixel> {
        var tempPath = Path.GetTempPath();
        var randomId = Guid.NewGuid();
        var randomIdPath = Path.Combine(tempPath, randomId.ToString());

        var arguments = new List<string>();

        if (Lossless)
            arguments.Add("--lossless");
        else
        {
            // --min 0 --max 63 -a end-usage=q -a cq-level=18 -a tune=ssim
            arguments.Add("--min 0");
            arguments.Add("--max 63");
            arguments.Add("-a end-usage=q");
            arguments.Add($"-a cq-level={CQLevel.ToString(CultureInfo.InvariantCulture)}");
            arguments.Add("-a tune=ssim");
        }

        arguments.Add($"{randomIdPath}.png");
        arguments.Add($"{randomIdPath}.avif");

        var psi = new ProcessStartInfo
        {
            FileName = Native.CAVIF,
            Arguments = string.Join(' ', arguments),
            RedirectStandardInput = true,
            RedirectStandardOutput = true
        };

        try
        {
            await image.SaveAsPngAsync($"{randomIdPath}.png", cancellationToken);

            var process = Process.Start(psi);
            await process.WaitForExitAsync(cancellationToken);

            using var fs = File.OpenRead($"{randomIdPath}.avif");
            await fs.CopyToAsync(stream, cancellationToken);
        }
        finally
        {
            if (File.Exists($"{randomIdPath}.png"))
                File.Delete($"{randomIdPath}.png");

            if (File.Exists($"{randomIdPath}.avif"))
                File.Delete($"{randomIdPath}.avif");
        }
    }
}
