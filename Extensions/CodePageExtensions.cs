using System.Text;
using ESCPOS_NET.Emitters;

namespace EPOSNext.Extensions;

public static class CodePageExtensions
{
    public static Encoding ToEncoding(this CodePage codePage)
    {
        var id = codePage switch
        {
            CodePage.PC437_USA_STANDARD_EUROPE_DEFAULT => 437,
            CodePage.KATAKANA => throw new NotSupportedException(),
            CodePage.PC850_MULTILINGUAL => 850,
            CodePage.PC860_PORTUGUESE => 860,
            CodePage.PC863_CANADIAN_FRENCH => 863,
            CodePage.PC865_NORDIC => 865,
            CodePage.HIRAGANA => throw new NotSupportedException(),
            CodePage.ONE_PASS_KANJI => throw new NotSupportedException(),
            CodePage.ONE_PASS_KANJI2 => throw new NotSupportedException(),
            CodePage.PC851_GREEK => 851,
            CodePage.PC853_TURKISH => 853,
            CodePage.PC857_TURKISH => 857,
            CodePage.PC737_GREEK => 737,
            CodePage.ISO8859_7_GREEK => 28597,
            CodePage.WPC1252 => 1252,
            CodePage.PC866_CYRILLIC2 => 866,
            CodePage.PC852_LATIN2 => 852,
            CodePage.PC858_EURO => 858,
            CodePage.KU42_THAI => throw new NotSupportedException(),
            CodePage.TIS11_THAI => throw new NotSupportedException(),
            CodePage.TIS13_THAI => throw new NotSupportedException(),
            CodePage.TIS14_THAI => throw new NotSupportedException(),
            CodePage.TIS16_THAI => throw new NotSupportedException(),
            CodePage.TIS17_THAI => throw new NotSupportedException(),
            CodePage.TIS18_THAI => throw new NotSupportedException(),
            CodePage.TCVN3_VIETNAMESE_L => throw new NotSupportedException(),
            CodePage.TCVN3_VIETNAMESE_U => throw new NotSupportedException(),
            CodePage.PC720_ARABIC => 720,
            CodePage.WPC775_BALTIC_RIM => 775,
            CodePage.PC855_CYRILLIC => 855,
            CodePage.PC861_ICELANDIC => 861,
            CodePage.PC862_HEBREW => 862,
            CodePage.PC864_ARABIC => 864,
            CodePage.PC869_GREEK => 869,
            CodePage.ISO8859_2_LATIN2 => 28592,
            CodePage.ISO8859_15_LATIN9 => 28605,
            CodePage.PC1098_FARSI => 1098,
            CodePage.PC1118_LITHUANIAN => 1118,
            CodePage.PC1119_LITHUANIAN => 1119,
            CodePage.PC1125_UKRANIAN => 1125,
            CodePage.WPC1250_LATIN2 => 1250,
            CodePage.WPC1251_CYRILLIC => 1251,
            CodePage.WPC1253_GREEK => 1253,
            CodePage.WPC1254_TURKISH => 1254,
            CodePage.WPC1255_HEBREW => 1255,
            CodePage.WPC1256_ARABIC => 1256,
            CodePage.WPC1257_BALTIC_RIM => 1257,
            CodePage.WPC1258_VIETNAMESE => 1258,
            CodePage.KZ1048_KAZAKHSTAN => 1048,
            CodePage.DEVANAGARI => 57002,
            CodePage.BENGALI => 57003,
            CodePage.TAMIL => 57004,
            CodePage.TELUGU => 57005,
            CodePage.ASSAMESE => 57006,
            CodePage.ORIYA => 57007,
            CodePage.KANNADA => 57008,
            CodePage.MALAYALAM => 57009,
            CodePage.GUJARATI => 57010,
            CodePage.PUNJABI => 57011,
            CodePage.MARATHI => 57002,
            _ => throw new ArgumentOutOfRangeException(nameof(codePage), codePage, null)
        };

        return Encoding.GetEncoding(id);
    }
}