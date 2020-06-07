using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using adrilight.DesktopDuplication;
using adrilight.Extensions;
using MoreLinq;
using NLog;

namespace adrilight
{
    internal sealed class SpotSet : ISpotSet
    {
        private ILogger _log = LogManager.GetCurrentClassLogger();

        public SpotSet(IUserSettings userSettings)
        {
            UserSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));


            UserSettings.PropertyChanged += (_, e) => DecideRefresh(e.PropertyName);
            Refresh();

            _log.Info($"SpotSet created.");
        }

        private void DecideRefresh(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(UserSettings.BorderDistanceX):
                case nameof(UserSettings.BorderDistanceY):
                case nameof(UserSettings.MirrorX):
                case nameof(UserSettings.MirrorY):
                case nameof(UserSettings.OffsetLed):
                case nameof(UserSettings.SpotHeight):
                case nameof(UserSettings.SpotsX):
                case nameof(UserSettings.SpotsY):
                case nameof(UserSettings.SpotWidth):
                    Refresh();
                    break;
            }
        }

        public ISpot[] Spots { get; set; }

        public object Lock { get; } = new object();

        /// <summary>
        /// returns the number of leds
        /// </summary>
        public static int CountLeds(int spotsX, int spotsY)
        {
            return 2 * spotsX + 2 * spotsY;
        }

        public int ExpectedScreenWidth => Screen.PrimaryScreen.Bounds.Width / DesktopDuplicator.ScalingFactor;
        public int ExpectedScreenHeight => Screen.PrimaryScreen.Bounds.Height / DesktopDuplicator.ScalingFactor;

        private IUserSettings UserSettings { get; }

        private void Refresh()
        {
            lock (Lock)
            {
                Spots = BuildSpots(ExpectedScreenWidth, ExpectedScreenHeight, UserSettings);
            }
        }

        internal static IEnumerable<(int x, int y)> BoundsWalker(int horizontalStripCount, int verticalStripCount)
        {
            if (horizontalStripCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(horizontalStripCount));
            }
            if (verticalStripCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(verticalStripCount));
            }

            /* Counting direction is clockwise:
             *
             *    0123
             *    9  4
             *    8765
             *
             * Number of expected entries = 2 * horizontalStripCount + 2 * verticalStripCount
             *
             * Ranges are
             * 1..horizontalStripCount, 0  = top
             * horizontalStripCount+1, 1..verticalStripCount  = right
             * horizontalStripCount..1, verticalStripCount+1  = bottom
             * 0, verticalStripCount..1)  = left
             */

            // Top
            for (var x = 1; x <= horizontalStripCount; x++)
            {
                yield return (x, 0);
            }

            // Right
            for (var y = 1; y <= verticalStripCount; y++)
            {
                yield return (horizontalStripCount + 1, y);
            }

            // Bottom
            for (var x = horizontalStripCount; x >= 1; x--)
            {
                yield return (x, verticalStripCount + 1);
            }

            // Left
            for (var y = verticalStripCount; y >= 1; y--)
            {
                yield return (0, y);
            }
        }

        internal static ISpot[] BuildSpots(int screenWidth, int screenHeight, IUserSettings userSettings)
        {
            var spotsX = userSettings.SpotsX;
            var spotsY = userSettings.SpotsY;

            var scalingFactor = DesktopDuplicator.ScalingFactor;
            var borderDistanceX = userSettings.BorderDistanceX / scalingFactor;
            var borderDistanceY = userSettings.BorderDistanceY / scalingFactor;
            var spotWidth = userSettings.SpotWidth / scalingFactor;
            var spotHeight = userSettings.SpotHeight / scalingFactor;

            //var counter = 0;
            //var relationIndex = spotsX - spotsY + 1;

            var interSpotGapX = (screenWidth - 2 * borderDistanceX - (spotsX + 2) * spotWidth) * 1.0 / (spotsX + 2 - 1);
            var interSpotGapY = (screenHeight - 2 * borderDistanceY - (spotsY + 2) * spotHeight) * 1.0 / (spotsY + 2 - 1);

            ISpot[] spots = BoundsWalker(spotsX, spotsY)
                .Select(p =>
                {
                    // Left row
                    var left = 0;
                    if (p.x == spotsX + 1)
                    {
                        // Right row
                        left = screenWidth - borderDistanceX - spotWidth;
                    }
                    else
                    {
                        // Top / bottom row
                        left = borderDistanceX + (p.x) * spotWidth + (int)((p.x) * interSpotGapX);
                    }

                    // Top row
                    var top = 0;
                    if (p.y == spotsY + 1)
                    {
                        // Bottom row
                        top = screenHeight - borderDistanceY - spotHeight;
                    }
                    else
                    {
                        // Left / right row
                        top = borderDistanceY + (p.y) * spotHeight + (int)((p.y) * interSpotGapY);
                    }

                    Debug.Assert(top >= 0, "top >= 0");
                    Debug.Assert(top <= screenHeight - spotHeight, "top <= screenHeight-spotHeight");
                    Debug.Assert(left >= 0, "left >= 0");
                    Debug.Assert(left <= screenWidth - spotWidth, "left <= screenWidth - spotWidth");

                    return new Spot(left, top, spotWidth, spotHeight);
                })
                .ToArray();

            if (userSettings.OffsetLed != 0)
            {
                spots = Enumerable.Concat(spots.TakeLast(userSettings.OffsetLed),
                        spots.Take(spots.Length - userSettings.OffsetLed))
                    .ToArray();
            }
            if (spotsY > 1 && userSettings.MirrorX)
            {
                MirrorX(spots, spotsX, spotsY + 2);
            }
            if (spotsX > 1 && userSettings.MirrorY)
            {
                MirrorY(spots, spotsX, spotsY + 2);
            }

            spots[0].IsFirst = true;
            return spots;
        }

        private static void Mirror(ISpot[] spots, int startIndex, int length)
        {
            var halfLength = length / 2;
            var endIndex = startIndex + length - 1;

            for (var i = 0; i < halfLength; i++)
            {
                spots.Swap(startIndex + i, endIndex - i);
            }
        }

        private static void MirrorX(ISpot[] spots, int spotsX, int spotsY)
        {
            // Copy swap last row to first row inverse
            for (var i = 0; i < spotsX; i++)
            {
                var index1 = i;
                var index2 = (spots.Length - 1) - (spotsY - 2) - i;
                spots.Swap(index1, index2);
            }

            // Mirror first column
            Mirror(spots, spotsX, spotsY - 2);

            // Mirror last column
            if (spotsX > 1)
            {
                Mirror(spots, 2 * spotsX + spotsY - 2, spotsY - 2);
            }
        }

        private static void MirrorY(ISpot[] spots, int spotsX, int spotsY)
        {
            // Copy swap last row to first row inverse
            for (var i = 0; i < spotsY - 2; i++)
            {
                var index1 = spotsX + i;
                var index2 = (spots.Length - 1) - i;
                spots.Swap(index1, index2);
            }

            // Mirror first row
            Mirror(spots, 0, spotsX);

            // Mirror last row
            if (spotsY > 1)
            {
                Mirror(spots, spotsX + spotsY - 2, spotsX);
            }
        }

        private static void Offset(ref ISpot[] spots, int offset)
        {
            spots = Enumerable.Concat(spots.TakeLast(offset), spots.Take(spots.Length - offset)).ToArray();
        }

        public void IndicateMissingValues()
        {
            foreach (var spot in Spots)
            {
                spot.IndicateMissingValue();
            }
        }
    }
}
