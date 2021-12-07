using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class CurrentCombination : Combination
    {
        private int currentPos { get; set; }
        public CurrentCombination()
        {
            this.colors = new Color[LENGTH];
            this.currentPos = 0;
        }

        public bool AddColor(Color color)
        {
            if (currentPos < LENGTH)
            {
                this.colors[currentPos] = color;
                this.currentPos += 1;
                return true;
            }

            return false;
        }

        public bool RemoveColor()
        {
            if (currentPos > 0)
            {
                this.colors[currentPos - 1] = Color.NullColor;
                this.currentPos -= 1;
                return true;
            }
            return false;
        }

        public Error IsInvalid()
        {
            if (!IsComplete())
            {
                return Error.LengthInvalid;
            }
            foreach (var number in colors.GroupBy(x => x))
            {
                if (number.Count() > 1)
                {
                    return Error.ColorTwice;
                }
            }
            return Error.NullError;
        }

        public bool IsComplete()
        {
            return currentPos == LENGTH;
        }

        public ProposeCombination ToProposeCombination()
        {
            return new ProposeCombination(this.getColors());
        }

    }

    public enum Error
    {
        NullError,
        LengthInvalid,
        ColorTwice
    }
}
