using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;
using ESCPOS_NET.Utilities;

namespace EPOSNext.Extensions.CommandEmitter;

public static partial class BaseCommandEmitterExtensions
{
    public static byte[] PrintLocalized(this BaseCommandEmitter e, CodePage page, string contents)
    {
        return ByteSplicer.Combine(
            e.CodePage(page),
            page.ToEncoding().GetBytes(contents.Replace("\r\n", "\n").Replace("\r", "\n"))
        );
    }

    public static byte[] PrintLineLocalized(this BaseCommandEmitter e, CodePage page, string contents)
    {
        return e.PrintLocalized(page, contents.Replace("\r", string.Empty).Replace("\n", string.Empty) + "\n");
    }

    public static byte[] SkipLines(this BaseCommandEmitter e, int amount)
    {
        return ByteSplicer.Combine(Enumerable.Repeat(e.PrintLine(""), amount).ToArray());
    }
}