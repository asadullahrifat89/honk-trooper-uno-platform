using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Windows.UI;
using static System.Formats.Asn1.AsnWriter;

namespace HonkTrooper
{
    public partial class ScoreBar : Border
    {
        #region Properties

        private int BossPointScore { get; set; } = 0;

        private int BossPointScoreDifference { get; set; } = 50;

        private int Score { get; set; } = 0;

        private TextBlock TextBlock { get; set; } = new TextBlock() { FontSize = 30, FontWeight = FontWeights.Bold };

        #endregion

        public ScoreBar()
        {
            VerticalAlignment = VerticalAlignment.Center;
            HorizontalAlignment = HorizontalAlignment.Center;

            this.Child = TextBlock;
            GainScore(0);
        }

        public void Reset()
        {
            Score = 0;
            BossPointScoreDifference = 50;
            TextBlock.Text = Score.ToString("0000");
        }

        public void GainScore(int score)
        {
            Score += score;
            TextBlock.Text = Score.ToString("0000");
        }

        public int GetScore()
        {
            return Score;
        }

        public int GetBossPointScoreDifference()
        {
            return BossPointScoreDifference;
        }

        public bool IsBossPointScore()
        {
            var bossPoint = Score - BossPointScore > BossPointScoreDifference;

            if (bossPoint)
            {
                BossPointScore = Score;
                BossPointScoreDifference += 10;
            }

            return bossPoint;
        }
    }
}
