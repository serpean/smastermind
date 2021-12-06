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

        public int getWhites(Combination combination)
        {
            int whites = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                Color[] externalColors = combination.getColors();
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

        public int getBlacks(Combination combination)
        {
            int blacks = 0;
            for (int i = 0; i < colors.Length; i++)
            {
                Color[] externalColors = combination.getColors();
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

        public Color[] getColors()
        {
            return colors;
        }

        public override string ToString()
            {
                return String.Join("-", colors);
            }
        }
}
