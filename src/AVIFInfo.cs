namespace NeoSolve.ImageSharp.AVIF;

public class AVIFInfo
{
    public int Width { get; set; }
    public int Height { get; set; }
    public int BitDepth { get; set; }
    public string Format { get; set; }
    public int ChromaSamPos { get; set; }
    public string Alpha { get; set; }
    public string Range { get; set; }
    public int ColorPrimaries { get; set; }
    public int TransferChar { get; set; }
    public int MatrixCoeffs { get; set; }
    public string IccProfile { get; set; }
    public string XmpMetadata { get; set; }
    public string ExifMetadata { get; set; }
    public string Transformations { get; set; }
    public string Progressive { get; set; }
    public string GainMap { get; set; }
}