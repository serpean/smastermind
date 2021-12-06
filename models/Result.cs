using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class Result
    {
        private int blacks { get; }
        private int whites { get; }

        public Result(int blacks, int whites)
        {
            this.blacks = blacks;
            this.whites = whites;
        }

        public bool match()
        {
            return blacks == Combination.LENGTH;
        }
    }
}
