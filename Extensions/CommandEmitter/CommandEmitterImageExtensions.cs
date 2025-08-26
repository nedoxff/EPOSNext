using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Utilities;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Dithering;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using Color = SixLabors.ImageSharp.Color;

namespace EPOSNext.Extensions.CommandEmitter;

public enum ImageDitherMode
{
    // error dither
    Atkinson,
    Burkes,
    FloydSteinberg,
    JarvisJudiceNinke,
    Sierra2,
    Sierra3,
    SierraLite,
    StevensonArce,
    Stucki,

    // ordered dither
    Bayer16x16,
    Bayer2x2,
    Bayer4x4,
    Bayer8x8,
    Ordered3x3
}

public enum InlineImageDotDensity
{
    DefaultDensity = 8,
    HighDensity = 24
}

public static partial class BaseCommandEmitterExtensions
{
    public static byte[] PrintImageInline(this BaseCommandEmitter e, byte[] image,
        InlineImageDotDensity dotDensity = InlineImageDotDensity.HighDensity,
        ImageDitherMode mode = ImageDitherMode.Stucki)
    {
        var imageCommand = new ByteArrayBuilder();
        using var img = Image.Load<Rgba32>(image);
        img.PrepareForPrinting(mode.ToDither(), int.MaxValue, (int)dotDensity);

        var bytesPerColumn = (int)dotDensity / 8;
        var densityByte = (byte)(dotDensity == InlineImageDotDensity.HighDensity ? 0x21 : 0x01);
        for (var x = 0; x < img.Width; x++)
        {
            imageCommand.Append([Cmd.ESC, (byte)'*', densityByte, 0x1, 0x0]);
            var segment = new byte[bytesPerColumn];
            for (var y = 0; y < img.Height; y++)
            {
                if (!img[x, y].IsBlack()) continue;
                segment[y / 8] |= (byte)(0x01 << (7 - y % 8));
            }

            imageCommand.Append(segment);
        }

        return imageCommand.ToArray();
    }

    public static byte[] PrintImageDithered(this BaseCommandEmitter e, byte[] image,
        ImageDitherMode ditherMode = ImageDitherMode.Stucki,
        int maxWidth = int.MaxValue)
    {
        var imageCommand = new ByteArrayBuilder();
        using var img = Image.Load<Rgba32>(image);
        var imageData = img.CustomToSingleBitPixelByteArray(ditherMode.ToDither(), maxWidth);

        var heightL = (byte)img.Height;
        var heightH = (byte)(img.Height >> 8);
        var byteWidth = ((img.Width + 7) & -8) / 8;
        var widthL = (byte)byteWidth;
        var widthH = (byte)(byteWidth >> 8);

        imageCommand.Append([Cmd.GS, Images.ImageCmdLegacy, 0x30, 0x00, widthL, widthH, heightL, heightH]);
        imageCommand.Append(imageData);

        return imageCommand.ToArray();
    }

    private static IDither ToDither(this ImageDitherMode dither)
    {
        return dither switch
        {
            ImageDitherMode.Atkinson => ErrorDither.Atkinson,
            ImageDitherMode.Burkes => ErrorDither.Burkes,
            ImageDitherMode.FloydSteinberg => ErrorDither.FloydSteinberg,
            ImageDitherMode.JarvisJudiceNinke => ErrorDither.JarvisJudiceNinke,
            ImageDitherMode.Sierra2 => ErrorDither.Sierra2,
            ImageDitherMode.Sierra3 => ErrorDither.Sierra3,
            ImageDitherMode.SierraLite => ErrorDither.SierraLite,
            ImageDitherMode.StevensonArce => ErrorDither.StevensonArce,
            ImageDitherMode.Stucki => ErrorDither.Stucki,
            ImageDitherMode.Bayer16x16 => OrderedDither.Bayer16x16,
            ImageDitherMode.Bayer2x2 => OrderedDither.Bayer2x2,
            ImageDitherMode.Bayer4x4 => OrderedDither.Bayer4x4,
            ImageDitherMode.Bayer8x8 => OrderedDither.Bayer8x8,
            ImageDitherMode.Ordered3x3 => OrderedDither.Ordered3x3,
            _ => throw new ArgumentOutOfRangeException(nameof(dither), dither, null)
        };
    }

    private static void PrepareForPrinting(this Image<Rgba32> image, IDither dither, int maxWidth = int.MaxValue,
        int maxHeight = int.MaxValue)
    {
        image.Mutate(img =>
        {
            if (maxWidth != int.MaxValue || maxHeight != int.MaxValue)
                img.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight),
                    Sampler = new NearestNeighborResampler()
                });

            img.BackgroundColor(Color.White);
            img.Grayscale().BinaryDither(dither);
        });
    }

    private static byte[] CustomToSingleBitPixelByteArray(this Image<Rgba32> image, IDither dither,
        int maxWidth = int.MaxValue)
    {
        image.PrepareForPrinting(dither, maxWidth);
        var bytesPerRow = ((image.Width + 7) & -8) / 8;
        var result = new byte[bytesPerRow * image.Height];

        image.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                var row = accessor.GetRowSpan(y);
                var rowStartPosition = y * bytesPerRow;
                for (var x = 0; x < row.Length; x++)
                {
                    if (!row[x].IsBlack()) continue;
                    result[rowStartPosition + x / 8] |= (byte)(0x01 << (7 - x % 8));
                }
            }
        });

        return result;
    }

    private static bool IsBlack(this Rgba32 color)
    {
        return color is { R: 0, G: 0, B: 0 };
    }
}