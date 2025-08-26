using ESCPOS_NET;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

namespace EPOSNext.Printers;

public record BluetoothPrinterOptions
{
    public static readonly BluetoothPrinterOptions Default = new();
    public bool Immediate { get; init; }
    public bool CheckPairedDevices { get; init; } = true;
    public bool AutoConnect { get; init; } = true;
}

public class BluetoothPrinter : BasePrinter
{
    private readonly BluetoothClient _client;
    private readonly BluetoothDeviceInfo _device;
    private readonly BluetoothPrinterOptions _options;
    private readonly string _pin;

    private BluetoothPrinter(Func<BluetoothDeviceInfo, bool> finder, BluetoothPrinterOptions options, string pin)
    {
        _client = new BluetoothClient();
        _options = options ?? BluetoothPrinterOptions.Default;
        _pin = pin;

        if (_options.CheckPairedDevices)
        {
            var pairedDevices = _client.PairedDevices;
            var matchingPairedDevice = pairedDevices.FirstOrDefault(finder);
            if (matchingPairedDevice != null) _device = matchingPairedDevice;
        }

        if (_device == null)
        {
            var devices = _client.DiscoverDevices();
            var matchingDevice = devices.FirstOrDefault(finder);
            _device = matchingDevice ??
                      throw new Exception("No matching Bluetooth device found with specified criteria");
        }

        if (_options.AutoConnect) Connect();
    }

    public static BluetoothPrinter FromDeviceName(string name, string pin = null,
        BluetoothPrinterOptions options = null)
    {
        return new BluetoothPrinter(d => d.DeviceName == name, options, pin);
    }

    public static BluetoothPrinter FromDeviceAddress(string address, string pin = null,
        BluetoothPrinterOptions options = null)
    {
        var validAddress = BluetoothAddress.TryParse(address, out var parsedAddress);
        if (!validAddress) throw new Exception($"Invalid Bluetooth address \"{address}\"");
        return new BluetoothPrinter(d => d.DeviceAddress == parsedAddress, options, pin);
    }

    public void Connect()
    {
        if (!_device.Authenticated)
        {
            var paired = BluetoothSecurity.PairRequest(_device.DeviceAddress, _pin);
            if (!paired)
                throw new Exception(
                    $"Failed to pair with {_device.DeviceAddress}/${_device.DeviceName} (is the PIN correct?)");
        }

        _device.Refresh();
        _client.Connect(_device.DeviceAddress, BluetoothService.SerialPort);
        if (!_client.Connected)
            throw new Exception(
                $"Failed to connect to {_device.DeviceAddress}/${_device.DeviceName}");

        var stream = _client.GetStream();
        Reader = new BinaryReader(stream);
        Writer = new BinaryWriter(stream);
    }

    public override void Write(byte[] bytes)
    {
        if (_options.Immediate) Writer.Write(bytes);
        else base.Write(bytes);
    }
}