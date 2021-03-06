﻿using System.Drawing;

namespace adrilight
{
    public interface ISpotSet
    {
        int ExpectedScreenWidth { get; }
        int ExpectedScreenHeight { get; }

        ISpot[] Spots { get; set; }
        object Lock { get; }

        void IndicateMissingValues();
    }
}
