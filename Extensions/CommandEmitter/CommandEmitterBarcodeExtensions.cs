using ESCPOS_NET.Emitters;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using ZXing;
using ZXing.Aztec;
using ZXing.Common;
using ZXing.Datamatrix;
using ZXing.PDF417;
using ZXing.PDF417.Internal;

namespace EPOSNext.Extensions.CommandEmitter;

public enum Pdf417AspectRatio
{
    A1 = 1,
    A2 = 2,
    A3 = 3,
    A4 = 4,
    AUTO = 5
}

public static partial class BaseCommandEmitterExtensions
{
    public static byte[] PrintSoftwarePdf417(this BaseCommandEmitter e, string contents,
        uint maxWidth = 0,
        bool compact = false,
        Pdf417AspectRatio aspectRatio = Pdf417AspectRatio.AUTO)
    {
        return e.PrintSoftware2DCode(BarcodeFormat.PDF_417,
            new PDF417EncodingOptions
            {
                Compact = compact,
                AspectRatio = (PDF417AspectRatio)aspectRatio
            }, contents, -1, (int)maxWidth);
    }

    public static byte[] PrintSoftwareDataMatrix(this BaseCommandEmitter e, string contents,
        uint size = 0)
    {
        return e.PrintSoftware2DCode(BarcodeFormat.DATA_MATRIX, new DatamatrixEncodingOptions(), contents, (int)size);
    }

    public static byte[] PrintSoftwareAztec(this BaseCommandEmitter e, string contents, uint size = 0)
    {
        return e.PrintSoftware2DCode(BarcodeFormat.AZTEC, new AztecEncodingOptions(), contents, (int)size);
    }

    private static byte[] PrintSoftware2DCode(this BaseCommandEmitter e, BarcodeFormat format, EncodingOptions options,
        string contents,
        int size = -1,
        int maxWidth = -1)
    {
        options.Margin = 0;
        options.NoPadding = true;
        options.PureBarcode = true;
        if (size != -1)
        {
            options.Width = size;
            options.Height = size;
        }

        var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
        {
            Format = format,
            Options = options
        };
        var matrix = writer.Encode(contents);
        var image = writer.Write(matrix);

        if (maxWidth > 0)
            image.Mutate(x =>
                x.Resize(maxWidth, matrix.Height / matrix.Width * maxWidth, new NearestNeighborResampler()));

        using var ms = new MemoryStream();
        image.Save(ms, new PngEncoder());
        return e.PrintImage(ms.ToArray(), true, true);
    }
}