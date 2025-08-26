using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace EPOSNext.Extensions;

public static class UsbContextExtensions
{
    public static IUsbDevice FindWithOpening(this UsbContext ctx, Func<IUsbDevice, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        using var collection = ctx.List();
        foreach (var device in collection)
        {
            var open = device.TryOpen();
            if (!open) continue;
            var match = predicate(device);
            device.Close();
            if (match) return device.Clone();
        }

        return null;
    }

    public static IUsbDevice FindWithOpening(this UsbContext ctx, UsbDeviceFinder finder)
    {
        ArgumentNullException.ThrowIfNull(finder);
        return ctx.FindWithOpening(finder.Check);
    }
}