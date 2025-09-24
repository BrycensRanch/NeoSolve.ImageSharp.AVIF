using SixLabors.ImageSharp.Metadata;
using System.Collections.Generic;
using SixLabors.ImageSharp.Formats;

namespace NeoSolve.ImageSharp.AVIF; 
public class AVIFFormat : IImageFormat {
    public string Name => "AVIF";
    public static AVIFFormat Instance { get; } = new AVIFFormat();

    public string DefaultMimeType => "image/avif";

    public IEnumerable<string> MimeTypes => AVIFConstants.MimeTypes;

    public IEnumerable<string> FileExtensions => AVIFConstants.FileExtensions;
    public IImageDecoder Decoder { get; } = new AVIFDecoder();
    public IImageEncoder Encoder { get; } = new AVIFEncoder();

    public ImageMetadata CreateDefaultFormatMetadata() => new();
}
