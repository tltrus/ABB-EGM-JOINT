using System;

namespace WpfApp.Models
{
    public class JointLimits
    {
        public double[] LowerBounds { get; private set; }
        public double[] UpperBounds { get; private set; }

        public JointLimits()
        {
            LowerBounds = new double[6];
            UpperBounds = new double[6];
        }

        public JointLimits(double[] lowerBounds, double[] upperBounds)
        {
            if (lowerBounds.Length != 6 || upperBounds.Length != 6)
                throw new ArgumentException("Arrays must contain exactly 6 elements");

            LowerBounds = lowerBounds;
            UpperBounds = upperBounds;
        }
    }
}