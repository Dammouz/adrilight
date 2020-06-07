using System;
using System.Linq;
using Microsoft.ML.Data;

namespace adrilight.Util.NightLightDetection
{
    internal class NightLightDataRow
    {
        public bool IsActive { get; }

        //[VectorType(43)]
        public byte[] Data { get; }

        [VectorType(35)]
        public float[] Data2 { get; }

        public NightLightDataRow()
        {
        }

        public NightLightDataRow(byte[] data, bool isActive)
        {
            Data = data;
            Data2 = data.Concat(Enumerable.Repeat((byte)0, Math.Max(0, 35 - data.Length))).Select(d => (float)d).Take(35).ToArray();
            IsActive = isActive;
        }
    }
}
