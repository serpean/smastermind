using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SMastermind.models
{
    class Result
    {
        public int Blacks { get; }
        public int Whites { get; }

        public Result(int blacks, int whites)
        {
            this.Blacks = blacks;
            this.Whites = whites;
        }

        public bool Match()
        {
            return Blacks == Combination.LENGTH;
        }
    }
}
