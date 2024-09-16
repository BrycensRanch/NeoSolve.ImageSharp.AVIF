# NeoSolve.ImageSharp.AVIF

AVIF encoder for ImageSharp via the avifenc binary taken from here: https://github.com/AOMediaCodec/libavif/releases

## Example usage
```csharp
using NeoSolve.ImageSharp.AVIF;
using SixLabors.ImageSharp;


using var im = await Image.LoadAsync("input.jpg");

var encoder = new AVIFEncoder { CQLevel = 18 };
await im.SaveAsync("output.avif", encoder);
```
