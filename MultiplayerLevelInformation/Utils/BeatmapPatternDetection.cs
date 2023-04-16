using System;
using System.Collections.Generic;

namespace MultiplayerLevelInformation.Utils
{
    public static class BeatmapPatternDetection
    {
        /*
            Code from https://github.com/kinsi55/BeatSaber_BetterSongList

            MIT License

            Copyright (c) 2021 Kinsi

            Permission is hereby granted, free of charge, to any person obtaining a copy
            of this software and associated documentation files (the "Software"), to deal
            in the Software without restriction, including without limitation the rights
            to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
            copies of the Software, and to permit persons to whom the Software is
            furnished to do so, subject to the following conditions:

            The above copyright notice and this permission notice shall be included in all
            copies or substantial portions of the Software.

            THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
            IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
            FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
            AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
            LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
            OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
            SOFTWARE.
        */

        public static bool CheckForCrouchWalls(List<BeatmapSaveDataVersion3.BeatmapSaveData.ObstacleData> obstacles)
        {
            if (obstacles == null || obstacles.Count == 0)
                return false;

            var wallExistence = new float[2];

            foreach (var o in obstacles)
            {
                // Ignore 1 wide walls on left
                if (o.line == 3 || (o.line == 0 && o.width == 1))
                    continue;

                // Filter out fake walls, they dont drain energy
                if (o.duration < 0 || o.width <= 0)
                    continue;

                // Detect >2 wide walls anywhere, or 2 wide wall in middle
                if (o.width > 2 || (o.width == 2 && o.line == 1))
                {
                    if (o.layer == 2 || o.layer != 0 && (o.height - o.layer >= 2))
                        return true;
                }

                // Is the wall on the left or right half?
                var isLeftHalf = o.line <= 1;

                // Check if the other half has an active wall, which would mean there is one on both halfs
                // I know this technically does not check if one of the halves is half-height, but whatever
                if (wallExistence[isLeftHalf ? 1 : 0] >= o.beat)
                    return true;

                // Extend wall lengths by 120ms so that staggered crouchwalls that dont overlap are caught
                wallExistence[isLeftHalf ? 0 : 1] = Math.Max(wallExistence[isLeftHalf ? 0 : 1], o.beat + o.duration + 0.12f);
            }
            return false;
        }
    }
}
