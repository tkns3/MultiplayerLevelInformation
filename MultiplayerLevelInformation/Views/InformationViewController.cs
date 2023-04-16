using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Security.Policy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using BeatSaberMarkupLanguage;
using System.Threading;

namespace MultiplayerLevelInformation.Views
{
    internal class InformationViewController : BSMLResourceViewController
    {
        public override string ResourceName => "MultiplayerLevelInformation.Views.InformationViewController.bsml";

        /* // テスト用
        override public string Content
        {
            get
            {
                return System.IO.File.ReadAllText(@"C:\Program Files (x86)\Steam\steamapps\common\Beat Saber\UserData\z_.bsml");
            }
        }
        */

        public Action<SelectedByTarget> OnSelectedByChanged;

        public string LastPlayedLevelHash { get; set; }

        [UIComponent("cover-image")]
        private Image coverImage;

        [UIComponent("move-arrow-image")]
        private Image moveArrowImage;

        [UIComponent("own-button")]
        private ClickableText ownButton;

        [UIComponent("host-button")]
        private ClickableText hostButton;

        [UIComponent("status-text")]
        private TextMeshProUGUI statusText;

        [UIComponent("hash-text")]
        private TextMeshProUGUI hashText;

        [UIComponent("key-text")]
        private TextMeshProUGUI keyText;

        [UIComponent("mode-difficulty-text")]
        private TextMeshProUGUI modeDifficultyText;

        [UIComponent("song-text")]
        private TextMeshProUGUI songText;

        [UIComponent("mapper-text")]
        private TextMeshProUGUI mapperText;

        [UIComponent("bpm-text")]
        private TextMeshProUGUI bpmText;

        [UIComponent("duration-text")]
        private TextMeshProUGUI durationText;

        [UIComponent("star-text")]
        private TextMeshProUGUI starText;

        [UIComponent("nps-text")]
        private TextMeshProUGUI npsText;

        [UIComponent("note-text")]
        private TextMeshProUGUI noteText;

        [UIComponent("wall-text")]
        private TextMeshProUGUI wallText;

        [UIComponent("bomb-text")]
        private TextMeshProUGUI bombText;

        [UIComponent("njs-text")]
        private TextMeshProUGUI njsText;

        [UIComponent("jd-text")]
        private TextMeshProUGUI jdText;

        [UIComponent("offset-text")]
        private TextMeshProUGUI offsetText;

        [UIComponent("rt-text")]
        private TextMeshProUGUI rtText;

        [UIAction("ChangeSelectedByOwn")]
        private void ChangeSelectedByOwn()
        {
            ownButton.DefaultColor = Color.green;
            hostButton.DefaultColor = Color.white;
            OnSelectedByChanged?.Invoke(SelectedByTarget.Own);
        }

        [UIAction("ChangeSelectedByHost")]
        private void ChangeSelectedByHost()
        {
            ownButton.DefaultColor = Color.white;
            hostButton.DefaultColor = Color.green;
            OnSelectedByChanged?.Invoke(SelectedByTarget.Host);
        }

        [UIAction("OpenBeatServerURL")]
        private void OpenBeatServerURL()
        {
            if (keyText.text.Equals("") || keyText.text.Equals("-"))
            {
                return;
            }
            var pi = new System.Diagnostics.ProcessStartInfo()
            {
                FileName = $"https://beatsaver.com/maps/{keyText.text}",
                UseShellExecute = true,
            };
            _ = System.Diagnostics.Process.Start(pi);
        }

        [UIAction("JumpToSelectedSong")]
        private void JumpToSelectedSong()
        {
            if (hashText.text.Length > 0)
            {
                HMMainThreadDispatcher.instance.Enqueue(JumpToSong(hashText.text));
            }
        }

        [UIAction("JumpToLastPlayedSong")]
        private void JumpToLastPlayedSong()
        {
            if (LastPlayedLevelHash.Length > 0)
            {
                HMMainThreadDispatcher.instance.Enqueue(JumpToSong(LastPlayedLevelHash));
            }
        }

        private IEnumerator JumpToSong(string hash)
        {
            var x = UnityEngine.Object.FindObjectOfType<LevelCollectionTableView>();
            if (x == null)
            {
                Plugin.Log.Warn("not found LevelCollectionTableView.");
                yield break;
            }

            if (!x.gameObject.activeSelf)
            {
                Plugin.Log.Warn("LevelCollectionTableView is not active.");
                yield break;
            }

            CustomPreviewBeatmapLevel beatmap = SongCore.Loader.GetLevelByHash(hash);
            if (beatmap == null)
            {
                Plugin.Log.Warn($"beatmapLevel is not found. hash={hash}");
                yield break;
            }

            x.SelectLevel(beatmap);
        }

        public void SetTargetSelectMode(bool enableTargetSelect, bool ownSelected)
        {
            ownButton.DefaultColor = ownSelected ? Color.green : Color.white;
            hostButton.DefaultColor = !ownSelected ? Color.green : Color.white;

            hostButton.gameObject.SetActive(enableTargetSelect);
        }

        public void SetNotSelected()
        {
            statusText.text = "Not Selected.";
            statusText.color = Color.gray;
            SetMapInfo0();
        }

        public void SetNotSupportOfficial(LevelOverview level)
        {
            statusText.text = $"{level.levelID} is not custom level.";
            statusText.color = Color.magenta;
            SetMapInfo0();
        }

        public void SetDownloading(LevelOverview level)
        {
            statusText.text = "Downloading Map Information from Beat Saver.";
            statusText.color = Color.gray;
            SetMapInfo1(level);
        }

        public void SetDownloaded(LevelOverview selectedLevel, LevelDetail downloadedLevelDetail)
        {
            if (selectedLevel.hash == downloadedLevelDetail.hash)
            {
                statusText.text = "Success.";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = $"Re-Published. Selected HASH={selectedLevel.hash}.";
                statusText.color = Color.magenta;
            }
            SetMapInfo2(selectedLevel, downloadedLevelDetail);
        }

        public void SetFailed(LevelOverview level, string reason)
        {
            statusText.text = $"{reason}";
            statusText.color = Color.magenta;
            SetMapInfo1(level);
        }

        private void SetMapInfo0()
        {
            hashText.text = "-";
            keyText.text = "-";
            modeDifficultyText.text = "-";
            songText.text = "-";
            mapperText.text = "-";
            bpmText.text = "-";
            durationText.text = "-";
            starText.text = "-";
            npsText.text = "-";
            noteText.text = "-";
            wallText.text = "-";
            bombText.text = "-";
            njsText.text = "-";
            jdText.text = "-";
            offsetText.text = "-";
            rtText.text = "-";

            coverImage.SetImage("MultiplayerLevelInformation.Images._404.png");
        }

        private void SetMapInfo1(LevelOverview level)
        {
            hashText.text = level.hash;
            keyText.text = "-";
            modeDifficultyText.text = $"{level.mode} / {DifficultyString(level.difficulty, "")}";
            songText.text = "-";
            mapperText.text = "-";
            bpmText.text = "-";
            durationText.text = "-";
            starText.text = "-";
            npsText.text = "-";
            noteText.text = "-";
            wallText.text = "-";
            bombText.text = "-";
            njsText.text = "-";
            jdText.text = "-";
            offsetText.text = "-";
            rtText.text = "-";

            coverImage.SetImage("MultiplayerLevelInformation.Images._404.png");
        }

        private void SetMapInfo2(LevelOverview level, LevelDetail levelDetail)
        {
            hashText.text = levelDetail.hash;
            keyText.text = levelDetail.key;
            modeDifficultyText.text = $"{levelDetail.mode} / {DifficultyString(levelDetail.difficulty, levelDetail.difficultyLabel)}";
            songText.text = levelDetail.songFullName;
            mapperText.text = $"[{levelDetail.mapperName}]";
            bpmText.text = $"{levelDetail.bpm:0.##}";
            durationText.text = $"{(int)levelDetail.duration / 60}:{(int)levelDetail.duration % 60:00}";
            starText.text = (levelDetail.star > 0) ? $"{levelDetail.star:0.##}" : "-";
            npsText.text = $"{levelDetail.nps:0.##}";
            noteText.text = $"{levelDetail.notes}";
            wallText.text = $"{levelDetail.ob}";
            bombText.text = $"{levelDetail.bombs}";
            njsText.text = $"{levelDetail.njs:0.##}";
            jdText.text = $"{levelDetail.jd:0.##}";
            offsetText.text = $"{levelDetail.offset:0.##}";
            rtText.text = $"{levelDetail.rt:0.##}";

            var previewBeatmap = SongCore.Loader.GetLevelByHash(level.hash);
            if (previewBeatmap == null)
            {
                StartCoroutine(downloadCoverImage());

                IEnumerator downloadCoverImage()
                {
                    Plugin.Log.Info(levelDetail.imageUrl);
                    var www = new WWW(levelDetail.imageUrl);
                    yield return www;
                    coverImage.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), Vector2.zero);
                }
            }
            else
            {
                getCoverImage();
                checkCrouchWalls();

                async void getCoverImage()
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    CancellationToken token = cts.Token;
                    var sprite = await previewBeatmap.GetCoverImageAsync(token);
                    coverImage.sprite = sprite;
                }

                async void checkCrouchWalls()
                {
                    await Utils.Levels.LoadSong(previewBeatmap, (IBeatmapLevel beatmap) =>
                    {
                        IDifficultyBeatmap getDifficultyBeatmap()
                        {
                            foreach (var c in beatmap.previewDifficultyBeatmapSets)
                            {
                                if (c.beatmapCharacteristic.serializedName == levelDetail.mode)
                                {
                                    return beatmap.beatmapLevelData.GetDifficultyBeatmap(c.beatmapCharacteristic, levelDetail.difficulty);
                                }
                            }
                            return null;
                        }

                        var idb = getDifficultyBeatmap();
                        if (idb is CustomDifficultyBeatmap customdiff)
                        {
                            if (Utils.BeatmapPatternDetection.CheckForCrouchWalls(customdiff.beatmapSaveData.obstacles))
                            {
                                wallText.text += " <b><size=3.0><color=#FF0>⚠</color></size></b>";
                            }
                        }
                    });
                }
            }
        }

        private static string DifficultyString(BeatmapDifficulty difficulty, string label)
        {
            string getDifficultyString()
            {
                switch (difficulty)
                {
                    case BeatmapDifficulty.Easy: return "Easy";
                    case BeatmapDifficulty.Normal: return "Normal";
                    case BeatmapDifficulty.Hard: return "Hard";
                    case BeatmapDifficulty.Expert: return "Expert";
                    case BeatmapDifficulty.ExpertPlus: return "Expert+";
                    default: return "Unknown";
                }
            }
            string s = getDifficultyString();
            if (label.Length > 0)
            {
                s += $" ({label})";
            }
            return s;
        }

        public void SetMoveArrowColor(Color color)
        {
            moveArrowImage.color = color;
        }
    }
}
