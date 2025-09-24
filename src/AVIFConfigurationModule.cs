using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;


namespace NeoSolve.ImageSharp.AVIF;

public sealed class AVIFConfigurationModule : IImageFormatConfigurationModule
{
    public void Configure(Configuration configuration)
    {
        configuration.ImageFormatsManager.SetEncoder(AVIFFormat.Instance, new AVIFEncoder());
        configuration.ImageFormatsManager.SetDecoder(AVIFFormat.Instance, AVIFDecoder.Instance);
        configuration.ImageFormatsManager.AddImageFormatDetector(new AVIFImageFormatDetector());
    }
}
