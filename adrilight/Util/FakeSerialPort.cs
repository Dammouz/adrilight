﻿using NLog;

namespace adrilight.Util
{
    internal class FakeSerialPort : ISerialPortWrapper
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public FakeSerialPort() => _log.Warn("FakeSerialPort created!");

        public bool IsOpen { get; private set; }

        public void Open() => IsOpen = true;
        public void Close() => IsOpen = false;

        public void Dispose()
        {
        }

        private static readonly FpsLogger fpsLogger = new FpsLogger("FakeSerialPort");

        public void Write(byte[] outputBuffer, int v, int streamLength)
        {
            //_log.Warn($"Faking writing of {streamLength} bytes to the serial port");
            fpsLogger.TrackSingleFrame();
        }
    }
}
