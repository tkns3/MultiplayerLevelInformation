using SongCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;


namespace MultiplayerLevelInformation.Utils
{
    public class Levels
    {
        /// <summary>
        /// Get level cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetLevelCancellationTokenSource;
        /// <summary>
        /// Get status cancellation token
        /// </summary>
        private static CancellationTokenSource m_GetStatusCancellationTokenSource;

        /// <summary>
        /// Has a DLC level
        /// </summary>
        /// <param name="p_LevelID">Level ID</param>
        /// <param name="p_AdditionalContentModel">Additional content</param>
        /// <returns></returns>
        public static async Task<bool> HasDLCLevel(string p_LevelID, AdditionalContentModel p_AdditionalContentModel = null)
        {
            /*
               Code from https://github.com/MatrikMoon/TournamentAssistant

               MIT License

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
            p_AdditionalContentModel = p_AdditionalContentModel ?? Resources.FindObjectsOfTypeAll<AdditionalContentModel>().FirstOrDefault();
            if (p_AdditionalContentModel != null)
            {
                m_GetStatusCancellationTokenSource?.Cancel();
                m_GetStatusCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetStatusCancellationTokenSource.Token;

                return await p_AdditionalContentModel.GetLevelEntitlementStatusAsync(p_LevelID, l_Token) == AdditionalContentModel.EntitlementStatus.Owned;
            }

            return false;
        }

        /// <summary>
        /// Load a song by PreviewBeatmapLevel
        /// </summary>
        /// <param name="p_Level"></param>
        /// <param name="p_LoadCallback">Load callback</param>
        public static async Task LoadSong(IPreviewBeatmapLevel p_Level, Action<IBeatmapLevel> p_LoadCallback)
        {
            /*
               Code from https://github.com/MatrikMoon/TournamentAssistant

               MIT License

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

            /// Load IBeatmapLevel
            if (p_Level is PreviewBeatmapLevelSO || p_Level is CustomPreviewBeatmapLevel)
            {
                if (p_Level is PreviewBeatmapLevelSO)
                {
                    if (!await HasDLCLevel(p_Level.levelID).ConfigureAwait(false))
                    {
                        p_LoadCallback(null);
                        return; /// In the case of unowned DLC, just bail out and do nothing
                    }
                }

                var l_Result = await GetLevelFromPreview(p_Level).ConfigureAwait(false);
                if (l_Result != null && !(l_Result?.isError == true))
                {
                    /// HTTPstatus requires cover texture to be applied in here
                    var l_LoadedLevel = l_Result?.beatmapLevel;
                    //l_LoadedLevel.SetField("_coverImageTexture2D", l_Level.GetField<Texture2D>("_coverImageTexture2D"));

                    p_LoadCallback(l_LoadedLevel);
                }
                else
                    p_LoadCallback(null);
            }
            else if (p_Level is BeatmapLevelSO)
            {
                p_LoadCallback(p_Level as IBeatmapLevel);
            }
        }

        ////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Get a level from a level preview
        /// </summary>
        /// <param name="p_Level">Level instance</param>
        /// <param name="p_BeatmapLevelsModel">Model</param>
        /// <returns>Level instance</returns>
        private static async Task<BeatmapLevelsModel.GetBeatmapLevelResult?> GetLevelFromPreview(IPreviewBeatmapLevel p_Level, BeatmapLevelsModel p_BeatmapLevelsModel = null)
        {
            /*
                Code from https://github.com/MatrikMoon/TournamentAssistant

                MIT License

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
            p_BeatmapLevelsModel = p_BeatmapLevelsModel ?? Resources.FindObjectsOfTypeAll<BeatmapLevelsModel>().FirstOrDefault();

            if (p_BeatmapLevelsModel != null)
            {
                m_GetLevelCancellationTokenSource?.Cancel();
                m_GetLevelCancellationTokenSource = new CancellationTokenSource();

                var l_Token = m_GetLevelCancellationTokenSource.Token;

                BeatmapLevelsModel.GetBeatmapLevelResult? l_Result = null;

                try
                {
                    l_Result = await p_BeatmapLevelsModel.GetBeatmapLevelAsync(p_Level.levelID, l_Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {

                }

                if (l_Result?.isError == true || l_Result?.beatmapLevel == null)
                    return null; /// Null out entirely in case of error

                return l_Result;
            }

            return null;
        }
    }
}
