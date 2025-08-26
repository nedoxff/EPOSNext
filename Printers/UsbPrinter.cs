using EPOSNext.Extensions;
using EPOSNext.Helpers;
using ESCPOS_NET;
using LibUsbDotNet.LibUsb;
using LibUsbDotNet.Main;

namespace EPOSNext.Printers;

public record UsbPrinterOptions
{
    public static readonly UsbPrinterOptions Default = new();
    public bool Immediate { get; init; }
    public TimeSpan ReadTimeout { get; init; } = TimeSpan.FromSeconds(1);
    public TimeSpan WriteTimeout { get; init; } = TimeSpan.FromSeconds(1);
}

public class UsbPrinter : BasePrinter
{
    private readonly UsbPrinterOptions _options;

    private UsbPrinter(UsbDeviceFinder finder, UsbPrinterOptions options)
    {
        var context = new UsbContext();
        var device = context.FindWithOpening(finder) ??
                     throw new Exception("No USB device was found with specified criteria");
        _options = options ?? UsbPrinterOptions.Default;

        var opened = device.TryOpen();
        if (!opened) throw new IOException("Failed to open the USB device (is it busy/used by other applications?)");
        var claimed = device.ClaimInterface(device.Configs.First().Interfaces.First().Number);
        if (!claimed)
            throw new IOException(
                "Failed to claim interface for the USB device (is it busy/used by other applications?)");

        var stream = new UsbEndpointStream(device.OpenEndpointWriter(WriteEndpointID.Ep01),
            device.OpenEndpointReader(ReadEndpointID.Ep01), _options.ReadTimeout, _options.WriteTimeout);
        Reader = new BinaryReader(stream);
        Writer = new BinaryWriter(stream);
    }

    public static UsbPrinter FromIds(int vid, int pid, UsbPrinterOptions options = null)
    {
        return new UsbPrinter(new UsbDeviceFinder { Pid = pid, Vid = vid }, options);
    }

    public static UsbPrinter FromSerial(string serial, UsbPrinterOptions options = null)
    {
        return new UsbPrinter(new UsbDeviceFinder { SerialNumber = serial }, options);
    }

    public override void Write(byte[] bytes)
    {
        if (_options.Immediate) Writer.Write(bytes);
        else base.Write(bytes);
    }
}