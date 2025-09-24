using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NeoSolve.ImageSharp.AVIF.Tests;

public class AVIFDecoderTests
{
    private const string AVIFTestFile = "Resources/test.avif";
    private const string JPGTestFile = "Resources/test.jpg";

    [Theory]
    [InlineData(AVIFTestFile, true)]
    [InlineData(JPGTestFile, false)]
    public void CanIdentifyAVIFFiles(string file, bool vaildAVIF)
    {
        var SUT = new AVIFDecoder();
        ImageInfo imageInfo;

        using (Stream stream = File.OpenRead(file))
        {
            imageInfo = SUT.Identify(new DecoderOptions(), stream);
        }
        var success = imageInfo != null;

        Assert.Equal(vaildAVIF, success);
    }

    [Theory]
    [InlineData(AVIFTestFile, true)]
    [InlineData(JPGTestFile, false)]
    public async Task CanIdentifyAVIFFilesAsync(string file, bool validAVIF)
    {
        var SUT = new AVIFDecoder();
        ImageInfo imageInfo;

        using (Stream stream = File.OpenRead(file))
        {
            imageInfo = await SUT.IdentifyAsync(new DecoderOptions(), stream);
        }
        var success = imageInfo != null;

        Assert.Equal(validAVIF, success);
    }

    [Fact]
    public void Decode_NonGeneric_CreatesRgba32Image()
    {
        Configuration config = CreateDefaultConfiguration();

        var decoderOptions = new DecoderOptions
        {
            Configuration = config
        };

        using Image image = Image.Load(decoderOptions, AVIFTestFile);
        Assert.IsType<Image<Rgba32>>(image);
    }

    [Fact]
    public void CanDetectImageFormat()
    {
        var SUT = new AVIFImageFormatDetector();
        var header = Encoding.UTF8.GetBytes("0123ftypavif");

        var result = SUT.TryDetectFormat(header, out var format);

        Assert.True(result);
        Assert.IsType<AVIFFormat>(format);
    }



    private static Configuration CreateDefaultConfiguration()
    {
        Configuration cfg = new(new AVIFConfigurationModule());
        return cfg;
    }

}
