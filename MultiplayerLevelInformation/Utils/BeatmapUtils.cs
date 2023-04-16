using System;

namespace MultiplayerLevelInformation.Utils
{
    internal static class BeatmapUtils
    {
        static double hjd(double bpm, double njs, double offset)
        {
            var num = 60f / bpm;
            var hjd = 4d;
            while (njs * num * hjd > 17.999f)
                hjd /= 2f;

            hjd += offset;

            return Math.Max(hjd, 0.25d);
        }

        public static double GetJd(double bpm, double njs, double offset)
        {
            return njs * (60f / bpm) * hjd(bpm, njs, offset) * 2;
        }

        public static double GetRT(double bpm, double njs, double offset)
        {
            return GetJd(bpm, njs, offset) / (2 * njs);
        }
    }
}
