using static System.Formats.Asn1.AsnWriter;

namespace HonkTrooper
{
    public partial class Threashold
    {
        #region Ctor

        public Threashold(double value)
        {
            Reset(value);
        }

        #endregion

        #region Properties

        private double ReleasePoint { get; set; } = 0;

        private double ReleasePointDifference { get; set; }

        #endregion

        #region Methods

        public double GetReleasePointDifference()
        {
            return ReleasePointDifference;
        }

        public bool ShouldRelease(double score)
        {
            var release = score - ReleasePoint > ReleasePointDifference;

            return release;
        }

        public void IncreaseReleasePoint(double value, double score)
        {
            ReleasePoint = score;
            ReleasePointDifference += value;
        }

        public void Reset(double value)
        {
            ReleasePointDifference = value;
        }

        #endregion
    }
}
