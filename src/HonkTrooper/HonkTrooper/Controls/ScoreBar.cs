using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace HonkTrooper
{
    public partial class ScoreBar : Border
    {
        #region Properties

        private double Score { get; set; } = 0;

        private TextBlock TextBlock { get; set; } = new TextBlock() { FontSize = 30, FontWeight = FontWeights.Bold, Foreground = new SolidColorBrush(Colors.White) };

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
            TextBlock.Text = Score.ToString("0000");
        }

        public void GainScore(int score)
        {
            Score += score;
            TextBlock.Text = Score.ToString("0000");
        }

        public void LooseScore(double score)
        {
            if (Score > 1)
            {
                Score -= score;
                TextBlock.Text = Score.ToString("0000");
            }
        }

        public double GetScore()
        {
            return Score;
        }
    }
}
