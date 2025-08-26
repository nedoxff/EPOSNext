using ESCPOS_NET.Emitters;
using ESCPOS_NET.Emitters.BaseCommandValues;

namespace EPOSNext.Extensions.CommandEmitter;

public enum CharacterFontType
{
    FontA = 0,
    FontB = 1,
    FontC = 2,
    FontD = 3,
    FontE = 4,
    SpecialFontA = 97,
    SpecialFontB = 98
}

public static partial class BaseCommandEmitterExtensions
{
    public static byte[] SelectCharacterFont(this BaseCommandEmitter e, CharacterFontType font) =>
        [Cmd.ESC, (byte)'M', (byte)font];

    public static byte[] ReverseColorsMode(this BaseCommandEmitter e, bool enabled) => [Cmd.GS, (byte)'B', (byte)(enabled ? 1 : 0)];
}