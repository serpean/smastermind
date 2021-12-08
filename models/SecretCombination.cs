using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class SecretCombination : Combination
    {
        public SecretCombination()
        {
            Color[] colors = new Color[Combination.LENGTH];
            // Generate Combination.LENGTH differentes ints.
            Array allColors = Enum.GetValues(typeof(Color));
            int[] randoms = GenerateRandoms(Combination.LENGTH, 0, allColors.Length);
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = (Color)allColors.GetValue(randoms[i]);
            }
            this.colors = colors;
        }

        private int[] GenerateRandoms(int take, int min, int max)
        {
            Random random = new Random();
            int[] randoms = new int[take];
            int i = 0;
            while(i < take)
            {
                var newRandom = random.Next(min, max);
                if (!randoms.Contains(newRandom))
                {
                    randoms[i] = newRandom;
                    i++;
                }
            }
            return randoms;
        }
    }
}
