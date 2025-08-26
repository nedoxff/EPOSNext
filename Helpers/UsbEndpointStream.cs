using LibUsbDotNet.LibUsb;

namespace EPOSNext.Helpers;

// adapted from https://github.com/LibUsbDotNet/LibUsbDotNet/blob/master/src/LibUsbDotNet/Main/UsbStream.cs
// to include a configurable timeout
public class UsbEndpointStream : Stream
{
    private readonly byte[] _readBuffer = new byte[4096];
    private readonly UsbEndpointReader _reader;
    private readonly TimeSpan _readTimeout;
    private readonly UsbEndpointWriter _writer;
    private readonly TimeSpan _writeTimeout;
    private long _position;
    private int _readBufferLength;
    private int _readBufferOffset;

    public UsbEndpointStream(UsbEndpointWriter writer, UsbEndpointReader reader, TimeSpan readTimeout,
        TimeSpan writeTimeout)
    {
        if (writer == null && reader == null)
            throw new ArgumentException("At least a reader or a writer must be provided");
        _writer = writer;
        _reader = reader;
        _readTimeout = readTimeout;
        _writeTimeout = writeTimeout;
    }

    public override bool CanRead => _reader != null;
    public override bool CanSeek => false;
    public override bool CanWrite => _writer != null;
    public override long Length => throw new NotSupportedException();

    public override long Position
    {
        get => _position;
        set => throw new NotSupportedException();
    }


    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (count == 0) return 0;
        if (_readBufferOffset >= _readBufferLength)
        {
            _readBufferOffset = 0;
            _reader.Read(_readBuffer, _readBufferOffset, _readBuffer.Length, _readTimeout.Milliseconds,
                out var transferLength).ThrowOnError();
            _readBufferLength = transferLength;
        }

        var bytesAvailable = _readBufferLength - _readBufferOffset;
        var read = Math.Min(bytesAvailable, count);
        Array.Copy(_readBuffer, _readBufferOffset, buffer, offset, read);

        _readBufferOffset += read;
        _position += read;
        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _writer.Write(buffer, offset, count, _writeTimeout.Milliseconds, out _).ThrowOnError();
    }
}