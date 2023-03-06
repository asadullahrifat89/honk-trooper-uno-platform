using Microsoft.UI.Text;
using Microsoft.UI.Xaml.Controls;

namespace HonkTrooper
{
    public partial class ScoreBar : Border
    {
        private int BossPointScore { get; set; } = 0;

        private int Score { get; set; } = 0;

        private TextBlock TextBlock { get; set; } = new TextBlock() { FontSize = 40, FontWeight = FontWeights.Bold };

        public ScoreBar()
        {
            this.Child = TextBlock;
            GainScore(0);
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

        public bool IsBossPointScore(int scoreDiff)
        {
            var bossPoint = Score - BossPointScore > scoreDiff;

            if (bossPoint)
                BossPointScore = Score;

            return bossPoint;
        }
    }
}
