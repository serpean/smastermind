using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class ProposeCombination : Combination
    {
        public ProposeCombination(Color[] colors)
        {
            this.colors = colors;
        }
    }
}
