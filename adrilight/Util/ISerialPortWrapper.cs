using System;

namespace adrilight.Util
{
    interface ISerialPortWrapper : IDisposable
    {
        bool IsOpen { get; }

        void Close();
        void Open();
        void Write(byte[] outputBuffer, int v, int streamLength);
    }
}
