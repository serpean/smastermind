using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class Combination
    {
        public static int LENGTH = 4;
        protected Color[] colors;

        public int GetWhites(Combination combination)
        {
            int whites = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                Color[] externalColors = combination.GetColors();
                for (int j = 0; j < externalColors.Length; j++)
                {
                    if (colors[i] == externalColors[j] && i != j)
                    {
                        whites++;
                    }
                }
            }
            return whites;
        }

        public int GetBlacks(Combination combination)
        {
            int blacks = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                Color[] externalColors = combination.GetColors();
                for (int j = 0; j < externalColors.Length; j++)
                {
                    if (colors[i] == externalColors[j] && i == j)
                    {
                        blacks++;
                    }
                }
            }

            return blacks;
        }

        public Color[] GetColors()
        {
            return colors;
        }

        public override string ToString()
            {
                return String.Join("-", colors);
            }
        }
}
