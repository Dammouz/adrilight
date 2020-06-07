using System;
using System.IO.Ports;

namespace adrilight.Util
{
    internal class WrappedSerialPort : ISerialPortWrapper
    {
        public WrappedSerialPort(SerialPort serialPort)
        {
            SerialPort = serialPort ?? throw new ArgumentNullException(nameof(serialPort));
        }

        private SerialPort SerialPort { get; }

        public bool IsOpen => SerialPort.IsOpen;

        public void Close() => SerialPort.Close();

        public void Open() => SerialPort.Open();

        public void Write(byte[] outputBuffer, int v, int streamLength) => SerialPort.Write(outputBuffer, v, streamLength);

        public void Dispose() => SerialPort.Dispose();
    }
}
